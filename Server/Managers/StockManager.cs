using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using Initiator;
using Server.Models;

namespace Server.Managers {
	/// <summary>
	/// </summary>
	public class StockManager {
		/// <summary>
		/// </summary>
		/// <param name="ids"></param>
		public StockManager(IEnumerable<string> ids) : this(ids, TimeSpan.FromMinutes(5)) { }

		/// <summary>
		/// </summary>
		/// <param name="ids"></param>
		/// <param name="interval"></param>
		public StockManager(IEnumerable<string> ids, TimeSpan interval) {
			foreach (var id in ids)
				Quotes.Add(id, (new List<Quote>(), new List<Quote>()));
			Timer = new Timer(interval.TotalMilliseconds);
			var lastElapsedFinished = true;
			void Elapsed(object sender, ElapsedEventArgs e) {
				if (DateTime.Now.TimeOfDay < TimeSpan.FromHours(9.5) || DateTime.Now.TimeOfDay > TimeSpan.FromHours(15))
					Timer.Stop();
				if (!lastElapsedFinished)
					return;
				var tasks = new List<Task>(Quotes.Count);
				foreach (var stock in Quotes) {
					var id = stock.Key;
					var (playBack, recent) = stock.Value;
					tasks.Add(
						new Task(
							async () => {
								recent.Clear();
								playBack.AddRange(await GetRealTimeQuote(id));
							}
						)
					);
				}
				lastElapsedFinished = false;
				Task.WhenAll(tasks).ContinueWith(_ => lastElapsedFinished = true);
			}
			Timer.Elapsed += Elapsed;
			Elapsed(null, null);
			Timer.Start();
		}

		/// <summary>
		/// </summary>
		protected Timer Timer { get; init; }

		/// <summary>
		/// </summary>
		protected Dictionary<string, (List<Quote> PlayBack, List<Quote> Recent)> Quotes { get; } = new();

		/// <summary>
		/// </summary>
		protected Dictionary<string, DateTime> LastSinglePushTime { get; } = new();

		/// <summary>
		/// </summary>
		protected Dictionary<string, DateTime> LastListPushTime { get; } = new();

		/// <summary>
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		public static async Task<string> GetAsync(string uri) {
			var request = (HttpWebRequest)WebRequest.Create(uri);
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			using var response = (HttpWebResponse)await request.GetResponseAsync();
			await using var stream = response.GetResponseStream();
			using var reader = new StreamReader(stream);
			return await reader.ReadToEndAsync();
		}

		/// <summary>
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public static async Task<List<Quote>> GetRealTimeQuote(params string[] ids) {
			var uri = "http://hq.sinajs.cn/list=" + string.Join(',', ids.Select(id => id[..2] + id[3..]));
			var raw = await GetAsync(uri);
			var rows = raw.Split('\n', '\r').Where(row => !string.IsNullOrEmpty(row)).ToList();
			var result = new List<Quote>(rows.Count);
			result.AddRange(
				from row in rows
				select row.Split('=').ToArray()
				into parts
				let id = parts[0][^8..]
				let content = parts[1][1..^3]
				select content.Split(',').ToArray()
				into parts
				select new Quote {
					OpeningPrice = Convert.ToDecimal(parts[1]),
					PreClosingPrice = Convert.ToDecimal(parts[2]),
					ClosingPrice = Convert.ToDecimal(parts[3]),
					HighestPrice = Convert.ToDecimal(parts[4]),
					LowestPrice = Convert.ToDecimal(parts[5]),
					TotalVolume = Convert.ToInt64(parts[8]),
					TotalTurnover = Convert.ToDecimal(parts[9]),
					AskPrices = new[] {Convert.ToDecimal(parts[11]), Convert.ToDecimal(parts[13]), Convert.ToDecimal(parts[15]), Convert.ToDecimal(parts[17]), Convert.ToDecimal(parts[19])},
					AskVolumes = new[] {Convert.ToInt64(parts[10]), Convert.ToInt64(parts[12]), Convert.ToInt64(parts[14]), Convert.ToInt64(parts[16]), Convert.ToInt64(parts[18])},
					BidPrices = new[] {Convert.ToDecimal(parts[21]), Convert.ToDecimal(parts[23]), Convert.ToDecimal(parts[25]), Convert.ToDecimal(parts[27]), Convert.ToDecimal(parts[29])},
					BidVolumes = new[] {Convert.ToInt64(parts[20]), Convert.ToInt64(parts[22]), Convert.ToInt64(parts[24]), Convert.ToInt64(parts[26]), Convert.ToInt64(parts[28])},
					TradingTime = DateTime.Parse($"{parts[30]}T{parts[31]}")
				}
			);
			return result;
		}

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool Contains(string id) => Quotes.ContainsKey(id);

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="quote"></param>
		public void Add(string id, Quote quote) => Quotes[id].Recent.Add(quote);

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public List<RealTimePrice> GetSingle(string token, string id) {
			var now = DateTime.Now;
			if (!LastSinglePushTime.ContainsKey(token)) {
				LastSinglePushTime[token] = now;
				return Quotes[id]
					.PlayBack.Select(quote => new RealTimePrice(quote, id))
					.ToList();
			}
			LastSinglePushTime[token] = now;
			return Quotes[id]
				.Recent
				.Where(quote => quote.TradingTime >= now)
				.Select(quote => new RealTimePrice(quote, id))
				.ToList();
		}

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public List<RealTimePrice> GetList(string token) {
			var now = DateTime.Now;
			if (!LastListPushTime.ContainsKey(token)) {
				LastListPushTime[token] = now;
				return Quotes.Select(
						quotes => {
							var (id, (playBack, recent)) = quotes;
							var quote = recent.Count > 0
								? recent.Last()
								: playBack.LastOrDefault();
							return new RealTimePrice(quote, id);
						}
					)
					.ToList();
			}
			LastListPushTime[token] = now;
			return Quotes
				.Where(quotes => quotes.Value.Recent.LastOrDefault()?.TradingTime >= now)
				.Select(
					quotes => {
						var (id, value) = quotes;
						var quote = value.Recent.Last();
						return new RealTimePrice(quote, id);
					}
				)
				.ToList();
		}
	}
}