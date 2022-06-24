// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

using System;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Server.Models {
	/// <summary>
	/// 
	/// </summary>
	[DataContract]
	public class Solvency : StatementBase, IEquatable<Solvency> {
		/// <summary>
		/// Current Ratio
		/// </summary>
		/// <value>Current Ratio</value>
		[DataMember(Name = "cr")]
		public decimal? Cr { get; set; }

		/// <summary>
		/// Quick Ratio
		/// </summary>
		/// <value>Quick Ratio</value>
		[DataMember(Name = "qr")]
		public decimal? Qr { get; set; }

		/// <summary>
		/// Cash Asset Ratio
		/// </summary>
		/// <value>Cash Asset Ratio</value>
		[DataMember(Name = "car")]
		public decimal? Car { get; set; }

		/// <summary>
		/// Total Liabilities Growth Rate
		/// </summary>
		/// <value>Total Liabilities Growth Rate</value>
		[DataMember(Name = "tlgr")]
		public decimal? Tlgr { get; set; }

		/// <summary>
		/// Debt Asset Ratio
		/// </summary>
		/// <value>Debt Asset Ratio</value>
		[DataMember(Name = "dar")]
		public decimal? Dar { get; set; }

		/// <summary>
		/// Equity Multiplier
		/// </summary>
		/// <value>Equity Multiplier</value>
		[DataMember(Name = "em")]
		public decimal? Em { get; set; }

		/// <summary>
		/// Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class Solvency {\n");
			sb.Append("  Cr: ").Append(Cr).Append('\n');
			sb.Append("  Qr: ").Append(Qr).Append('\n');
			sb.Append("  Car: ").Append(Car).Append('\n');
			sb.Append("  Tlgr: ").Append(Tlgr).Append('\n');
			sb.Append("  Dar: ").Append(Dar).Append('\n');
			sb.Append("  Em: ").Append(Em).Append('\n');
			sb.Append("}\n");
			return sb.ToString();
		}

		/// <summary>
		/// Returns the JSON string presentation of the object
		/// </summary>
		/// <returns>JSON string presentation of the object</returns>
		public new string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

		/// <summary>
		/// Returns true if objects are equal
		/// </summary>
		/// <param name="obj">Object to be compared</param>
		/// <returns>Boolean</returns>
		public override bool Equals(object obj) {
			if (obj is null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((Solvency)obj);
		}

		/// <summary>
		/// Returns true if Solvency instances are equal
		/// </summary>
		/// <param name="other">Instance of Solvency to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(Solvency other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return
				(
					Cr == other.Cr ||
					Cr != null &&
					Cr.Equals(other.Cr)
				) &&
				(
					Qr == other.Qr ||
					Qr != null &&
					Qr.Equals(other.Qr)
				) &&
				(
					Car == other.Car ||
					Car != null &&
					Car.Equals(other.Car)
				) &&
				(
					Tlgr == other.Tlgr ||
					Tlgr != null &&
					Tlgr.Equals(other.Tlgr)
				) &&
				(
					Dar == other.Dar ||
					Dar != null &&
					Dar.Equals(other.Dar)
				) &&
				(
					Em == other.Em ||
					Em != null &&
					Em.Equals(other.Em)
				);
		}

		/// <summary>
		/// Gets the hash code
		/// </summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode() {
			unchecked// Overflow is fine, just wrap
			{
				int hashCode = 41;
				// Suitable nullity checks etc, of course :)
				if (Cr != null)
					hashCode = hashCode * 59 + Cr.GetHashCode();
				if (Qr != null)
					hashCode = hashCode * 59 + Qr.GetHashCode();
				if (Car != null)
					hashCode = hashCode * 59 + Car.GetHashCode();
				if (Tlgr != null)
					hashCode = hashCode * 59 + Tlgr.GetHashCode();
				if (Dar != null)
					hashCode = hashCode * 59 + Dar.GetHashCode();
				if (Em != null)
					hashCode = hashCode * 59 + Em.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(Solvency left, Solvency right) => Equals(left, right);

		public static bool operator !=(Solvency left, Solvency right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}