// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Server.Models {
	/// <summary>
	/// </summary>
	[DataContract]
	public class OperationalCapability : StatementBase, IEquatable<OperationalCapability> {
		/// <summary>
		///     Receivable Turnover Ratio
		/// </summary>
		/// <value>Receivable Turnover Ratio</value>
		[DataMember(Name = "rtr")]
		public decimal? Rtr { get; set; }

		/// <summary>
		///     Receivable Turnover Days
		/// </summary>
		/// <value>Receivable Turnover Days</value>
		[DataMember(Name = "rtd")]
		public decimal? Rtd { get; set; }

		/// <summary>
		///     Inventory Turnover Ratio
		/// </summary>
		/// <value>Inventory Turnover Ratio</value>
		[DataMember(Name = "itr")]
		public decimal? Itr { get; set; }

		/// <summary>
		///     Inventory Turnover Days
		/// </summary>
		/// <value>Inventory Turnover Days</value>
		[DataMember(Name = "itd")]
		public decimal? Itd { get; set; }

		/// <summary>
		///     Current Asset Turnover Ratio
		/// </summary>
		/// <value>Current Asset Turnover Ratio</value>
		[DataMember(Name = "catr")]
		public decimal? Catr { get; set; }

		/// <summary>
		///     Total Asset Turnover Ratio
		/// </summary>
		/// <value>Total Asset Turnover Ratio</value>
		[DataMember(Name = "tatr")]
		public decimal? Tatr { get; set; }

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class OperationalCapability {\n");
			sb.Append("  Rtr: ").Append(Rtr).Append('\n');
			sb.Append("  Rtd: ").Append(Rtd).Append('\n');
			sb.Append("  Itr: ").Append(Itr).Append('\n');
			sb.Append("  Itd: ").Append(Itd).Append('\n');
			sb.Append("  Catr: ").Append(Catr).Append('\n');
			sb.Append("  Tatr: ").Append(Tatr).Append('\n');
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
			return obj.GetType() == GetType() && Equals((OperationalCapability)obj);
		}

		/// <summary>
		///     Returns true if OperationalCapability instances are equal
		/// </summary>
		/// <param name="other">Instance of OperationalCapability to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(OperationalCapability other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return
				(
					Rtr == other.Rtr ||
					Rtr != null &&
					Rtr.Equals(other.Rtr)
				) &&
				(
					Rtd == other.Rtd ||
					Rtd != null &&
					Rtd.Equals(other.Rtd)
				) &&
				(
					Itr == other.Itr ||
					Itr != null &&
					Itr.Equals(other.Itr)
				) &&
				(
					Itd == other.Itd ||
					Itd != null &&
					Itd.Equals(other.Itd)
				) &&
				(
					Catr == other.Catr ||
					Catr != null &&
					Catr.Equals(other.Catr)
				) &&
				(
					Tatr == other.Tatr ||
					Tatr != null &&
					Tatr.Equals(other.Tatr)
				);
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
				if (Rtr != null)
					hashCode = hashCode * 59 + Rtr.GetHashCode();
				if (Rtd != null)
					hashCode = hashCode * 59 + Rtd.GetHashCode();
				if (Itr != null)
					hashCode = hashCode * 59 + Itr.GetHashCode();
				if (Itd != null)
					hashCode = hashCode * 59 + Itd.GetHashCode();
				if (Catr != null)
					hashCode = hashCode * 59 + Catr.GetHashCode();
				if (Tatr != null)
					hashCode = hashCode * 59 + Tatr.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(OperationalCapability left, OperationalCapability right) => Equals(left, right);

		public static bool operator !=(OperationalCapability left, OperationalCapability right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}