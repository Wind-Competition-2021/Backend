using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Security;
using Timer = System.Timers.Timer;

namespace Server.Controllers {
	[ApiController]
	public class WebSocketController : ControllerBase {
		public static string FromBytes(byte[] buffer) => Encoding.UTF8.GetString(buffer);

		public static byte[] ToBytes(string text) => Encoding.UTF8.GetBytes(text);

		protected async Task<IActionResult> HandleWebSocket(Action<string, WebSocketMessageType> OnReceived, Func<(string Text, WebSocketMessageType MessageType)> OnSent, int sendInterval = 1000) {
			if (!HttpContext.WebSockets.IsWebSocketRequest) {
				HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return BadRequest("Not a websocket request");
			}
			async Task<WebSocketCloseStatus> ListenMessage(WebSocket webSocket, Timer senderTimer = null) {
				const int bufferSize = 1024;
				var buffer = new byte[bufferSize];
				var offset = 0;
				var free = buffer.Length;
				WebSocketCloseStatus? closeStatus = null;
				while (true) {
					var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, free), CancellationToken.None);
					offset += result.Count;
					free -= result.Count;
					if (result.EndOfMessage || result.CloseStatus.HasValue) {
						if (result.EndOfMessage)
							OnReceived(FromBytes(buffer), result.MessageType);
						if (result.CloseStatus.HasValue)
							closeStatus = result.CloseStatus;
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
				if (!closeStatus.HasValue)
					await ListenMessage(webSocket);
				if (senderTimer is {
					Enabled: true
				})
					senderTimer.Stop();
				return closeStatus!.Value;
			}
			var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
			var sendMessageTimer = new Timer(sendInterval);
			sendMessageTimer.Elapsed += (sender, args) => {
				var (body, messageType) = OnSent();
				webSocket.SendAsync(ToBytes(body), messageType, true, CancellationToken.None);
			};
			sendMessageTimer.Start();
			await ListenMessage(webSocket);
			return Ok(null);
		}

		[HttpGet]
		[Route("/api/ws/stock/list")]
		[Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
		public virtual async Task<IActionResult> StartStockListUpdating() {
			Random random = new();
			return await HandleWebSocket(
				(text, type) => {
					Console.WriteLine($"type: {type}");
					Console.WriteLine($"content: {text}");
				},
				() => {
					var priceBase = random.Next() % 10000;
					var resp = new RealTimePrice {
						Id = $"sh.{random.Next() % 1000000}",
						PreClosing = priceBase + random.Next() % 1000,
						Opening = priceBase + random.Next() % 1000,
						Closing = priceBase + random.Next() % 1000,
						Highest = priceBase + 1000 + random.Next() % 1000,
						Lowest = priceBase - 1000 - random.Next() % 1000,
						Volume = Math.Abs(random.Next() % 10000),
						Turnover = priceBase * random.Next() % 10000,
						Time = DateTime.Now
					};
					return (resp.ToJson(), WebSocketMessageType.Text);
				}
			);
		}

		[HttpGet]
		[Route("/api/ws/stock")]
		[Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
		public virtual async Task<IActionResult> StartStockUpdating() {
			Random random = new();
			return await HandleWebSocket(
				(text, type) => {
					Console.WriteLine($"type: {type}");
					Console.WriteLine($"content: {text}");
				},
				() => {
					var priceBase = random.Next() % 10000;
					var resp = new RealTimePrice {
						Id = $"sh.{random.Next() % 1000000}",
						PreClosing = priceBase + random.Next() % 1000,
						Opening = priceBase + random.Next() % 1000,
						Closing = priceBase + random.Next() % 1000,
						Highest = priceBase + 1000 + random.Next() % 1000,
						Lowest = priceBase - 1000 - random.Next() % 1000,
						Volume = Math.Abs(random.Next() % 10000),
						Turnover = priceBase * random.Next() % 10000,
						Time = DateTime.Now
					};
					return (resp.ToJson(), WebSocketMessageType.Text);
				}
			);
		}
	}
}