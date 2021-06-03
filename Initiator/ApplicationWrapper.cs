using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Colorful;
using QuickFix;
using QuickFix.Fields;
using Console = Colorful.Console;

namespace Initiator {
	public interface ITransmissionAnalysis {
		public uint UploadPackagesCount { get; }
		public uint DownloadPackagesCount { get; }
		public ulong UploadTraffic { get; }
		public ulong DownloadTraffic { get; }
		public double UploadSpeed { get; }
		public double DownloadSpeed { get; }
	}

	public class SessionEventArgs : EventArgs {
		public readonly SessionID SessionId;

		public SessionEventArgs(SessionID sessionId) => SessionId = sessionId;
	}

	public class MessageEventArgs : SessionEventArgs {
		public readonly Message Message;

		public MessageEventArgs(Message message, SessionID sessionId) : base(sessionId) => Message = message;
	}

	public delegate void SessionEventHandler(object sender, SessionEventArgs e);

	public delegate void MessageEventHandler(object sender, MessageEventArgs e);

	public class ApplicationWrapper : ITransmissionAnalysis, IApplication {
		#region Enums
		[Flags]
		public enum EchoType : byte {
			None = 0,
			App = 1,
			Admin = 2,
			BothLevel = 3,
			In = 4,
			Out = 8,
			BothDirection = 12,
			Session = 16,
			All = 31
		}
		#endregion

		#region Constructors
		public ApplicationWrapper() {
			UploadPackagesCount = 0;
			UploadTraffic = 0;
			DownloadPackagesCount = 0;
			DownloadPackagesCount = 0;
		}
		#endregion

		#region Fields
		private DateTime _startTime;
		private DateTime _logoutTime;
		#endregion

		#region Events
		public event SessionEventHandler SessionCreated = delegate { };
		public event SessionEventHandler SessionLoggedIn = delegate { };
		public event SessionEventHandler SessionLoggedOut = delegate { };
		public event MessageEventHandler MessageSent = delegate { };
		public event MessageEventHandler MessageReceived = delegate { };
		#endregion

		#region Properties
		public uint UploadPackagesCount { get; private set; }

		public uint DownloadPackagesCount { get; private set; }

		public ulong UploadTraffic { get; private set; }

		public ulong DownloadTraffic { get; private set; }

		public double UploadSpeed => _startTime == default || DateTime.Now == _startTime ? -1 : UploadTraffic / (DateTime.Now - _startTime).TotalSeconds;
		public double DownloadSpeed => _startTime == default || DateTime.Now == _startTime ? -1 : DownloadTraffic / (DateTime.Now - _startTime).TotalSeconds;
		public EchoType Echo { get; set; } = EchoType.All;
		public HashSet<MsgType> EchoBlacklist { get; } = new(new MsgType[] {new(MsgType.HEARTBEAT)});
		public Dictionary<SessionID, SessionStatus> Sessions { get; } = new();
		#endregion

		#region Methods
		public void OnCreate(SessionID sessionId) {
			SessionCreated(this, new SessionEventArgs(sessionId));
			SessionStatus status = new() {
				Created = true
			};
			Sessions[sessionId] = status;

			if (!Echo.HasFlag(EchoType.Session))
				return;
			Console.Write("[create] ", Color.BlueViolet);
			Console.WriteLine(sessionId);
		}

		public void OnLogon(SessionID sessionId) {
			SessionLoggedIn(this, new SessionEventArgs(sessionId));
			_logoutTime = DateTime.Now;

			if (!Sessions.ContainsKey(sessionId))
				throw new KeyNotFoundException("SessionId doesn't exist");
			Sessions[sessionId].LoggedIn = true;

			if (!Echo.HasFlag(EchoType.Session))
				return;
			Console.Write("[logon] ", Color.BlueViolet);
			Console.WriteLine(sessionId);
		}

		public void OnLogout(SessionID sessionId) {
			SessionLoggedOut(this, new SessionEventArgs(sessionId));
			if (_startTime == default)
				_startTime = DateTime.Now;
			else if (_logoutTime != default)
				_startTime = DateTime.Now - (_logoutTime - _startTime);

			if (!Sessions.ContainsKey(sessionId))
				throw new KeyNotFoundException("SessionId doesn't exist");
			Sessions[sessionId].LoggedIn = false;

			if (!Echo.HasFlag(EchoType.Session))
				return;
			Console.Write("[logout] ", Color.BlueViolet);
			Console.WriteLine(sessionId);
		}

