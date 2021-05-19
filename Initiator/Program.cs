using System;
using System.Linq;
using System.Threading.Tasks;
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
			initiator.LogIn();
			initiator.UntilLoggedIn()
				.ContinueWith(
					_ => {
						initiator.RequestMarketData(MarketDataRequestType.PlayBack);
						Task.Delay(TimeSpan.FromSeconds(10))
							.ContinueWith(
								_ => {
									initiator.LogOut();
									initiator.Stop();
								}
							);
					}
				);
		}
	}
}