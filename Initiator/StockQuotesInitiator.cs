using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Transport;
using static Initiator.ApplicationWrapper;
using Message = QuickFix.Message;

namespace Initiator {
	public class StockQuotesInitiator {
		public StockQuotesInitiator(string configFile) {
			Settings = new SessionSettings(configFile);
			FileStoreFactory storeFactory = new(Settings);
			FileLogFactory logFactory = new(Settings);
			DefaultMessageFactory messageFactory = new();
			Sessions = Settings.GetSessions();
			Application = new ApplicationWrapper();
			SocketInitiator = new SocketInitiator(Application, storeFactory, Settings, logFactory, messageFactory);
		}

		public StockQuotesInitiator() : this(@"Config/Initiator.cfg") { }

		public HashSet<SessionID> Sessions { get; }
		public SessionID DefaultSession { get; set; }

		public EchoType Echo {
			get => Application.Echo;
			set => Application.Echo = value;
		}

		public HashSet<MsgType> EchoBlacklist => Application.EchoBlacklist;

		protected SessionSettings Settings { get; }
		protected SocketInitiator SocketInitiator { get; }
		public ApplicationWrapper Application { get; }

		public void Start() => SocketInitiator.Start();

		public void Stop() => SocketInitiator.Stop();

		public bool LogIn(SessionID sessionId = null) {
			var session = sessionId ?? DefaultSession;
			Logon logon = new(new EncryptMethod(0), new HeartBtInt(Settings.Get(session).GetInt("HeartBtInt")));
			return SendMessage(logon, session);
		}

		public bool LogOut(SessionID sessionId = null) {
			var session = sessionId ?? DefaultSession;
			Logout logout = new();
			return SendMessage(logout, session);
		}

		public bool RequestMarketData(MarketDataRequestType type = null, DateTime? beginDate = null, DateTime? endDate = null, SessionID sessionId = null) {
			type ??= MarketDataRequestType.RealTime;
			var request = new MarketDataRequest {
				MDReqID = new MDReqID("MarketDataRequestId"),
				SubscriptionRequestType = type.Type
			};
			if (beginDate.HasValue) {
				var begin = new TDBeginDate((DateTime)beginDate);
				request.SetField(begin);
			}
			if (endDate.HasValue) {
				var end = new TDEndDate((DateTime)endDate);
				request.SetField(end);
			}
			return SendMessage(request, sessionId ?? DefaultSession);
		}

		public Task UntilLoggedIn(SessionID sessionId = null, int interval = 1000, int timeout = -1) => Application.UntilLoggedIn(sessionId ?? DefaultSession ?? throw new NullReferenceException("Default session not set"), interval, timeout);

		private static bool SendMessage(Message message, SessionID sessionId) => Session.SendToTarget(message, sessionId ?? throw new NullReferenceException("Default session not set"));

		public class MarketDataRequestType {
			public static readonly MarketDataRequestType PlayBack = new('0');
			public static readonly MarketDataRequestType RealTime = new('1');
			public static readonly MarketDataRequestType History = new('2');
			private MarketDataRequestType(char type) => Type = new SubscriptionRequestType(type);
			public SubscriptionRequestType Type { get; set; }
		}
	}
}