		public void OnMessageSent(Message message, SessionID sessionId) {
			MessageSent(this, new MessageEventArgs(message, sessionId));
			++UploadPackagesCount;
			int length;
			try {
				length = message.Header.GetInt(9);
			}
			catch (FieldNotFoundException) {
				length = message.ToString().Length;
			}
			UploadTraffic += (ulong)length;
		}

		public void OnMessageReceived(Message message, SessionID sessionId) {
			MessageReceived(this, new MessageEventArgs(message, sessionId));
			++DownloadPackagesCount;
			int length;
			try {
				length = message.Header.GetInt(9);
			}
			catch (FieldNotFoundException) {
				length = message.ToString().Length;
			}
			DownloadTraffic += (ulong)length;
		}

		public void ToAdmin(Message message, SessionID sessionId) {
			OnMessageSent(message, sessionId);
			if (!Echo.HasFlag(EchoType.Out | EchoType.Admin))
				return;
			MsgType type = new();
			message.Header.GetField(type);
			if (EchoBlacklist.Contains(type))
				return;
			Console.Write("--> ", Color.Navy);
			Console.Write($"[{type.Obj}]", Color.MediumSpringGreen);
			message.Print();
		}

		public void FromAdmin(Message message, SessionID sessionId) {
			OnMessageReceived(message, sessionId);
			if (!Echo.HasFlag(EchoType.In | EchoType.Admin))
				return;
			MsgType type = new();
			message.Header.GetField(type);
			if (EchoBlacklist.Contains(type))
				return;
			Console.Write("--> ", Color.Navy);
			Console.Write($"[{type.Obj}]", Color.MediumSpringGreen);
			message.Print();
		}

		public void ToApp(Message message, SessionID sessionId) {
			OnMessageSent(message, sessionId);
			if (!Echo.HasFlag(EchoType.Out | EchoType.App))
				return;
			MsgType type = new();
			message.Header.GetField(type);
			if (EchoBlacklist.Contains(type))
				return;
			Console.Write("--> ", Color.Orchid);
			Console.Write($"[{type.Obj}]", Color.MediumSpringGreen);
			message.Print();
		}

		public void FromApp(Message message, SessionID sessionId) {
			OnMessageReceived(message, sessionId);
			if (!Echo.HasFlag(EchoType.In | EchoType.App))
				return;
			MsgType type = new();
			message.Header.GetField(type);
			if (EchoBlacklist.Contains(type))
				return;
			Console.Write("<-- ", Color.Gold);
			Console.Write($"[{type.Obj}]", Color.MediumSpringGreen);
			message.Print();
		}

		public async Task UntilLoggedIn(SessionID sessionId, int interval = 1000, int timeout = -1) {
			var waitTask = Task.Run(
				async () => {
					while (!Sessions.ContainsKey(sessionId) || !Sessions[sessionId].LoggedIn)
						await Task.Delay(interval);
				}
			);

			if (waitTask !=
				await Task.WhenAny(
					waitTask,
					Task.Delay(timeout)
				))
				throw new TimeoutException();
		}
		#endregion
	}

	public static class MessageUtility {
		public static void Print(this Message message) {
			string raw = message.ToString();
			var fields = raw.Split((char)1).Where(field => !string.IsNullOrEmpty(field));
			foreach (string field in fields) {
				string[] pair = field.Split('=');
				Console.WriteFormatted("{0}={1} ", Color.AliceBlue, new Formatter(pair[0], Color.Cyan), new Formatter(pair[1], Color.DarkSalmon));
			}
			Console.WriteLine();
		}
	}

	public class SessionStatus {
		private Status _status = Status.None;

		public bool Created {
			get => _status.HasFlag(Status.Created);
			init {
				if (value)
					_status |= Status.Created;
				else
					_status &= ~Status.Created;
			}
		}

		public bool LoggedIn {
			get => _status.HasFlag(Status.LoggedIn);
			set {
				if (value) {
					if (!Created)
						throw new InvalidOperationException("Session should be created before logging in");
					_status |= Status.LoggedIn;
				}
				else
					_status &= ~Status.LoggedIn;
			}
		}

		public bool LoggedOut => Created && !LoggedIn;

		[Flags]
		private enum Status : byte {
			None = 0,
			Created = 1,
			LoggedIn = 2
		}
	}
}