using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using QuickFix.FIX44;

namespace Initiator {
	public class Stock : IEquatable<Stock> {
		#region Constructors
		public Stock(string id) => Id = id;
		#endregion

		public bool Equals(Stock other) => other?.Id?.Equals(Id) == true;

		public override int GetHashCode() => Id.Aggregate(0, (current, t) => current * 26 + t - 'A');

		#region Properties
		[RegularExpression(@"[a-zA-Z]{2}\.\d{6}")]
		public string Id { get; }

		public List<Quote> Quotes { get; } = new();
		#endregion
	}

	public class Quote {
		#region Fields
		private static readonly Regex NumberExtractor = new(@"\d+", RegexOptions.Compiled);
		#endregion

		#region Constructors
		public Quote(TDFData message) {
			var date = Convert.ToInt32(message.TradingDay.Obj);
			var time = Convert.ToInt32(message.Time.Obj);
			TradingTime = new DateTime(date / 10000, date / 100 % 100, date % 100, time / 10000000, time / 100000 % 100, time / 1000 % 100, time % 1000);
			PreClosingPrice = Convert.ToDecimal(message.PreClose.Obj);
			OpeningPrice = Convert.ToDecimal(message.Open.Obj);
			HighestPrice = Convert.ToDecimal(message.High.Obj);
			LowestPrice = Convert.ToDecimal(message.Low.Obj);
			HighLimitedPrice = Convert.ToDecimal(message.HighLimited);
			LowLimitedPrice = Convert.ToDecimal(message.LowLimited);
			ClosingPrice = Convert.ToDecimal(message.Match.Obj);
			AskPrices = NumberExtractor.Matches(message.AskPrice.Obj).Take(5).Select(match => Convert.ToDecimal(match.Value)).ToArray();
			AskVolumes = NumberExtractor.Matches(message.AskVol.Obj).Take(5).Select(match => Convert.ToInt32(match.Value)).ToArray();
			BidPrices = NumberExtractor.Matches(message.BidPrice.Obj).Take(5).Select(match => Convert.ToDecimal(match.Value)).ToArray();
			BidVolumes = NumberExtractor.Matches(message.BidVol.Obj).Take(5).Select(match => Convert.ToInt32(match.Value)).ToArray();
			NumberOfTrades = Convert.ToInt32(message.NumTrades.Obj);
			TotalVolume = Convert.ToInt32(message.IVolumn);
			TotalTurnover = Convert.ToDecimal(message.ITurnover);
		}
		#endregion

		#region Properties
		public DateTime TradingTime { get; }
		public decimal PreClosingPrice { get; }
		public decimal OpeningPrice { get; }
		public decimal HighestPrice { get; }
		public decimal LowestPrice { get; }
		public decimal HighLimitedPrice { get; }
		public decimal LowLimitedPrice { get; }
		public decimal ClosingPrice { get; }
		public decimal[] AskPrices { get; }
		public int[] AskVolumes { get; }
		public decimal[] BidPrices { get; }
		public int[] BidVolumes { get; }
		public int NumberOfTrades { get; }
		public long TotalVolume { get; }
		public decimal TotalTurnover { get; }
		#endregion
	}
}