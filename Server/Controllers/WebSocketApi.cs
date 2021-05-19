using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Initiator;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Managers;
using Server.Models;
using Timer = System.Timers.Timer;

namespace Server.Controllers {
	/// <summary>
	/// </summary>
	[ApiController]
	public class WebSocketController : ControllerBase {
		/// <summary>
		/// </summary>
		/// <param name="manager"></param>
		/// <param name="initiator"></param>
		public WebSocketController(ConfigManager manager, StockQuotesInitiator initiator) {
			ConfigManager = manager;
			Initiator = initiator;
		}

		/// <summary>
		/// </summary>
		private Dictionary<string, Dictionary<string, int>> ListOffset { get; } = new();

		/// <summary>
		/// </summary>
		private Dictionary<string, int> SingleOffset { get; } = new();

		/// <summary>
		/// </summary>
		public ConfigManager ConfigManager { get; }

		/// <summary>
		/// </summary>
		public StockQuotesInitiator Initiator { get; }

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
			var listOffset = ListOffset[token] = new Dictionary<string, int>(Initiator.Stocks.Count);
			foreach (var stock in Initiator.Stocks)
				listOffset[stock.Id] = 0;
			var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
			var messageSender = new Timer {
				Interval = config.RefreshInterval.Single!.Value,
				AutoReset = false
			};
			var lastElapsedFinished = false;
			messageSender.Elapsed += (sender, args) => {
				if (!lastElapsedFinished)
					goto ResetTimer;
				List<Task> tasks = new();
				foreach (var stock in Initiator.Stocks) {
					if (!listOffset.ContainsKey(stock.Id))
						listOffset[stock.Id] = 0;
					var offset = listOffset[stock.Id];
					var count = stock.Quotes.Count;
					if (offset >= count)
						continue;
					for (var i = offset; i < count; ++i) {
						var quote = stock.Quotes[i];
						tasks.Add(
							webSocket.SendAsync(
								new RealTimePrice {
									Id = stock.Id,
									Opening = (int)quote.OpeningPrice,
									Closing = (int)quote.ClosingPrice,
									Highest = (int)quote.HighestPrice,
									Lowest = (int)quote.LowestPrice,
									Volume = quote.TotalVolume,
									Turnover = (long)quote.TotalTurnover,
									Time = quote.TradingTime,
									PreClosing = (int)quote.PreClosingPrice,
									Pinned = config.PinnedStocks.Contains(stock.Id)
								}
							)
						);
						listOffset[stock.Id] = count;
					}
				}
				if (tasks.Count == 0)
					goto ResetTimer;
				Task.WhenAll(tasks)
					.ContinueWith(_ => lastElapsedFinished = true);
			ResetTimer:
				messageSender.Interval = config.RefreshInterval.Single!.Value;
				messageSender.Start();
			};
			messageSender.Start();
			var result = await webSocket.Listen();
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
			Initiator.Stocks.TryGetValue(new Stock(id), out var stock);
			var config = ConfigManager[token];
			SingleOffset[token] = 0;
			var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
			var messageSender = new Timer {
				Interval = config.RefreshInterval.Single!.Value,
				AutoReset = false
			};
			var lastElapsedFinished = false;
			messageSender.Elapsed += (sender, args) => {
				var offset = SingleOffset[token];
				if (lastElapsedFinished && (stock != null || Initiator.Stocks.TryGetValue(new Stock(id), out stock)) && stock.Quotes.Count is var count && count > offset) {
					List<Task> tasks = new(count - offset);
					for (var i = offset; i < count; ++i) {
						var quote = stock.Quotes[i];
						tasks[i - offset] = webSocket.SendAsync(
							new RealTimePrice {
								Id = id,
								Opening = (int)quote.OpeningPrice,
								Closing = (int)quote.ClosingPrice,
								Highest = (int)quote.HighestPrice,
								Lowest = (int)quote.LowestPrice,
								Volume = quote.TotalVolume,
								Turnover = (long)quote.TotalTurnover,
								Time = quote.TradingTime,
								PreClosing = (int)quote.PreClosingPrice,
								Pinned = config.PinnedStocks.Contains(id)
							}
						);
					}
					SingleOffset[token] = count;
					Task.WhenAll(tasks)
						.ContinueWith(_ => lastElapsedFinished = true);
				}
				messageSender.Interval = config.RefreshInterval.Single!.Value;
				messageSender.Start();
			};
			messageSender.Start();
			var result = await webSocket.Listen();
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
				var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, free), CancellationToken.None);
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