﻿using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Initiator;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QuickFix.Fields;
using QuickFix.FIX44;
using Server.Managers;
using Server.Models;
using Quote = Initiator.Quote;
using Timer = System.Timers.Timer;

namespace Server.Controllers {
	/// <summary>
	///     Controller for websocket request
	/// </summary>
	[ApiController]
	public sealed class WebSocketController : ControllerBase {
		/// <summary>
		/// </summary>
		/// <param name="configurationManager"></param>
		/// <param name="realtimeQuotesManager"></param>
		/// <param name="initiator"></param>
		public WebSocketController(ConfigurationManager configurationManager, RealtimeQuotesManager realtimeQuotesManager, StockQuotesInitiator initiator) {
			ConfigurationManager = configurationManager;
			RealtimeQuotesManager = realtimeQuotesManager;
			Initiator = initiator;
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
		/// </summary>
		public RealtimeQuotesManager RealtimeQuotesManager { get; }

		/// <summary>
		/// </summary>
		public StockQuotesInitiator Initiator { get; }

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[HttpGet("/api/ws/stock/list")]
		//[Authorize(AuthenticationSchemes = TokenQueryAuthenticationHandler.SchemeName)]
		public Task<IActionResult> UpdateStockList([FromQuery] [Required] string token)
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
				config => config.RefreshInterval.List!.Value * 1000
			);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[HttpGet("/api/ws/stock")]
		//[Authorize(AuthenticationSchemes = TokenQueryAuthenticationHandler.SchemeName)]
		public Task<IActionResult> UpdateStock([FromQuery] [Required] string token, [FromQuery] [Required] string id)
			=> UpdateQuotes(
				token,
				(webSocket, config) => {
					var prices = RealtimeQuotesManager.GetSingle(token, id);
					if (prices == null || prices.Count == 0)
						return null;
					foreach (var price in prices)
						price.Pinned = config.PinnedStocks.Contains(id);
					return webSocket.SendAsync(prices.ToArray());
				},
				config => config.RefreshInterval.Single!.Value * 1000
			);

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

		private async Task<IActionResult> UpdateQuotes(string token, Func<WebSocket, Configuration, Task> getTasks, Func<Configuration, int> getInterval) {
			//Reject if not websocket
			if (!HttpContext.WebSockets.IsWebSocketRequest) {
				HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return BadRequest("Not a websocket request");
			}
			var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
			var messageSender = new Timer {
				Interval = getInterval(ConfigurationManager[token]),
				AutoReset = false
			};
			//Indicate whether the last push has finished
			var lastElapsedFinished = true;
			void Elapsed(object o, ElapsedEventArgs elapsedEventArgs) {
				if (RealtimeQuotesManager?.Stopped == true)
					return;
				if (!lastElapsedFinished || RealtimeQuotesManager?.Initialized != true)
					goto ResetTimer;
				var task = getTasks(webSocket, ConfigurationManager[token]);
				task?.ContinueWith(_ => lastElapsedFinished = true);
			ResetTimer:
				messageSender.Interval = getInterval(ConfigurationManager[token]);
				messageSender.Start();
			}
			messageSender.Elapsed += Elapsed;
			Elapsed(null, null);
			var receive = webSocket.Listen(
				async (text, type) => {
					if (type != WebSocketMessageType.Text)
						return;
					try {
						var ids = JsonConvert.DeserializeObject<string[]>(text);
						await RealtimeQuotesManager.Initialize(ids);
					}
					catch (Exception) {
						// ignored
					}
				}
			);
			var send = Task.Run(
				async () => {
					while (RealtimeQuotesManager?.Stopped != true)
						await Task.Delay(TimeSpan.FromMinutes(1));
					return new WebSocketReceiveResult(0, WebSocketMessageType.Close, true, WebSocketCloseStatus.NormalClosure, "Trade Off");
				}
			);
			var result = (await Task.WhenAny(new[] {receive, send})).Result;
			messageSender.Close();
			await webSocket.CloseAsync(result.CloseStatus!.Value, result.CloseStatusDescription, CancellationToken.None);
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
		///     Receive message and deserialize to T
		/// </summary>
		/// <typeparam name="T">Type to deserialize to</typeparam>
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
			var (result, buffer) = await webSocket.ReceiveAsync(CancellationToken.None);
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
			var (result, buffer) = await webSocket.ReceiveAsync(CancellationToken.None);
			if (onReceived != null)
				await onReceived(Encoding.UTF8.GetString(buffer), result.MessageType);
			if (!result.CloseStatus.HasValue)
				result = await webSocket.Listen(onReceived);
			return result;
		}
	}
}