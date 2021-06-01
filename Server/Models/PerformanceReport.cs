// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Server.Models {
	/// <summary>
	/// 
	/// </summary>
	[DataContract]
	public class PerformanceReport : StatementBase, IEquatable<PerformanceReport> {
		/// <summary>
		/// Gets or Sets UpdateDate
		/// </summary>
		[DataMember(Name = "updateDate")]
		public DateTime? UpdateDate { get; set; }

		/// <summary>
		/// Total Assets
		/// </summary>
		/// <value>Total Assets</value>
		[DataMember(Name = "ta")]
		public decimal? Ta { get; set; }

		/// <summary>
		/// Net Assets
		/// </summary>
		/// <value>Net Assets</value>
		[DataMember(Name = "na")]
		public decimal? Na { get; set; }

		/// <summary>
		/// Earnings Per Stock Growth Rate
		/// </summary>
		/// <value>Earnings Per Stock Growth Rate</value>
		[DataMember(Name = "epsgr")]
		public decimal? Epsgr { get; set; }

		/// <summary>
		/// Rate Of Equity Weighted
		/// </summary>
		/// <value>Rate Of Equity Weighted</value>
		[DataMember(Name = "roew")]
		public decimal? Roew { get; set; }

		/// <summary>
		/// Earnings Per Stock Diluted
		/// </summary>
		/// <value>Earnings Per Stock Diluted</value>
		[DataMember(Name = "epsd")]
		public decimal? Epsd { get; set; }

		/// <summary>
		/// Growth Revenue Growth Rate
		/// </summary>
		/// <value>Growth Revenue Growth Rate</value>
		[DataMember(Name = "grgr")]
		public decimal? Grgr { get; set; }

		/// <summary>
		/// Operating Profit Growth Rate
		/// </summary>
		/// <value>Operating Profit Growth Rate</value>
		[DataMember(Name = "opgr")]
		public decimal? Opgr { get; set; }

		/// <summary>
		/// Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class PerformanceReport {\n");
			sb.Append("  UpdateDate: ").Append(UpdateDate).Append('\n');
			sb.Append("  Ta: ").Append(Ta).Append('\n');
			sb.Append("  Na: ").Append(Na).Append('\n');
			sb.Append("  Epsgr: ").Append(Epsgr).Append('\n');
			sb.Append("  Roew: ").Append(Roew).Append('\n');
			sb.Append("  Epsd: ").Append(Epsd).Append('\n');
			sb.Append("  Grgr: ").Append(Grgr).Append('\n');
			sb.Append("  Opgr: ").Append(Opgr).Append('\n');
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
			return obj.GetType() == GetType() && Equals((PerformanceReport)obj);
		}

		/// <summary>
		/// Returns true if PerformanceReport instances are equal
		/// </summary>
		/// <param name="other">Instance of PerformanceReport to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(PerformanceReport other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return
				(
					UpdateDate == other.UpdateDate ||
					UpdateDate != null &&
					UpdateDate.Equals(other.UpdateDate)
				) &&
				(
					Ta == other.Ta ||
					Ta != null &&
					Ta.Equals(other.Ta)
				) &&
				(
					Na == other.Na ||
					Na != null &&
					Na.Equals(other.Na)
				) &&
				(
					Epsgr == other.Epsgr ||
					Epsgr != null &&
					Epsgr.Equals(other.Epsgr)
				) &&
				(
					Roew == other.Roew ||
					Roew != null &&
					Roew.Equals(other.Roew)
				) &&
				(
					Epsd == other.Epsd ||
					Epsd != null &&
					Epsd.Equals(other.Epsd)
				) &&
				(
					Grgr == other.Grgr ||
					Grgr != null &&
					Grgr.Equals(other.Grgr)
				) &&
				(
					Opgr == other.Opgr ||
					Opgr != null &&
					Opgr.Equals(other.Opgr)
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
				if (UpdateDate != null)
					hashCode = hashCode * 59 + UpdateDate.GetHashCode();
				if (Ta != null)
					hashCode = hashCode * 59 + Ta.GetHashCode();
				if (Na != null)
					hashCode = hashCode * 59 + Na.GetHashCode();
				if (Epsgr != null)
					hashCode = hashCode * 59 + Epsgr.GetHashCode();
				if (Roew != null)
					hashCode = hashCode * 59 + Roew.GetHashCode();
				if (Epsd != null)
					hashCode = hashCode * 59 + Epsd.GetHashCode();
				if (Grgr != null)
					hashCode = hashCode * 59 + Grgr.GetHashCode();
				if (Opgr != null)
					hashCode = hashCode * 59 + Opgr.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(PerformanceReport left, PerformanceReport right) => Equals(left, right);

		public static bool operator !=(PerformanceReport left, PerformanceReport right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}