using System.Threading.Tasks;
using NUnit.Framework;

namespace Tushare.Test {
	public class TushareTest {
		public TushareManager TuShare { get; set; }

		[SetUp]
		public void Setup() {
			TuShare = new TushareManager("ecffe13bdfb4ccb617b344f276b4827d3614e0a736a5fe7c0c6767ce");
		}

		[Test]
		public async Task CheckTradeStatus() {
			bool status = await TuShare.CheckTradeStatus();
			Assert.IsTrue(status);
		}
	}
}