using System;

namespace Shared {
	public class Quote {
		public DateTime TradingTime { get; init; }

		public decimal PreClosingPrice { get; init; }

		public decimal OpeningPrice { get; init; }

		public decimal HighestPrice { get; init; }

		public decimal LowestPrice { get; init; }

		public decimal HighLimitedPrice { get; init; }

		public decimal LowLimitedPrice { get; init; }

		public decimal ClosingPrice { get; init; }

		public decimal[] AskPrices { get; init; }

		public long[] AskVolumes { get; init; }

		public decimal[] BidPrices { get; init; }

		public long[] BidVolumes { get; init; }

		public int NumberOfTrades { get; init; }

		public long TotalVolume { get; init; }

		public decimal TotalTurnover { get; init; }
	}
}