using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Initiator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using QuickFix.Fields;
using QuickFix.FIX44;
using Server.Managers;
using Server.Models;
using Server.Security;
using Quote = Initiator.Quote;
using Timer = Server.Utilities.Timer;

namespace Server.Controllers {
	/// <summary>
	///     Controller for websocket request
	/// </summary>
	[ApiController]
	public sealed class WebSocketController : ControllerBase {
		#region Constructors
		/// <summary>
		/// </summary>
		/// <param name="configurationManager"></param>
		/// <param name="realtimeQuotesManager"></param>
		/// <param name="playbackQuotesManager"></param>
		/// <param name="initiator"></param>
		/// <param name="settings"></param>
		public WebSocketController(
			ConfigurationManager configurationManager,
			RealtimeQuotesManager realtimeQuotesManager,
			PlaybackQuotesManager playbackQuotesManager,
			StockQuotesInitiator initiator,
			JsonSerializerSettings settings
		) {
			ConfigurationManager = configurationManager;
			RealtimeQuotesManager = realtimeQuotesManager;
			PlaybackQuotesManager = playbackQuotesManager;
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
		#endregion

		#region Classes
		/// <summary>
		/// </summary>
		[DataContract]
		public class Message {
			/// <summary>
			/// </summary>
			[DataMember(Name = "type")]
			public MessageType Type { get; set; }

			/// <summary>
			/// </summary>
			[DataMember(Name = "content")]
			public JToken Content { get; set; }
		}
		#endregion

		/// <summary>
		/// </summary>
		[DataContract]
		public record PlaybackMessage(DateTime Time, List<RealtimePrice> Quotes) {
			/// <summary>
			/// </summary>
			[DataMember(Name = "time")]
			public DateTime Time { get; } = Time;

			/// <summary>
			/// </summary>
			[DataMember(Name = "quotes")]
			public List<RealtimePrice> Quotes { get; } = Quotes;
		}

		#region Properties
		/// <summary>
		/// </summary>
		public ConfigurationManager ConfigurationManager { get; }

		/// <summary>
		/// </summary>
		public RealtimeQuotesManager RealtimeQuotesManager { get; }

		/// <summary>
		/// </summary>
		public PlaybackQuotesManager PlaybackQuotesManager { get; }

		/// <summary>
		/// </summary>
		public StockQuotesInitiator Initiator { get; }

		/// <summary>
		/// </summary>
		public JsonSerializerSettings SerializerSettings { get; init; }
		#endregion

		#region Methods
		#region Realtime
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[HttpGet("/api/quote/realtime/list")]
		[Authorize(AuthenticationSchemes = TokenQueryAuthenticationHandler.SchemeName)]
		public Task<IActionResult> UpdateQuotesList([FromQuery] [Required] string token)
			=> UpdateQuotes(
				token,
				SendRealtimeMessage(token),
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
				SendRealtimeMessage(token, id),
				config => config.RefreshInterval.Trend!.Value
			);

		private Task<IActionResult> UpdateQuotes(string token, Func<WebSocket, Configuration, Task> sendMessage, Func<Configuration, TimeSpan> getInterval)
			=> PushQuotes(
				token,
				sendMessage,
				getInterval,
				() => RealtimeQuotesManager.Initialized,
				() => RealtimeQuotesManager?.Stopped == true,
				"Trade Off",
				(ids, _) => RealtimeQuotesManager.Initialize(ids)
			);

		private Func<WebSocket, Configuration, Task> SendRealtimeMessage(string token, string id = null)
			=> (webSocket, config) => {
				var prices = id is null ? RealtimeQuotesManager.GetList(token) : RealtimeQuotesManager.GetTrend(token, id);
				if (prices == null || prices.Count == 0)
					return null;
				foreach (var price in prices)
					price.Pinned = config.PinnedStocks.Contains(price.Id);
				return webSocket.SendAsync(prices.ToArray());
			};
		#endregion

		#region Replay
		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <param name="begin"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		[HttpGet("/api/quote/playback/list")]
		[Authorize(AuthenticationSchemes = TokenQueryAuthenticationHandler.SchemeName)]
		public Task<IActionResult> ReplayQuotesList([FromQuery] [Required] string token, [FromQuery] DateTime? begin, [FromQuery] DateTime? end)
			=> ReplayQuotes(
				token,
				SendPlaybackMessage(token),
				config => TimeSpan.FromSeconds(300d / config.PlaybackSpeed!.Value),
				begin,
				end,
				msg => {
					if (msg.Type != MessageType.Control || !PlaybackQuotesManager.Contains(token))
						return Task.CompletedTask;
					var signal = JsonConvert.DeserializeObject<ControlSignal>(JsonConvert.SerializeObject(msg.Content.Value<string>()));
					switch (signal) {
						case ControlSignal.Stop:
							PlaybackQuotesManager[token].Stop();
							break;
						case ControlSignal.Resume:
							PlaybackQuotesManager[token].Start();
							break;
					}
					return Task.CompletedTask;
				},
				(_, _) => {
					PlaybackQuotesManager[token].Close();
					PlaybackQuotesManager.Remove(token);
					return Task.CompletedTask;
				}
			);

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet("/api/quote/playback/trend")]
		[Authorize(AuthenticationSchemes = TokenQueryAuthenticationHandler.SchemeName)]
		public Task<IActionResult> ReplayQuotesTrend([FromQuery] [Required] string token, [FromQuery] [Required] string id)
			=> ReplayQuotes(
				token,
				SendPlaybackMessage(token, id),
				config => TimeSpan.FromSeconds(300d / config.PlaybackSpeed!.Value),
				null,
				null,
				null,
				(_, _) => {
					PlaybackQuotesManager.TrendsLastIndices[token].Remove(id);
					return Task.CompletedTask;
				}
			);

