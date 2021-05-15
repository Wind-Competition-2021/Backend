using System.Linq;
using static Initiator.StockQuotesInitiator;

namespace Initiator {
	public static class Program {
		public static void Main() {
			StockQuotesInitiator initiator = new() {
				Echo = ApplicationWrapper.EchoType.All
			};
			initiator.DefaultSession = initiator.Sessions.First();
			initiator.Start();
			initiator.LogOut();
			initiator.LogIn();
			initiator.UntilLoggedIn(interval: 200).ContinueWith(_ => initiator.RequestMarketData(MarketDataRequestType.PlayBack));
		}
	}
}