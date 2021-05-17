using System;

namespace QuickFix.Fields {
	public class TDBeginDate : IntField {
		public const int FieldTag = 9052;

		public TDBeginDate() : base(FieldTag) { }
		public TDBeginDate(int val) : base(FieldTag, val) { }
		public TDBeginDate(DateTime datetime) : this(datetime.Year, datetime.Month, datetime.Day) { }

		public TDBeginDate(int year, int month, int day) : this(year * 10000 + month * 100 + day) { }
	}

	public class TDEndDate : IntField {
		public const int FieldTag = 9051;
		public TDEndDate() : base(FieldTag) { }
		public TDEndDate(int val) : base(FieldTag, val) { }
		public TDEndDate(DateTime datetime) : this(datetime.Year, datetime.Month, datetime.Day) { }

		public TDEndDate(int year, int month, int day) : this(year * 10000 + month * 100 + day) { }
	}

	public class WindCode : StringField {
		public const int FieldTag = 9001;
		public WindCode() : base(FieldTag) { }
		public WindCode(string str) : base(FieldTag, str) { }
	}

	public class TradingDay : StringField {
		public const int FieldTag = 9002;
		public TradingDay() : base(FieldTag) { }
		public TradingDay(string str) : base(FieldTag, str) { }
	}

	public class Time : StringField {
		public const int FieldTag = 9003;
		public Time() : base(FieldTag) { }
		public Time(string str) : base(FieldTag, str) { }
	}

	public class PreClose : StringField {
		public const int FieldTag = 9009;
		public PreClose() : base(FieldTag) { }
		public PreClose(string str) : base(FieldTag, str) { }
	}

	public class High : StringField {
		public const int FieldTag = 9010;
		public High() : base(FieldTag) { }
		public High(string str) : base(FieldTag, str) { }
	}

	public class Open : StringField {
		public const int FieldTag = 9011;
		public Open() : base(FieldTag) { }
		public Open(string str) : base(FieldTag, str) { }
	}

	public class Low : StringField {
		public const int FieldTag = 9012;
		public Low() : base(FieldTag) { }
		public Low(string str) : base(FieldTag, str) { }
	}

	public class Match : StringField {
		public const int FieldTag = 9013;
		public Match() : base(FieldTag) { }
		public Match(string str) : base(FieldTag, str) { }
	}

	public class AskPrice : StringField {
		public const int FieldTag = 9020;
		public AskPrice() : base(FieldTag) { }
		public AskPrice(string str) : base(FieldTag, str) { }
	}

	public class AskVol : StringField {
		public const int FieldTag = 9021;
		public AskVol() : base(FieldTag) { }
		public AskVol(string str) : base(FieldTag, str) { }
	}

	public class BidPrice : StringField {
		public const int FieldTag = 9022;
		public BidPrice() : base(FieldTag) { }
		public BidPrice(string str) : base(FieldTag, str) { }
	}

	public class BidVol : StringField {
		public const int FieldTag = 9023;
		public BidVol() : base(FieldTag) { }
		public BidVol(string str) : base(FieldTag, str) { }
	}

	public class NumTrades : StringField {
		public const int FieldTag = 9030;
		public NumTrades() : base(FieldTag) { }
		public NumTrades(string str) : base(FieldTag, str) { }
	}

	public class IVolumn : StringField {
		public const int FieldTag = 9031;
		public IVolumn() : base(FieldTag) { }
		public IVolumn(string str) : base(FieldTag, str) { }
	}

	public class ITurnover : StringField {
		public const int FieldTag = 9032;
		public ITurnover() : base(FieldTag) { }
		public ITurnover(string str) : base(FieldTag, str) { }
	}

	public class HighLimited : StringField {
		public const int FieldTag = 9040;
		public HighLimited() : base(FieldTag) { }
		public HighLimited(string str) : base(FieldTag, str) { }
	}

	public class LowLimited : StringField {
		public const int FieldTag = 9041;
		public LowLimited() : base(FieldTag) { }
		public LowLimited(string str) : base(FieldTag, str) { }
	}
}