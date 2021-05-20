using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Initiator;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QuickFix.FIX44;
using Server.Managers;
using Quote = Initiator.Quote;
using Timer = System.Timers.Timer;

namespace Server.Controllers {
	/// <summary>
	/// </summary>
	[ApiController]
	public class WebSocketController : ControllerBase {
		/// <summary>
		/// </summary>
		/// <param name="configManager"></param>
		/// <param name="initiator"></param>
		public WebSocketController(ConfigManager configManager, StockQuotesInitiator initiator) {
			ConfigManager = configManager;
			Initiator = initiator;
			Initiator.Application.MessageReceived += (sender, e) => {
				var message = new TDFData(e.Message);
				StockManger.Add(message.WindCode.Obj, new Quote(message));
			};
		}

		/// <summary>
		/// </summary>
		public ConfigManager ConfigManager { get; }

		/// <summary>
		/// </summary>
		public StockManager StockManger { get; private set; }

		/// <summary>
		/// </summary>
		public StockQuotesInitiator Initiator { get; }

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
			foreach (var row in rows) {
				var parts = row.Split('=').ToArray();
				var id = parts[0][^8..];
				var content = parts[1][1..^3];
				parts = content.Split(',').ToArray();
				var price = new Quote {
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
				};
				result.Add(price);
			}
			return result;
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[HttpGet("/api/ws/stock/list")]
		//[Authorize(AuthenticationSchemes = TokenQueryAuthenticationHandler.SchemeName)]
		public virtual async Task<IActionResult> StartStockListUpdating([FromQuery] [Required] string token) {
			if (!HttpContext.WebSockets.IsWebSocketRequest) {
				HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return BadRequest("Not a websocket request");
			}
			var config = ConfigManager[token];
			var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
			var messageSender = new Timer {
				Interval = config.RefreshInterval.Single!.Value,
				AutoReset = false
			};
			var lastElapsedFinished = false;
			messageSender.Elapsed += (sender, args) => {
				if (!lastElapsedFinished || StockManger == null)
					goto ResetTimer;
				var prices = StockManger.GetList(token);
				if (prices.Count == 0)
					goto ResetTimer;
				List<Task> tasks = new(prices.Count);
				foreach (var price in prices) {
					price.Pinned = config.PinnedStocks.Contains(price.Id);
					tasks.Add(webSocket.SendAsync(price));
				}
				Task.WhenAll(tasks).ContinueWith(_ => lastElapsedFinished = true);
			ResetTimer:
				messageSender.Interval = config.RefreshInterval.Single!.Value;
				messageSender.Start();
			};
			messageSender.Start();
			var result = await webSocket.Listen(
				(text, type) => {
					try {
						var ids = JsonConvert.DeserializeObject<string[]>(text);
						StockManger = new StockManager(ids);
					}
					catch (Exception) { }
				}
			);
			messageSender.Close();
			await webSocket.CloseAsync(result.CloseStatus!.Value, result.CloseStatusDescription, CancellationToken.None);
			return new EmptyResult();
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[HttpGet("/api/ws/stock")]
		//[Authorize(AuthenticationSchemes = TokenQueryAuthenticationHandler.SchemeName)]
		public virtual async Task<IActionResult> StartStockUpdating([FromQuery] [Required] string token, [FromQuery] [Required] string id) {
			if (!HttpContext.WebSockets.IsWebSocketRequest) {
				HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return BadRequest("Not a websocket request");
			}
			var config = ConfigManager[token];
			var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
			var messageSender = new Timer {
				Interval = config.RefreshInterval.Single!.Value,
				AutoReset = false
			};
			var lastElapsedFinished = false;
			messageSender.Elapsed += (sender, args) => {
				if (!lastElapsedFinished || StockManger == null)
					goto ResetTimer;
				var prices = StockManger.GetSingle(token, id);
				if (prices.Count == 0)
					goto ResetTimer;
				List<Task> tasks = new(prices.Count);
				foreach (var price in prices) {
					price.Pinned = config.PinnedStocks.Contains(id);
					tasks.Add(webSocket.SendAsync(price));
				}
				Task.WhenAll(tasks).ContinueWith(_ => lastElapsedFinished = true);
			ResetTimer:
				messageSender.Interval = config.RefreshInterval.Single!.Value;
				messageSender.Start();
			};
			messageSender.Start();
			var result = await webSocket.Listen(
				(text, type) => {
					try {
						var ids = JsonConvert.DeserializeObject<string[]>(text);
						StockManger = new StockManager(ids);
					}
					catch (Exception) { }
				}
			);
			messageSender.Close();
			await webSocket.CloseAsync(result.CloseStatus!.Value, result.CloseStatusDescription, CancellationToken.None);
			return new EmptyResult();
		}
	}

