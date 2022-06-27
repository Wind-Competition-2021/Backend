using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using Server.Models;
using Shared;
using Tushare;
using Timer = Server.Utilities.Timer;
using Console = Colorful.Console;

namespace Server.Managers {
	/// <summary>
	///     Manager of real time stock quotes
	/// </summary>
	public class RealtimeQuotesManager {
		private bool _isTradeDay = true;
		private bool _lastElapsedFinished = true;

		/// <summary>
		///     Default interval is 1 minutes
		/// </summary>
		public RealtimeQuotesManager(TushareManager tushare) : this(TimeSpan.FromMinutes(1), tushare) { }

		/// <summary>
		/// </summary>
		/// <param name="interval">Interval to sync with public quote server</param>
		/// <param name="tushare"></param>
		public RealtimeQuotesManager(TimeSpan interval, TushareManager tushare) {
			Tushare = tushare;
			void Elapsed(object sender, ElapsedEventArgs e) {
				if (TradeOff)
					Synchronizer.Stop();
				if (!_lastElapsedFinished)
					return;
				var tasks = new List<Task>(Quotes.Count);
				foreach (var stock in Quotes) {
					string id = stock.Key;
					var (playBack, recent) = stock.Value;
					tasks.Add(
						GetRealTimeQuote(id)
							.ContinueWith(
								task => {
									var result = task.Result;
									if (result.Count > 0 &&
										result[0]
											.TradingTime >
										playBack[^1]
											.TradingTime)
										playBack.AddRange(result);
									recent.Clear();
								}
							)
					);
				}
				_lastElapsedFinished = false;
				Task.WhenAll(tasks)
					.ContinueWith(
						_ => _lastElapsedFinished = true
					);
			}
			Synchronizer = new Timer(interval.TotalMilliseconds) {
				Immediate = true
			};
			Synchronizer.Elapsed += Elapsed;
		}

		/// <summary>
		///     Whether the manager has been initialized with stock list
		/// </summary>
		public bool Initialized { get; private set; }

		/// <summary>
		///     Whether stock trading is off
		/// </summary>
		public bool TradeOff {
			get {
				#if MOCK
				return true
				#else
				return DateTime.Now.TimeOfDay.TotalHours is < 9.5 or > 15 || !_isTradeDay;
				#endif
			}
		}

		/// <summary>
		///     Whether the manager has stopped due to trade off
		/// </summary>
		public bool Stopped => !Synchronizer.Enabled;

		/// <summary>
		///     Timer for synchronization
		/// </summary>
		protected Timer Synchronizer { get; init; }

		/// <summary>
		/// </summary>
		protected TushareManager Tushare { get; }

		/// <summary>
		///     Realtime quotes
		/// </summary>
		protected Dictionary<string, (List<Quote> PlayBack, List<Quote> Recent)> Quotes { get; } = new();

		/// <summary>
		///     Record the last websocket push time of a user on single stock trend chart
		/// </summary>
		protected Dictionary<string, DateTime> LastTrendPushTime { get; } = new();

		/// <summary>
		///     Record the last websocket push time of a user on stock list
		/// </summary>
		protected Dictionary<string, DateTime> LastListPushTime { get; } = new();

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
		public void RemoveToken(string token) {
			LastTrendPushTime.Remove(token);
			LastListPushTime.Remove(token);
		}

