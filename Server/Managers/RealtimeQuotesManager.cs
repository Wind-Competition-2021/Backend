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
	///     Manager of real time stock quotes
	/// </summary>
	public class RealtimeQuotesManager {
		/// <summary>
		///     Default interval is 5 minutes
		/// </summary>
		public RealtimeQuotesManager() : this(TimeSpan.FromSeconds(5)) { }

		/// <summary>
		/// </summary>
		/// <param name="interval">Interval to sync with public quote server</param>
		public RealtimeQuotesManager(TimeSpan interval) => Timer = new Timer(interval.TotalMilliseconds);

		/// <summary>
		///     Whether the manager has been initialized with stock list
		/// </summary>
		public bool Initialized { get; private set; }

		/// <summary>
		///     Whether the manager has stopped due to trade off
		/// </summary>
		public bool Stopped { get; private set; }

		/// <summary>
		///     Timer for synchronization
		/// </summary>
		protected Timer Timer { get; init; }

		/// <summary>
		///     Realtime quotes
		/// </summary>
		protected Dictionary<string, (List<Quote> PlayBack, List<Quote> Recent)> Quotes { get; } = new();

		/// <summary>
		///     Record the last websocket push time of a user on single stock trend chart
		/// </summary>
		protected Dictionary<string, DateTime> LastSinglePushTime { get; } = new();

		/// <summary>
		///     Record the last websocket push time of a user on stock list
		/// </summary>
		protected Dictionary<string, DateTime> LastListPushTime { get; } = new();

		/// <summary>
		///     Send get request
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
		///     Get realtime quote from sina server
		/// </summary>
		/// <param name="ids">Stock list</param>
		/// <returns></returns>
		public static async Task<List<Quote>> GetRealTimeQuote(params string[] ids) {
			//Local mocked server
			var uri = "http://localhost:8520/?list=" + string.Join(',', ids.Select(id => id[..2] + id[3..]));
			//var uri = "http://hq.sinajs.cn/list=" + string.Join(',', ids.Select(id => id[..2] + id[3..]));
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
		///     Initialize the manager with stock list
		/// </summary>
		/// <param name="ids">Stock list</param>
		/// <returns></returns>
		public async Task Initialize(string[] ids) {
			foreach (var id in ids)
				Quotes.Add(id, (new List<Quote>(), new List<Quote>()));
			var lastElapsedFinished = true;
			void Elapsed(object sender, ElapsedEventArgs e) {
				if (DateTime.Now.TimeOfDay < TimeSpan.FromHours(9.5) || DateTime.Now.TimeOfDay > TimeSpan.FromHours(23.9)) {
					Timer.Stop();
					Stopped = true;
				}
				if (!lastElapsedFinished)
					return;
				var tasks = new List<Task>(Quotes.Count);
				foreach (var stock in Quotes) {
					var id = stock.Key;
					var (playBack, recent) = stock.Value;
					tasks.Add(
						GetRealTimeQuote(id)
							.ContinueWith(
								task => {
									var result = task.Result;
									playBack.AddRange(result);
									recent.Clear();
								}
							)
					);
				}
				lastElapsedFinished = false;
				Task.WhenAll(tasks)
					.ContinueWith(
						_ =>
							lastElapsedFinished = true
					);
			}
			Timer.Elapsed += Elapsed;
			await GetRealTimeQuote(ids)
				.ContinueWith(
					task => {
						var result = task.Result;
						for (var i = 0; i < ids.Length; ++i)
							Quotes[ids[i]].PlayBack.Add(result[i]);
						Timer.Start();
						Initialized = true;
					}
				);
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
		public List<RealtimePrice> GetSingle(string token, string id) {
			if (!Initialized || Stopped)
				return null;
			var now = DateTime.Now;
			if (!LastSinglePushTime.ContainsKey(token)) {
				LastSinglePushTime[token] = now;
				return Quotes[id]
					.PlayBack.Select(quote => new RealtimePrice(quote, id))
					.ToList();
			}
			var time = LastSinglePushTime[token];
			var result = Quotes[id]
				.Recent.Concat(Quotes[id].PlayBack)
				.Where(quote => quote.TradingTime >= time)
				.Select(quote => new RealtimePrice(quote, id))
				.ToList();

			if (result.Count > 0)
				LastSinglePushTime[token] = now;
			return result;
		}

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public List<RealtimePrice> GetList(string token) {
			if (!Initialized || Stopped)
				return null;
			var now = DateTime.Now;
			if (!LastListPushTime.ContainsKey(token)) {
				LastListPushTime[token] = now;
				return Quotes.Select(
						quotes => {
							var (id, (playBack, recent)) = quotes;
							var quote = recent.Count > 0
								? recent.Last()
								: playBack.LastOrDefault();
							return new RealtimePrice(quote, id);
						}
					)
					.ToList();
			}
			var time = LastListPushTime[token];
			var result = Quotes
				.Where(quotes => quotes.Value.Recent.LastOrDefault()?.TradingTime >= time)
				.Select(
					quotes => {
						var (id, value) = quotes;
						var quote = value.Recent.Last();
						return new RealtimePrice(quote, id);
					}
				)
				.ToList();
			if (result.Count == 0)
				result = Quotes
					.Where(quotes => quotes.Value.PlayBack.LastOrDefault()?.TradingTime >= time)
					.Select(
						quotes => {
							var (id, value) = quotes;
							var quote = value.PlayBack.Last();
							return new RealtimePrice(quote, id);
						}
					)
					.ToList();
			if (result.Count > 0)
				LastListPushTime[token] = now;
			return result;
		}
	}
}