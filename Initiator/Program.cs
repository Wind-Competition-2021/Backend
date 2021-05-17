using System.Linq;
using static Initiator.StockQuotesInitiator;

namespace Initiator {
	public static class Program {
		public static void Main() {
			StockQuotesInitiator initiator = new() {
				Echo = ApplicationWrapper.EchoType.All
			};
			initiator.EchoBlacklist.Clear();
			initiator.DefaultSession = initiator.Sessions.First();
			initiator.Start();
			//initiator.LogOut();
			initiator.LogIn();
			//initiator.RequestMarketData(MarketDataRequestType.History, new DateTime(2021, 5, 6), new DateTime(2021, 5, 7));
			initiator.Application.SessionLoggedIn += (sender, e) => { initiator.RequestMarketData(MarketDataRequestType.RealTime); };
		}
	}
}