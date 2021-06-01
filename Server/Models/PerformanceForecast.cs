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
	public class PerformanceForecast : StatementBase, IEquatable<PerformanceForecast> {
		/// <summary>
		///     Gets or Sets Type
		/// </summary>
		[DataMember(Name = "type")]
		public string Type { get; set; }

		/// <summary>
		///     Gets or Sets _Abstract
		/// </summary>
		[DataMember(Name = "abstract")]
		public string Abstract { get; set; }

		/// <summary>
		///     Gets or Sets NpasgrUpperLimit
		/// </summary>
		[DataMember(Name = "npasgrUpperLimit")]
		public decimal? NpasgrUpperLimit { get; set; }

		/// <summary>
		///     Gets or Sets NpasgrLowerLimit
		/// </summary>
		[DataMember(Name = "npasgrLowerLimit")]
		public decimal? NpasgrLowerLimit { get; set; }

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class PerformanceForecast {\n");
			sb.Append("  Type: ").Append(Type).Append('\n');
			sb.Append("  _Abstract: ").Append(Abstract).Append('\n');
			sb.Append("  NpasgrUpperLimit: ").Append(NpasgrUpperLimit).Append('\n');
			sb.Append("  NpasgrLowerLimit: ").Append(NpasgrLowerLimit).Append('\n');
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
			return obj.GetType() == GetType() && Equals((PerformanceForecast)obj);
		}

		/// <summary>
		///     Returns true if PerformanceForecast instances are equal
		/// </summary>
		/// <param name="other">Instance of PerformanceForecast to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(PerformanceForecast other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return
				(
					Type == other.Type ||
					Type != null &&
					Type.Equals(other.Type)
				) &&
				(
					Abstract == other.Abstract ||
					Abstract != null &&
					Abstract.Equals(other.Abstract)
				) &&
				(
					NpasgrUpperLimit == other.NpasgrUpperLimit ||
					NpasgrUpperLimit != null &&
					NpasgrUpperLimit.Equals(other.NpasgrUpperLimit)
				) &&
				(
					NpasgrLowerLimit == other.NpasgrLowerLimit ||
					NpasgrLowerLimit != null &&
					NpasgrLowerLimit.Equals(other.NpasgrLowerLimit)
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
				if (Type != null)
					hashCode = hashCode * 59 + Type.GetHashCode();
				if (Abstract != null)
					hashCode = hashCode * 59 + Abstract.GetHashCode();
				if (NpasgrUpperLimit != null)
					hashCode = hashCode * 59 + NpasgrUpperLimit.GetHashCode();
				if (NpasgrLowerLimit != null)
					hashCode = hashCode * 59 + NpasgrLowerLimit.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(PerformanceForecast left, PerformanceForecast right) => Equals(left, right);

		public static bool operator !=(PerformanceForecast left, PerformanceForecast right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}