using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared;

namespace Server.Models {
	/// <summary>
	///     Rehabilitation adjustment type
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum RehabilitationType {
		/// <summary>
		///     Forward rehabilitation
		/// </summary>
		[EnumMember(Value = "pre")]
		PreEnum = 0,

		/// <summary>
		///     Backward rehabilitation
		/// </summary>
		[EnumMember(Value = "post")]
		PostEnum = 1,

		/// <summary>
		///     No rehabilitation
		/// </summary>
		[EnumMember(Value = "none")]
		NoneEnum = 2
	}

	/// <summary>
	/// </summary>
	[DataContract]
	public class PriceBase : IEquatable<PriceBase> {
		/// <summary>
		///     Id of the stock, example: sh.600001
		/// </summary>
		[JsonConverter(typeof(StockIdConverter))]
		[DataMember(Name = "id")]
		public StockId Id { get; set; }

		/// <summary>
		///     Opening price
		/// </summary>
		[DataMember(Name = "opening")]
		public int? Opening { get; set; }

		/// <summary>
		///     Closing price, alias current price
		/// </summary>
		[DataMember(Name = "closing")]
		public int? Closing { get; set; }

		/// <summary>
		///     Highest price
		/// </summary>
		[DataMember(Name = "highest")]
		public int? Highest { get; set; }

		/// <summary>
		///     Lowest price
		/// </summary>
		[DataMember(Name = "lowest")]
		public int? Lowest { get; set; }

		/// <summary>
		///     Total volume
		/// </summary>
		[DataMember(Name = "volume")]
		public long? Volume { get; set; }

		/// <summary>
		///     Total turnover
		/// </summary>
		[DataMember(Name = "turnover")]
		public long? Turnover { get; set; }

		/// <summary>
		///     Returns true if PriceBase instances are equal
		/// </summary>
		/// <param name="other">Instance of PriceBase to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(PriceBase other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return
				(
					Id == other.Id ||
					Id != null &&
					Id.Equals(other.Id)
				) &&
				(
					Opening == other.Opening ||
					Opening != null &&
					Opening.Equals(other.Opening)
				) &&
				(
					Closing == other.Closing ||
					Closing != null &&
					Closing.Equals(other.Closing)
				) &&
				(
					Highest == other.Highest ||
					Highest != null &&
					Highest.Equals(other.Highest)
				) &&
				(
					Lowest == other.Lowest ||
					Lowest != null &&
					Lowest.Equals(other.Lowest)
				) &&
				(
					Volume == other.Volume ||
					Volume != null &&
					Volume.Equals(other.Volume)
				) &&
				(
					Turnover == other.Turnover ||
					Turnover != null &&
					Turnover.Equals(other.Turnover)
				);
		}

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class PriceBase {\n");
			sb.Append("  Id: ").Append(Id).Append('\n');
			sb.Append("  Opening: ").Append(Opening).Append('\n');
			sb.Append("  Closing: ").Append(Closing).Append('\n');
			sb.Append("  Highest: ").Append(Highest).Append('\n');
			sb.Append("  Lowest: ").Append(Lowest).Append('\n');
			sb.Append("  Volume: ").Append(Volume).Append('\n');
			sb.Append("  Turnover: ").Append(Turnover).Append('\n');
			sb.Append("}\n");
			return sb.ToString();
		}

		/// <summary>
		///     Returns the JSON string presentation of the object
		/// </summary>
		/// <returns>JSON string presentation of the object</returns>
		public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

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
			return obj.GetType() == GetType() && Equals((PriceBase)obj);
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
				if (Id != null)
					hashCode = hashCode * 59 + Id.GetHashCode();
				if (Opening != null)
					hashCode = hashCode * 59 + Opening.GetHashCode();
				if (Closing != null)
					hashCode = hashCode * 59 + Closing.GetHashCode();
				if (Highest != null)
					hashCode = hashCode * 59 + Highest.GetHashCode();
				if (Lowest != null)
					hashCode = hashCode * 59 + Lowest.GetHashCode();
				if (Volume != null)
					hashCode = hashCode * 59 + Volume.GetHashCode();
				if (Turnover != null)
					hashCode = hashCode * 59 + Turnover.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(PriceBase left, PriceBase right) => Equals(left, right);

		public static bool operator !=(PriceBase left, PriceBase right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}