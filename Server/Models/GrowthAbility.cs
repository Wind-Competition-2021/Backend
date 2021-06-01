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
	public class GrowthAbility : StatementBase, IEquatable<GrowthAbility> {
		/// <summary>
		///     Net Asset Growth Rate
		/// </summary>
		/// <value>Net Asset Growth Rate</value>
		[DataMember(Name = "nagr")]
		public string Nagr { get; set; }

		/// <summary>
		///     Total Asset Growth Rate
		/// </summary>
		/// <value>Total Asset Growth Rate</value>
		[DataMember(Name = "tagr")]
		public string Tagr { get; set; }

		/// <summary>
		///     Net Profit Growth Rate
		/// </summary>
		/// <value>Net Profit Growth Rate</value>
		[DataMember(Name = "npgr")]
		public string Npgr { get; set; }

		/// <summary>
		///     Basic Earnings Per Stock Growth Rate
		/// </summary>
		/// <value>Basic Earnings Per Stock Growth Rate</value>
		[DataMember(Name = "bepsgr")]
		public string Bepsgr { get; set; }

		/// <summary>
		///     Net Profit Attributable to Shareholders Growth Rate
		/// </summary>
		/// <value>Net Profit Attributable to Shareholders Growth Rate</value>
		[DataMember(Name = "npasgr")]
		public string Npasgr { get; set; }

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class GrowthAbility {\n");
			sb.Append("  Nagr: ").Append(Nagr).Append('\n');
			sb.Append("  Tagr: ").Append(Tagr).Append('\n');
			sb.Append("  Npgr: ").Append(Npgr).Append('\n');
			sb.Append("  Bepsgr: ").Append(Bepsgr).Append('\n');
			sb.Append("  Npasgr: ").Append(Npasgr).Append('\n');
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
			return obj.GetType() == GetType() && Equals((GrowthAbility)obj);
		}

		/// <summary>
		///     Returns true if GrowthAbility instances are equal
		/// </summary>
		/// <param name="other">Instance of GrowthAbility to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(GrowthAbility other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return
				(
					Nagr == other.Nagr ||
					Nagr != null &&
					Nagr.Equals(other.Nagr)
				) &&
				(
					Tagr == other.Tagr ||
					Tagr != null &&
					Tagr.Equals(other.Tagr)
				) &&
				(
					Npgr == other.Npgr ||
					Npgr != null &&
					Npgr.Equals(other.Npgr)
				) &&
				(
					Bepsgr == other.Bepsgr ||
					Bepsgr != null &&
					Bepsgr.Equals(other.Bepsgr)
				) &&
				(
					Npasgr == other.Npasgr ||
					Npasgr != null &&
					Npasgr.Equals(other.Npasgr)
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
				if (Nagr != null)
					hashCode = hashCode * 59 + Nagr.GetHashCode();
				if (Tagr != null)
					hashCode = hashCode * 59 + Tagr.GetHashCode();
				if (Npgr != null)
					hashCode = hashCode * 59 + Npgr.GetHashCode();
				if (Bepsgr != null)
					hashCode = hashCode * 59 + Bepsgr.GetHashCode();
				if (Npasgr != null)
					hashCode = hashCode * 59 + Npasgr.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(GrowthAbility left, GrowthAbility right) => Equals(left, right);

		public static bool operator !=(GrowthAbility left, GrowthAbility right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}