using System;
using System.Runtime.Serialization;
using System.Text;
using Initiator;
using Newtonsoft.Json;

namespace Server.Models {
	/// <summary>
	///     Used for Realtime quotes
	/// </summary>
	[DataContract]
	public class RealtimePrice : PriceBase, IEquatable<RealtimePrice> {
		/// <summary>
		///     Default constructor
		/// </summary>
		public RealtimePrice() { }

		/// <summary>
		/// Base copy constructor
		/// </summary>
		/// <param name="other"></param>
		public RealtimePrice(PriceBase other) : base(other) { }

		/// <summary>
		/// </summary>
		/// <param name="quote"></param>
		/// <param name="id"></param>
		public RealtimePrice(Quote quote, string id = null) {
			if (!string.IsNullOrEmpty(id))
				Id = id;
			Opening = (int)(quote.OpeningPrice * 10000m);
			Closing = (int)(quote.ClosingPrice * 10000m);
			Highest = (int)(quote.HighestPrice * 10000m);
			Lowest = (int)(quote.LowestPrice * 10000m);
			Volume = quote.TotalVolume;
			Turnover = (long)(quote.TotalTurnover * 10000m);
			Time = quote.TradingTime;
			PreClosing = (int)(quote.PreClosingPrice * 10000m);
		}

		/// <summary>
		///     Gets or Sets Time
		/// </summary>
		[DataMember(Name = "time")]
		public DateTime? Time { get; set; }

		/// <summary>
		///     Gets or Sets PreClosing
		/// </summary>
		[DataMember(Name = "preClosing")]
		public int? PreClosing { get; set; }

		/// <summary>
		///     Whether the stock is in user's pinned list
		/// </summary>
		[DataMember(Name = "pinned")]
		public bool Pinned { get; set; }

		/// <summary>
		///     Returns true if RealTimePrice instances are equal
		/// </summary>
		/// <param name="other">Instance of RealTimePrice to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(RealtimePrice other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return
				Equals((PriceBase)other) &&
				(Time == other.Time || Time != null && Time.Equals(other.Time)) &&
				(PreClosing == other.PreClosing || PreClosing != null && PreClosing.Equals(other.PreClosing)) &&
				Pinned == other.Pinned;
		}

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class RealTimePrice {\n");
			sb.Append("  Time: ").Append(Time).Append('\n');
			sb.Append("  PreClosing: ").Append(PreClosing).Append('\n');
			sb.Append("  Pinned: ").Append(Pinned).Append('\n');
			sb.Append("}\n");
			return sb.ToString();
		}

		/// <summary>
		///     Returns the JSON string presentation of the object
		/// </summary>
		/// <returns>JSON string presentation of the object</returns>
		public new string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

		/// <summary>
		///     Returns true if objects are equal
		/// </summary>
		/// <param name="obj">Object to be compared</param>
		/// <returns>Boolean</returns>
		public override bool Equals(object obj) {
			if (obj is null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((RealtimePrice)obj);
		}

		/// <summary>
		///     Gets the hash code
		/// </summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode() {
			unchecked// Overflow is fine, just wrap
			{
				int hashCode = 41;
				// Suitable nullity checks etc, of course :)
				if (Time != null)
					hashCode = hashCode * 59 + Time.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(RealtimePrice left, RealtimePrice right) => Equals(left, right);

		public static bool operator !=(RealtimePrice left, RealtimePrice right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}