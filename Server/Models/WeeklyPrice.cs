using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Server.Models {
	/// <summary>
	///     Weekly quote
	/// </summary>
	[DataContract]
	public class WeeklyPrice : PriceBase, IEquatable<WeeklyPrice> {
		/// <summary>
		///     Gets or Sets Date
		/// </summary>
		[DataMember(Name = "date")]
		public DateTime? Date { get; set; }

		/// <summary>
		///     Gets or Sets Rehabilitation
		/// </summary>
		[DataMember(Name = "rehabilitation")]
		public RehabilitationType? Rehabilitation { get; set; }

		/// <summary>
		///     Gets or Sets TurnoverRate
		/// </summary>
		[DataMember(Name = "turnoverRate")]
		public decimal? TurnoverRate { get; set; }

		/// <summary>
		///     Returns true if WeeklyPrice instances are equal
		/// </summary>
		/// <param name="other">Instance of WeeklyPrice to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(WeeklyPrice other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return
				(
					Date == other.Date ||
					Date != null &&
					Date.Equals(other.Date)
				) &&
				(
					Rehabilitation == other.Rehabilitation ||
					Rehabilitation != null &&
					Rehabilitation.Equals(other.Rehabilitation)
				) &&
				(
					TurnoverRate == other.TurnoverRate ||
					TurnoverRate != null &&
					TurnoverRate.Equals(other.TurnoverRate)
				);
		}

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class WeeklyPrice {\n");
			sb.Append("  Date: ").Append(Date).Append('\n');
			sb.Append("  Rehabilitation: ").Append(Rehabilitation).Append('\n');
			sb.Append("  TurnoverRate: ").Append(TurnoverRate).Append('\n');
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
			return obj.GetType() == GetType() && Equals((WeeklyPrice)obj);
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
				if (Date != null)
					hashCode = hashCode * 59 + Date.GetHashCode();
				if (Rehabilitation != null)
					hashCode = hashCode * 59 + Rehabilitation.GetHashCode();
				if (TurnoverRate != null)
					hashCode = hashCode * 59 + TurnoverRate.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(WeeklyPrice left, WeeklyPrice right) => Equals(left, right);

		public static bool operator !=(WeeklyPrice left, WeeklyPrice right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}