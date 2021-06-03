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
using System.Timers;
using BaoStock;
using Initiator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QuickFix.Fields;
using QuickFix.FIX44;
using Server.Managers;
using Server.Models;
using Server.Security;
using Shared;
using Quote = Initiator.Quote;
using Timer = Server.Utilities.Timer;

namespace Server.Controllers {
	/// <summary>
	///     Controller for websocket request
	/// </summary>
	[ApiController]
	public sealed class WebSocketController : ControllerBase {
		/// <summary>
		/// </summary>
		/// <param name="configurationManager"></param>
		/// <param name="baoStock"></param>
		/// <param name="realtimeQuotesManager"></param>
		/// <param name="initiator"></param>
		/// <param name="settings"></param>
		public WebSocketController(ConfigurationManager configurationManager, BaoStockManager baoStock, RealtimeQuotesManager realtimeQuotesManager, StockQuotesInitiator initiator, JsonSerializerSettings settings) {
			ConfigurationManager = configurationManager;
			BaoStock = baoStock;
			RealtimeQuotesManager = realtimeQuotesManager;
			Initiator = initiator;
			SerializerSettings = settings;
			Initiator.Application.MessageReceived += (_, e) => {
				MsgType type = new();
				e.Message.Header.GetField(type);
				if (type.Obj != TDFData.MsgType)
					return;
				var message = new TDFData(e.Message);
				RealtimeQuotesManager.Add(message.WindCode.Obj, new Quote(message));
			};
		}

		/// <summary>
		/// </summary>
		public ConfigurationManager ConfigurationManager { get; }

		/// <summary>
		/// 
		/// </summary>
		public BaoStockManager BaoStock { get; }

		/// <summary>
		/// </summary>
		public RealtimeQuotesManager RealtimeQuotesManager { get; }

		/// <summary>
		/// </summary>
		public StockQuotesInitiator Initiator { get; }