		private Task<IActionResult> ReplayQuotes(string token, Func<WebSocket, Configuration, Task> sendMessage, Func<Configuration, TimeSpan> getInterval, DateTime? begin = null, DateTime? end = null, Func<Message, Task> onMessage = null, Func<bool, WebSocketReceiveResult, Task> onClose = null)
			=> PushQuotes(
				token,
				sendMessage,
				getInterval,
				() => PlaybackQuotesManager.Contains(token),
				() => PlaybackQuotesManager[token].Finished,
				"Playback Finished",
				!begin.HasValue || !end.HasValue
					? null
					: (ids, msgSender) => {
						PlaybackQuotesManager.Initialize(token, ids, begin!.Value, end!.Value, msgSender);
						return Task.CompletedTask;
					},
				onMessage,
				onClose
			);

		private Func<WebSocket, Configuration, Task> SendPlaybackMessage(string token, string id = null)
			=> (webSocket, config) => {
				var prices = id is null ? PlaybackQuotesManager.GetList(token) : PlaybackQuotesManager.GetTrend(token, id);
				var result = new PlaybackMessage(PlaybackQuotesManager[token].Now, prices);
				if (prices?.Count > 0)
					foreach (var price in prices)
						price.Pinned = config.PinnedStocks.Contains(price.Id);
				return webSocket.SendAsync(result);
			};
		#endregion

		private async Task<IActionResult> PushQuotes(
			string token,
			Func<WebSocket, Configuration, Task> sendMessage,
			Func<Configuration, TimeSpan> getInterval,
			Func<bool> isInitialized,
			Func<bool> isFinished,
			string closeDescription,
			Func<string[], Timer, Task> initialize = null,
			Func<Message, Task> onMessage = null,
			Func<bool, WebSocketReceiveResult, Task> onClose = null
		) {
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
				if (!lastElapsedFinished || !isInitialized())
					return;
				var tsk = sendMessage(webSocket, ConfigurationManager[token]);
				if (tsk is not null) {
					lastElapsedFinished = false;
					tsk.ContinueWith(
						_ =>
							lastElapsedFinished = true
					);
				}
				if (isFinished())
					messageSender.Close();
			}

			if (isInitialized()) {
				messageSender.Elapsed += Elapsed;
				messageSender.Start();
			}
			var receive = initialize is null
				? webSocket.WaitUntilClose()
				: webSocket.Listen(
					async (text, type) => {
						if (type != WebSocketMessageType.Text)
							return;
						try {
							var msg = JsonConvert.DeserializeObject<Message>(text, SerializerSettings);
							if (msg?.Type == MessageType.Initialization) {
								await initialize((msg.Content as JArray)!.ToEnumerable<string>().ToArray(), messageSender);
								messageSender.Elapsed += Elapsed;
								messageSender.Start();
							}
							else if (onMessage is not null)
								await onMessage(msg);
						}
						catch (JsonSerializationException) {
							// ignored
						}
					}
				);
			var send = Task.Run(
				async () => {
					while (!isInitialized() || !isFinished())
						await Task.Delay(getInterval(ConfigurationManager[token]));
					return new WebSocketReceiveResult(0, WebSocketMessageType.Close, true, WebSocketCloseStatus.NormalClosure, closeDescription);
				}
			);
			var task = await Task.WhenAny(new[] {receive, send});
			var result = task.Result;
			messageSender.Close();
			onClose?.Invoke(task == receive, result);
			await webSocket.CloseAsync(result.CloseStatus!.Value, result.CloseStatusDescription, CancellationToken.None);
			return new EmptyResult();
		}
		#endregion

		#region Enums
		/// <summary>
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public enum MessageType : byte {
			/// <summary>
			/// </summary>
			[EnumMember(Value = "init")]
			Initialization = 0,

			/// <summary>
			/// </summary>
			[EnumMember(Value = "ctrl")]
			Control = 1
		}

		/// <summary>
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public enum ControlSignal : byte {
			/// <summary>
			/// </summary>
			[EnumMember(Value = "stop")]
			Stop = 0,

			/// <summary>
			/// </summary>
			[EnumMember(Value = "resume")]
			Resume = 1
		}
		#endregion
	}

	#region Extensions
	/// <summary>
	/// </summary>
	public static class JArrayExtension {
		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static IEnumerable<T> ToEnumerable<T>(this JArray array) => array.Cast<JValue>().Select(value => (T)value.Value);
	}

	/// <summary>
	/// </summary>
	public static class WebSocketExtension {
		/// <summary>
		///     Combine multiple continuous message
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
		/// <returns></returns>
		public static async Task<WebSocketReceiveResult> WaitUntilClose(this WebSocket webSocket) {
			(var result, byte[] _) = await webSocket.ReceiveAsync(CancellationToken.None);
			if (!result.CloseStatus.HasValue)
				result = await webSocket.WaitUntilClose();
			return result;
		}

		/// <summary>
		///     Recursively receive messages until websocket is closed
		/// </summary>
		/// <param name="webSocket"></param>
		/// <param name="onReceived"></param>
		/// <returns></returns>
		public static async Task<WebSocketReceiveResult> Listen(this WebSocket webSocket, Action<string, WebSocketMessageType> onReceived) {
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
		public static async Task<WebSocketReceiveResult> Listen(this WebSocket webSocket, Func<string, WebSocketMessageType, Task> onReceived) {
			(var result, byte[] buffer) = await webSocket.ReceiveAsync(CancellationToken.None);
			if (onReceived != null)
				await onReceived(Encoding.UTF8.GetString(buffer), result.MessageType);
			if (!result.CloseStatus.HasValue)
				result = await webSocket.Listen(onReceived);
			return result;
		}
	}
	#endregion
}