	/// <summary>
	/// </summary>
	public static class WebSocketExtension {
		/// <summary>
		/// </summary>
		/// <param name="webSocket"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="bufferSize"></param>
		/// <returns></returns>
		public static async Task<(WebSocketReceiveResult WebSocketResult, byte[] Body)> ReceiveAsync(this WebSocket webSocket, CancellationToken cancellationToken, int bufferSize = 1024) {
			var buffer = new byte[bufferSize];
			var offset = 0;
			var free = buffer.Length;
			WebSocketReceiveResult finalResult;
			while (true) {
				var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, free), cancellationToken);
				offset += result.Count;
				free -= result.Count;
				if (result.EndOfMessage) {
					finalResult = new WebSocketReceiveResult(buffer.Length - free, result.MessageType, true, result.CloseStatus, result.CloseStatusDescription);
					break;
				}
				if (free > 0)
					continue;
				var newSize = buffer.Length + bufferSize;
				var newBuffer = new byte[newSize];
				Array.Copy(buffer, 0, newBuffer, 0, offset);
				buffer = newBuffer;
				free = buffer.Length - offset;
			}
			return (finalResult, buffer);
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="webSocket"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static async Task<(WebSocketReceiveResult WebSocketResult, T body)> ReceiveAsync<T>(this WebSocket webSocket, Encoding encoding = null, CancellationToken? cancellationToken = null) {
			cancellationToken ??= CancellationToken.None;
			encoding ??= Encoding.UTF8;
			var (result, body) = await webSocket.ReceiveAsync(cancellationToken!.Value);
			return (result, JsonConvert.DeserializeObject<T>(encoding.GetString(body)));
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="webSocket"></param>
		/// <param name="body"></param>
		/// <param name="encoding">Default is UTF8</param>
		/// <param name="messageType">Default is WebSocketMessageType.Text</param>
		/// <param name="endOfMessage">Default is true</param>
		/// <param name="cancellationToken">Default is CancellationToken.None</param>
		/// <returns></returns>
		public static Task SendAsync<T>(this WebSocket webSocket, T body, Encoding encoding = null, WebSocketMessageType messageType = WebSocketMessageType.Text, bool endOfMessage = true, CancellationToken? cancellationToken = null) {
			encoding ??= Encoding.UTF8;
			cancellationToken ??= CancellationToken.None;
			return webSocket.SendAsync(encoding.GetBytes(JsonConvert.SerializeObject(body)), messageType, endOfMessage, cancellationToken!.Value);
		}

		/// <summary>
		/// </summary>
		/// <param name="webSocket"></param>
		/// <param name="OnReceived"></param>
		/// <returns></returns>
		public static async Task<WebSocketReceiveResult> Listen(this WebSocket webSocket, Action<string, WebSocketMessageType> OnReceived = null) {
			var (result, buffer) = await webSocket.ReceiveAsync(CancellationToken.None);
			OnReceived?.Invoke(Encoding.UTF8.GetString(buffer), result.MessageType);
			if (!result.CloseStatus.HasValue)
				result = await webSocket.Listen(OnReceived);
			return result;
		}
	}
}