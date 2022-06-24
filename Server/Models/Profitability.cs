// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Server.Models {
	/// <summary>
	/// </summary>
	[DataContract]
	public class Profitability : StatementBase, IEquatable<Profitability> {
		/// <summary>
		///     Return On Equity
		/// </summary>
		/// <value>Return On Equity</value>
		[DataMember(Name = "roe")]
		public decimal? Roe { get; set; }

		/// <summary>
		///     Net Profit Margin
		/// </summary>
		/// <value>Net Profit Margin</value>
		[DataMember(Name = "npm")]
		public decimal? Npm { get; set; }

		/// <summary>
		///     Gross Profit Margin
		/// </summary>
		/// <value>Gross Profit Margin</value>
		[DataMember(Name = "gpm")]
		public decimal? Gpm { get; set; }

		/// <summary>
		///     Net Profit
		/// </summary>
		/// <value>Net Profit</value>
		[DataMember(Name = "np")]
		public long? Np { get; set; }

		/// <summary>
		///     Earnings Per Share
		/// </summary>
		/// <value>Earnings Per Share</value>
		[DataMember(Name = "eps")]
		public decimal? Eps { get; set; }

		/// <summary>
		///     Main Business Revenue
		/// </summary>
		/// <value>Main Business Revenue</value>
		[DataMember(Name = "mbr")]
		public long? Mbr { get; set; }

		/// <summary>
		///     Total Shares
		/// </summary>
		/// <value>Totala Shares</value>
		[DataMember(Name = "ts")]
		public long? Ts { get; set; }

		/// <summary>
		///     Circulating Shares
		/// </summary>
		/// <value>Circulating Shares</value>
		[DataMember(Name = "cs")]
		public long? Cs { get; set; }

		/// <summary>
		///     Returns true if Profitability instances are equal
		/// </summary>
		/// <param name="other">Instance of Profitability to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(Profitability other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return
				(
					Roe == other.Roe ||
					Roe != null &&
					Roe.Equals(other.Roe)
				) &&
				(
					Npm == other.Npm ||
					Npm != null &&
					Npm.Equals(other.Npm)
				) &&
				(
					Gpm == other.Gpm ||
					Gpm != null &&
					Gpm.Equals(other.Gpm)
				) &&
				(
					Np == other.Np ||
					Np != null &&
					Np.Equals(other.Np)
				) &&
				(
					Eps == other.Eps ||
					Eps != null &&
					Eps.Equals(other.Eps)
				) &&
				(
					Mbr == other.Mbr ||
					Mbr != null &&
					Mbr.Equals(other.Mbr)
				) &&
				(
					Ts == other.Ts ||
					Ts != null &&
					Ts.Equals(other.Ts)
				) &&
				(
					Cs == other.Cs ||
					Cs != null &&
					Cs.Equals(other.Cs)
				);
		}

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class Profitability {\n");
			sb.Append("  Roe: ").Append(Roe).Append('\n');
			sb.Append("  Npm: ").Append(Npm).Append('\n');
			sb.Append("  Gpm: ").Append(Gpm).Append('\n');
			sb.Append("  Np: ").Append(Np).Append('\n');
			sb.Append("  Eps: ").Append(Eps).Append('\n');
			sb.Append("  Mbr: ").Append(Mbr).Append('\n');
			sb.Append("  Ts: ").Append(Ts).Append('\n');
			sb.Append("  Cs: ").Append(Cs).Append('\n');
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
			return obj.GetType() == GetType() && Equals((Profitability)obj);
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
				if (Roe != null)
					hashCode = hashCode * 59 + Roe.GetHashCode();
				if (Npm != null)
					hashCode = hashCode * 59 + Npm.GetHashCode();
				if (Gpm != null)
					hashCode = hashCode * 59 + Gpm.GetHashCode();
				if (Np != null)
					hashCode = hashCode * 59 + Np.GetHashCode();
				if (Eps != null)
					hashCode = hashCode * 59 + Eps.GetHashCode();
				if (Mbr != null)
					hashCode = hashCode * 59 + Mbr.GetHashCode();
				if (Ts != null)
					hashCode = hashCode * 59 + Ts.GetHashCode();
				if (Cs != null)
					hashCode = hashCode * 59 + Cs.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(Profitability left, Profitability right) => Equals(left, right);

		public static bool operator !=(Profitability left, Profitability right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}