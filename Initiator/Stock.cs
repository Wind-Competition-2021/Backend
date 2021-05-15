using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using QuickFix.FIX44;

namespace Initiator {
	public class Market : IEquatable<Market> {
		private Market(string code) => Code = code;
		public static readonly Market ShenZhen = new("SZ");
		public static readonly Market ShangHai = new("SH");
		public static readonly Market HongKong = new("HK");
		public string Code { get; }

		public static Market Find(string code) {
			return code switch {
				"SZ" => ShenZhen,
				"SH" => ShangHai,
				"HK" => HongKong,
				_    => null
			};
		}

		public bool Equals(Market other) => other != null && Code.Equals(other.Code);

		public override int GetHashCode() => Code.Aggregate(0, (current, t) => current * 26 + t - 'A');
	}

	public class Stock : IEquatable<Stock> {
		#region Constructors
		public Stock(Market market, string id) {
			Market = market;
			Id = id;
		}

		public Stock(string marketCode, string id) {
			Market = Market.Find(marketCode) ?? throw new ArgumentException("Unrecognized market code", nameof(marketCode));
			Id = id;
		}

		public Stock(string unifiedId) {
			var parts = unifiedId.Split('.');
			if (parts.Length != 2)
				throw new FormatException();
			Market = Market.Find(parts[0]) ?? throw new Exception("Unrecognized market code");
			Id = parts[1];
		}
		#endregion

		#region Properties
		public Market Market { get; }
		public string Id { get; }
		public Queue<Quote> Quotes { get; } = new();
		#endregion

		public void Push(TDFData message) { }

		#region Classes
		public class Quote {
			#region Fields
			private static readonly Regex numberExtractor = new(@"\d+", RegexOptions.Compiled);
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
				CurrentPrice = Convert.ToDecimal(message.Match.Obj);
				AskPrices = numberExtractor.Matches(message.AskPrice.Obj).Take(5).Select(match => Convert.ToDecimal(match.Value)).ToArray();
				AskVolumes = numberExtractor.Matches(message.AskVol.Obj).Take(5).Select(match => Convert.ToInt32(match.Value)).ToArray();
				BidPrices = numberExtractor.Matches(message.BidPrice.Obj).Take(5).Select(match => Convert.ToDecimal(match.Value)).ToArray();
				BidVolumes = numberExtractor.Matches(message.BidVol.Obj).Take(5).Select(match => Convert.ToInt32(match.Value)).ToArray();
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
			public decimal CurrentPrice { get; }
			public decimal[] AskPrices { get; }
			public int[] AskVolumes { get; }
			public decimal[] BidPrices { get; }
			public int[] BidVolumes { get; }
			public int NumberOfTrades { get; }
			public int TotalVolume { get; }
			public decimal TotalTurnover { get; }
			#endregion
		}
		#endregion

		public bool Equals(Stock other) => other != null && Market.Equals(other.Market) && Id.Equals(other.Id);

		public override int GetHashCode() => (Market.Code + Id).Aggregate(0, (current, t) => current * 26 + t - 'A');
	}
}