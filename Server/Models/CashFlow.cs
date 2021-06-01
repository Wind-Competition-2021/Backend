using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Server.Models {
	/// <summary>
	/// </summary>
	[DataContract]
	public class CashFlow : StatementBase, IEquatable<CashFlow> {
		/// <summary>
		///     Current Assets to Total Assets Ratio
		/// </summary>
		/// <value>Current Assets to Total Assets Ratio</value>
		[DataMember(Name = "catar")]
		public decimal? Catar { get; set; }

		/// <summary>
		///     Fixed Assets to Total Assets Ratio
		/// </summary>
		/// <value>Fixed Assets to Total Assets Ratio</value>
		[DataMember(Name = "fatar")]
		public decimal? Fatar { get; set; }

		/// <summary>
		///     Tangible Assets to Total Assets Ratio
		/// </summary>
		/// <value>Tangible Assets to Total Assets Ratio</value>
		[DataMember(Name = "tatar")]
		public decimal? Tatar { get; set; }

		/// <summary>
		///     Interest Protection Multiples
		/// </summary>
		/// <value>Interest Protection Multiples</value>
		[DataMember(Name = "ipm")]
		public decimal? Ipm { get; set; }

		/// <summary>
		///     Operating Net Cash Flow to Operating Revenue Ratio
		/// </summary>
		/// <value>Operating Net Cash Flow to Operating Revenue Ratio</value>
		[DataMember(Name = "oncforr")]
		public decimal? Oncforr { get; set; }

		/// <summary>
		///     Operating Net Cash Flow to Net Profit Ratio
		/// </summary>
		/// <value>Operating Net Cash Flow to Net Profit Ratio</value>
		[DataMember(Name = "oncfnpr")]
		public decimal? Oncfnpr { get; set; }

		/// <summary>
		///     Operating Net Cash Flow to Gross Revenue Ratio
		/// </summary>
		/// <value>Operating Net Cash Flow to Gross Revenue Ratio</value>
		[DataMember(Name = "oncfgrr")]
		public decimal? Oncfgrr { get; set; }

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class CashFlow {\n");
			sb.Append("  Catar: ").Append(Catar).Append('\n');
			sb.Append("  Fatar: ").Append(Fatar).Append('\n');
			sb.Append("  Tatar: ").Append(Tatar).Append('\n');
			sb.Append("  Ipm: ").Append(Ipm).Append('\n');
			sb.Append("  Oncforr: ").Append(Oncforr).Append('\n');
			sb.Append("  Oncfnpr: ").Append(Oncfnpr).Append('\n');
			sb.Append("  Oncfgrr: ").Append(Oncfgrr).Append('\n');
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
			return obj.GetType() == GetType() && Equals((CashFlow)obj);
		}

		/// <summary>
		///     Returns true if CashFlow instances are equal
		/// </summary>
		/// <param name="other">Instance of CashFlow to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(CashFlow other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return
				(
					Catar == other.Catar ||
					Catar != null &&
					Catar.Equals(other.Catar)
				) &&
				(
					Fatar == other.Fatar ||
					Fatar != null &&
					Fatar.Equals(other.Fatar)
				) &&
				(
					Tatar == other.Tatar ||
					Tatar != null &&
					Tatar.Equals(other.Tatar)
				) &&
				(
					Ipm == other.Ipm ||
					Ipm != null &&
					Ipm.Equals(other.Ipm)
				) &&
				(
					Oncforr == other.Oncforr ||
					Oncforr != null &&
					Oncforr.Equals(other.Oncforr)
				) &&
				(
					Oncfnpr == other.Oncfnpr ||
					Oncfnpr != null &&
					Oncfnpr.Equals(other.Oncfnpr)
				) &&
				(
					Oncfgrr == other.Oncfgrr ||
					Oncfgrr != null &&
					Oncfgrr.Equals(other.Oncfgrr)
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
				if (Catar != null)
					hashCode = hashCode * 59 + Catar.GetHashCode();
				if (Fatar != null)
					hashCode = hashCode * 59 + Fatar.GetHashCode();
				if (Tatar != null)
					hashCode = hashCode * 59 + Tatar.GetHashCode();
				if (Ipm != null)
					hashCode = hashCode * 59 + Ipm.GetHashCode();
				if (Oncforr != null)
					hashCode = hashCode * 59 + Oncforr.GetHashCode();
				if (Oncfnpr != null)
					hashCode = hashCode * 59 + Oncfnpr.GetHashCode();
				if (Oncfgrr != null)
					hashCode = hashCode * 59 + Oncfgrr.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(CashFlow left, CashFlow right) => Equals(left, right);

		public static bool operator !=(CashFlow left, CashFlow right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}