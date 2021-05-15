using QuickFix.Fields;

namespace QuickFix.FIX44 {
	public class TDFData : Message {
		public const string MsgType = "TD";
		public TDFData() => Header.SetField(new MsgType(MsgType));

		public WindCode WindCode {
			get {
				WindCode windCode = new();
				GetField(windCode);
				return windCode;
			}
			set => SetField(value);
		}

		public TradingDay TradingDay {
			get {
				TradingDay tradingDay = new();
				GetField(tradingDay);
				return tradingDay;
			}
			set => SetField(value);
		}

		public Time Time {
			get {
				Time time = new();
				GetField(time);
				return time;
			}
			set => SetField(value);
		}

		public PreClose PreClose {
			get {
				PreClose preClose = new();
				GetField(preClose);
				return preClose;
			}
			set => SetField(value);
		}

		public High High {
			get {
				High high = new();
				GetField(high);
				return high;
			}
			set => SetField(value);
		}

		public Open Open {
			get {
				Open open = new();
				GetField(open);
				return open;
			}
			set => SetField(value);
		}

		public Low Low {
			get {
				Low low = new();
				GetField(low);
				return low;
			}
			set => SetField(value);
		}

		public Match Match {
			get {
				Match match = new();
				GetField(match);
				return match;
			}
			set => SetField(value);
		}

		public AskPrice AskPrice {
			get {
				AskPrice askPrice = new();
				GetField(askPrice);
				return askPrice;
			}
			set => SetField(value);
		}

		public AskVol AskVol {
			get {
				AskVol askVol = new();
				GetField(askVol);
				return askVol;
			}
			set => SetField(value);
		}

		public BidPrice BidPrice {
			get {
				BidPrice bidPrice = new();
				GetField(bidPrice);
				return bidPrice;
			}
			set => SetField(value);
		}

		public BidVol BidVol {
			get {
				BidVol bidVol = new();
				GetField(bidVol);
				return bidVol;
			}
			set => SetField(value);
		}

		public NumTrades NumTrades {
			get {
				NumTrades numTrades = new();
				GetField(numTrades);
				return numTrades;
			}
			set => SetField(value);
		}

		public IVolumn IVolumn {
			get {
				IVolumn iVolumn = new();
				GetField(iVolumn);
				return iVolumn;
			}
			set => SetField(value);
		}

		public ITurnover ITurnover {
			get {
				ITurnover iTurnover = new();
				GetField(iTurnover);
				return iTurnover;
			}
			set => SetField(value);
		}

		public HighLimited HighLimited {
			get {
				HighLimited highLimited = new();
				GetField(highLimited);
				return highLimited;
			}
			set => SetField(value);
		}

		public LowLimited LowLimited {
			get {
				LowLimited lowLimited = new();
				GetField(lowLimited);
				return lowLimited;
			}
			set => SetField(value);
		}
	}
}