		/// <summary>
		/// 
		/// </summary>
		public JsonSerializerSettings SerializerSettings { get; init; }

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[HttpGet("/api/quote/realtime/list")]
		[Authorize(AuthenticationSchemes = TokenQueryAuthenticationHandler.SchemeName)]
		public Task<IActionResult> UpdateQuotesList([FromQuery] [Required] string token)
			=> UpdateQuotes(
				token,
				(webSocket, config) => {
					var prices = RealtimeQuotesManager.GetList(token);
					if (prices == null || prices.Count == 0)
						return null;
					foreach (var price in prices)
						price.Pinned = config.PinnedStocks.Contains(price.Id);
					return webSocket.SendAsync(prices.ToArray());
				},
				config => config.RefreshInterval.List!.Value
			);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[HttpGet("/api/quote/realtime/trend")]
		[Authorize(AuthenticationSchemes = TokenQueryAuthenticationHandler.SchemeName)]
		public Task<IActionResult> UpdateQuotesTrend([FromQuery] [Required] string token, [FromQuery] [Required] string id)
			=> UpdateQuotes(
				token,
				(webSocket, config) => {
					var prices = RealtimeQuotesManager.GetTrend(token, id);
					if (prices == null || prices.Count == 0)
						return null;
					foreach (var price in prices)
						price.Pinned = config.PinnedStocks.Contains(id);
					return webSocket.SendAsync(prices.ToArray());
				},
				config => config.RefreshInterval.Trend!.Value
			);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="token"></param>
		/// <param name="begin"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		[HttpGet("/api/quote/playback/list")]
		[Authorize(AuthenticationSchemes = TokenQueryAuthenticationHandler.SchemeName)]
		public Task<IActionResult> ReplayQuotesList([FromQuery] [Required] string token, [FromQuery] DateTime? begin, [FromQuery] DateTime? end) {
			IEnumerator<MinutelyPrice>[] enumerators = null;
			var time = TimeSpan.FromMinutes(5 * Math.Max(114, (int)Math.Floor(begin!.Value.TimeOfDay.TotalMinutes / 5)));
			return ReplayQuotes(
				token,
				(webSocket, config) => {
					if (enumerators == null)
						return Task.CompletedTask;
					time = time.Add(TimeSpan.FromMinutes(5));
					var message = new List<MinutelyPrice>(enumerators.Length);
					bool finished = true;
					foreach (var enumerator in enumerators) {
						if (!enumerator.MoveNext())
							continue;
						finished = false;
						if (enumerator.Current!.Time!.Value.TimeOfDay <= time)
							message.Add(enumerator.Current);
					}
					if (finished)
						webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Playback Finished", CancellationToken.None);
					return webSocket.SendAsync(message);
				},
				config => TimeSpan.FromSeconds(300d / config.PlaybackSpeed!.Value),
				(content, type) => {
					if (type != WebSocketMessageType.Text)
						return Task.CompletedTask;
					try {
						string[] ids = JsonConvert.DeserializeObject<string[]>(content, SerializerSettings);
						enumerators = new IEnumerator<MinutelyPrice>[ids!.Length];
						for (int i = 0; i < ids.Length; ++i)
							enumerators[i] = BaoStock.Fetch<MinutelyPrice[]>("getMinutelyPrice", (StockId)ids[i], begin, end, 5).AsEnumerable().GetEnumerator();
					}
					catch (Exception) {
						// ignored
					}
					return Task.CompletedTask;
				}
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="token"></param>
		/// <param name="id"></param>
		/// <param name="begin"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		[HttpGet("/api/quote/playback/trend")]
		[Authorize(AuthenticationSchemes = TokenQueryAuthenticationHandler.SchemeName)]
		public Task<IActionResult> ReplayQuotesTrend([FromQuery] [Required] string token, [FromQuery] [Required] string id, [FromQuery] DateTime? begin, [FromQuery] DateTime? end) {
			var collection = BaoStock.Fetch<MinutelyPrice[]>("getMinutelyPrice", (StockId)id, begin, end, 5);
			var enumerator = collection.AsEnumerable().GetEnumerator();
			var time = TimeSpan.FromMinutes(5 * Math.Max(114, (int)Math.Floor(begin!.Value.TimeOfDay.TotalMinutes / 5)));
			bool first = true;
			return ReplayQuotes(
				token,
				(webSocket, config) => {
					time = time.Add(TimeSpan.FromMinutes(5));
					var message = new List<MinutelyPrice>(1);
					if (first) {
						while (enumerator.MoveNext() && enumerator.Current?.Time!.Value.TimeOfDay < time)
							message.Add(enumerator.Current);
						first = false;
					}
					else {
						bool finished = !enumerator.MoveNext();
						if (finished)
							webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Playback Finished", CancellationToken.None);
						else
							message.Add(enumerator.Current);
					}
					return webSocket.SendAsync(message);
				},
				config => TimeSpan.FromSeconds(300d / config.PlaybackSpeed!.Value)
			);
		}

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

		private async Task<IActionResult> UpdateQuotes(string token, Func<WebSocket, Configuration, Task> getTasks, Func<Configuration, TimeSpan> getInterval) {
			//Reject if not websocket
			if (!HttpContext.WebSockets.IsWebSocketRequest) {
				HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return BadRequest("Not a websocket request");
			}
			var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
			var messageSender = new Timer(_ => getInterval(ConfigurationManager[token])) {
				Immediate = true
			};
			//Indicate whether the last push has finished
			bool lastElapsedFinished = true;
			void Elapsed(object o, ElapsedEventArgs elapsedEventArgs) {
				if (!lastElapsedFinished || RealtimeQuotesManager?.Initialized != true)
					return;
				if (RealtimeQuotesManager?.Stopped == true) {
					messageSender.Close();
					return;
				}
				var task = getTasks(webSocket, ConfigurationManager[token]);
				task?.ContinueWith(_ => lastElapsedFinished = true);
			}
			messageSender.Elapsed += Elapsed;
			if (RealtimeQuotesManager.Initialized)
				messageSender.Start();
			var receive = webSocket.Listen(
				async (text, type) => {
					if (type != WebSocketMessageType.Text)
						return;
					try {
						string[] ids = JsonConvert.DeserializeObject<string[]>(text, SerializerSettings);
						await RealtimeQuotesManager.Initialize(ids);
						messageSender.Start();
					}
					catch (Exception) {
						// ignored
					}
				}
			);
			var send = Task.Run(
				async () => {
					while (!RealtimeQuotesManager.Initialized || RealtimeQuotesManager?.Stopped != true)
						await Task.Delay(TimeSpan.FromMinutes(1));
					return new WebSocketReceiveResult(0, WebSocketMessageType.Close, true, WebSocketCloseStatus.NormalClosure, "Trade Off");
				}
			);
			var result = (await Task.WhenAny(new[] {receive, send})).Result;
			messageSender.Close();
			await webSocket.CloseAsync(result.CloseStatus!.Value, result.CloseStatusDescription, CancellationToken.None);
			RealtimeQuotesManager.RemoveToken(token);
			return new EmptyResult();
		}

		private async Task<IActionResult> ReplayQuotes(string token, Func<WebSocket, Configuration, Task> sendMessage, Func<Configuration, TimeSpan> getInterval, Func<string, WebSocketMessageType, Task> receiveMessage = null) {
			//Reject if not websocket
			if (!HttpContext.WebSockets.IsWebSocketRequest) {
				HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return BadRequest("Not a websocket request");
			}
			var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
			var messageSender = new Timer(_ => getInterval(ConfigurationManager[token])) {
				Immediate = true
			};
			//Indicate whether the last push has finished
			bool lastElapsedFinished = true;
			void Elapsed(object o, ElapsedEventArgs elapsedEventArgs) {
				if (webSocket.CloseStatus.HasValue) {
					messageSender.Close();
					return;
				}
				if (!lastElapsedFinished)
					return;
				var task = sendMessage(webSocket, ConfigurationManager[token]);
				task?.ContinueWith(_ => lastElapsedFinished = true);
			}
			messageSender.Elapsed += Elapsed;
			messageSender.Start();
			var receive = webSocket.Listen(receiveMessage);
			var send = Task.Run(
				async () => {
					while (!webSocket.CloseStatus.HasValue)
						await Task.Delay(getInterval(ConfigurationManager[token]));
					return new WebSocketReceiveResult(0, WebSocketMessageType.Close, true, webSocket.CloseStatus, webSocket.CloseStatusDescription);
				}
			);
			var result = await Task.WhenAny(new[] {receive, send});
			messageSender.Close();
			if (result == receive)
				await webSocket.CloseAsync(result.Result.CloseStatus!.Value, result.Result.CloseStatusDescription, CancellationToken.None);
			return new EmptyResult();
		}
	}

	/// <summary>
	/// </summary>
	public static class WebSocketExtension {
		/// <summary>
		///     Combine multiple unended message
		/// </summary>
		/// <param name="webSocket"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="bufferSize"></param>
		/// <returns></returns>
		public static async Task<(WebSocketReceiveResult WebSocketResult, byte[] Body)> ReceiveAsync(this WebSocket webSocket, CancellationToken cancellationToken, int bufferSize = 1024) {
			byte[] buffer = new byte[bufferSize];
			int offset = 0;
			int free = buffer.Length;
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
				int newSize = buffer.Length + bufferSize;
				byte[] newBuffer = new byte[newSize];
				Array.Copy(buffer, 0, newBuffer, 0, offset);
				buffer = newBuffer;
				free = buffer.Length - offset;
			}
			return (finalResult, buffer);
		}

		/// <summary>
		///     Receive message and deserialize to T
		/// </summary>
		/// <typeparam name="T">Type to deserialize to</typeparam>
		/// <param name="webSocket"></param>
		/// <param name="settings"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static async Task<(WebSocketReceiveResult WebSocketResult, T body)> ReceiveAsync<T>(this WebSocket webSocket, Encoding encoding = null, JsonSerializerSettings settings = null, CancellationToken? cancellationToken = null) {
			cancellationToken ??= CancellationToken.None;
			encoding ??= Encoding.UTF8;
			(var result, byte[] body) = await webSocket.ReceiveAsync(cancellationToken!.Value);
			return (result, JsonConvert.DeserializeObject<T>(encoding.GetString(body), settings));
		}

