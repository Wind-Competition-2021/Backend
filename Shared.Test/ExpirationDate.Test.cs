using System;
using NUnit.Framework;
using Shared;

namespace Shared.Test {
	public class ExpirationDateTest {
		[SetUp]
		public void Setup() { }

		[Test]
		public void Constructor() {
			var now = DateTime.Now;
			ExpirationDate expDate = new(now);
			Assert.AreEqual(now, expDate.AbsoluteExpiration);
			expDate = new ExpirationDate(TimeSpan.FromMinutes(1));
			Assert.AreEqual(TimeSpan.FromMinutes(1), expDate.SlidingExpiration);
		}
	}
}