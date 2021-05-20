/*
 * StockQuotes
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: 0.1
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System;
using System.Runtime.Serialization;
using System.Text;
using Initiator;
using Newtonsoft.Json;

namespace Server.Models {
	/// <summary>
	/// </summary>
	[DataContract]
	public class RealTimePrice : PriceBase, IEquatable<RealTimePrice> {
		/// <summary>
		///     Default constructor
		/// </summary>
		public RealTimePrice() { }

		/// <summary>
		/// </summary>
		/// <param name="quote"></param>
		/// <param name="id"></param>
		public RealTimePrice(Quote quote, string id = null) {
			if (!string.IsNullOrEmpty(id))
				Id = id;
			Opening = (int)(quote.OpeningPrice * 10000m);
			Closing = (int)(quote.ClosingPrice * 10000m);
			Highest = (int)(quote.HighestPrice * 10000m);
			Lowest = (int)(quote.LowestPrice * 10000m);
			Volume = quote.TotalVolume;
			Turnover = (long)quote.TotalTurnover;
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
		/// </summary>
		[DataMember(Name = "pinned")]
		public bool Pinned { get; set; }

		/// <summary>
		///     Returns true if RealTimePrice instances are equal
		/// </summary>
		/// <param name="other">Instance of RealTimePrice to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(RealTimePrice other) {
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
			return obj.GetType() == GetType() && Equals((RealTimePrice)obj);
		}

		/// <summary>
		///     Gets the hash code
		/// </summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode() {
			unchecked// Overflow is fine, just wrap
			{
				var hashCode = 41;
				// Suitable nullity checks etc, of course :)
				if (Time != null)
					hashCode = hashCode * 59 + Time.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(RealTimePrice left, RealTimePrice right) => Equals(left, right);

		public static bool operator !=(RealTimePrice left, RealTimePrice right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}