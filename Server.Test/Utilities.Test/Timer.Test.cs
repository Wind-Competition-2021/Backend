using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Server.Utilities.Test {
	public class TimerTest {
		[SetUp]
		public void Setup() { }

		[Test]
		public async Task Delay() {
			var timer = new Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
			var startTime = DateTime.Now;
			int count = 0;
			timer.Elapsed += (sender, e) => {
				Console.WriteLine(e.SignalTime);
				if (count == 0)
					Assert.IsTrue(Math.Abs((e.SignalTime - startTime).TotalSeconds - 1) < 0.01);
				else {
					timer.Stop();
					Assert.IsTrue(Math.Abs((e.SignalTime - startTime).TotalSeconds / 3 - 1) < 0.01);
				}
				++count;
			};
			timer.Start();
			await Task.Delay(TimeSpan.FromSeconds(4));
		}
	}
}