		/// <summary>
		///     Send typed object using Json serializer
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
		///     Recursively receive messages until websocket is closed
		/// </summary>
		/// <param name="webSocket"></param>
		/// <param name="onReceived"></param>
		/// <returns></returns>
		public static async Task<WebSocketReceiveResult> Listen(this WebSocket webSocket, Action<string, WebSocketMessageType> onReceived = null) {
			(var result, byte[] buffer) = await webSocket.ReceiveAsync(CancellationToken.None);
			onReceived?.Invoke(Encoding.UTF8.GetString(buffer), result.MessageType);
			if (!result.CloseStatus.HasValue)
				result = await webSocket.Listen(onReceived);
			return result;
		}

		/// <summary>
		/// </summary>
		/// <param name="webSocket"></param>
		/// <param name="onReceived"></param>
		/// <returns></returns>
		public static async Task<WebSocketReceiveResult> Listen(this WebSocket webSocket, Func<string, WebSocketMessageType, Task> onReceived = null) {
			(var result, byte[] buffer) = await webSocket.ReceiveAsync(CancellationToken.None);
			if (onReceived != null)
				await onReceived(Encoding.UTF8.GetString(buffer), result.MessageType);
			if (!result.CloseStatus.HasValue)
				result = await webSocket.Listen(onReceived);
			return result;
		}
	}
}