		/// <summary>
		///     Initialize the manager with stock list
		/// </summary>
		/// <param name="ids">Stock list</param>
		/// <returns></returns>
		public async Task Initialize(string[] ids) {
			foreach (string id in ids)
                if (!Quotes.ContainsKey(id))
                    Quotes.Add(id, (new List<Quote>(), new List<Quote>()));
			_isTradeDay = await Tushare.CheckTradeStatus();
			var now = DateTime.Now;
			//Update trade day status at 0:00
			var statusUpdater = new Timer(now.Date.AddDays(1) - now, TimeSpan.FromDays(1));
			statusUpdater.Elapsed += (_, _) => Tushare.CheckTradeStatus()
				.ContinueWith(
					task => {
						_isTradeDay = task.Result;
						Console.WriteLine($"{DateTime.Now:yyyy-MM-dd hh:mm:ss}: Today {(task.Result ? "is" : "isn't")} trade day", Color.Magenta);
					}
				);
			statusUpdater.Start();
			//Restart synchronizer when trade is on
			var restarter = new Timer((now.TimeOfDay.TotalHours >= 9.5 ? now.Date.AddDays(1) : now.Date).AddHours(9.5) - now, TimeSpan.FromDays(1));
			restarter.Elapsed += (_, _) => {
				if (!Synchronizer.Enabled && _isTradeDay) {
					Synchronizer.Start();
					Console.WriteLine($"{DateTime.Now:yyyy-MM-dd hh:mm:ss}: Synchronizer restarted", Color.Yellow);
				}
			};
			restarter.Start();
			await GetRealTimeQuote(ids)
				.ContinueWith(
					task => {
						var result = task.Result;
						for (int i = 0; i < ids.Length; ++i)
							Quotes[ids[i]].PlayBack.Add(result[i]);
						Synchronizer.Start();
						Initialized = true;
					}
				);
		}

		/// <summary>
		///     Send get request
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="headers"></param>
		/// <returns></returns>
		public static async Task<string> GetAsync(string uri, WebHeaderCollection headers = null) {
			var request = (HttpWebRequest)WebRequest.Create(uri);
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			if (headers is not null)
				request.Headers = headers;
			using var response = (HttpWebResponse)await request.GetResponseAsync();
			await using var stream = response.GetResponseStream();
			using var reader = new StreamReader(stream!);
			return await reader.ReadToEndAsync();
		}

		/// <summary>
		///     Get realtime quote from sina server
		/// </summary>
		/// <param name="ids">Stock list</param>
		/// <returns></returns>
		public static async Task<List<Quote>> GetRealTimeQuote(params string[] ids) {
			#if MOCK
			var uri = "http://localhost:8520/?list=" + string.Join(',', ids.Select(id => id[..2] + id[3..]));
			#else
			string uri = "http://hq.sinajs.cn/list=" + string.Join(',', ids.Select(id => id[..2] + id[3..]));
			#endif
			string raw = await GetAsync(uri, new WebHeaderCollection {
				{"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.53 Safari/537.36 Edg/103.0.1264.37"},
				{"Referer", "https://finance.sina.com.cn"}
			});
			var rows = raw.Split('\n', '\r')
				.Where(row => !string.IsNullOrEmpty(row))
				.ToList();
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
		/// <param name="token"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public List<RealtimePrice> GetTrend(string token, string id) {
			if (!Initialized || Stopped)
				return null;
			var now = DateTime.Now;
			if (!LastTrendPushTime.ContainsKey(token)) {
				LastTrendPushTime[token] = now;
				return Quotes[id]
					.PlayBack.Select(quote => new RealtimePrice(quote, id))
					.ToList();
			}
			var time = LastTrendPushTime[token];
			var result = Quotes[id]
				.Recent.Concat(Quotes[id].PlayBack)
				.Where(quote => quote.TradingTime >= time)
				.Select(quote => new RealtimePrice(quote, id))
				.ToList();

			if (result.Count > 0)
				LastTrendPushTime[token] = now;
			return result;
		}

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public List<RealtimePrice> GetList(string token) {
			if (!Initialized || Stopped)
				return null;
			LastListPushTime[token] = DateTime.Now;
			return Quotes.Select(
					quotes => {
						(string id, var (playBack, recent)) = quotes;
						var quote = recent.Count > 0
							? recent.Last()
							: playBack.LastOrDefault();
						return new RealtimePrice(quote, id);
					}
				)
				.ToList();
		}
	}
}