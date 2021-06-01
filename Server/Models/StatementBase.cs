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
	public class StatementBase : IEquatable<StatementBase> {
		/// <summary>
		/// Gets or Sets Id
		/// </summary>
		[JsonConverter(typeof(StockIdConverter))]
		[DataMember(Name = "id")]
		public StockId Id { get; set; }

		/// <summary>
		/// Gets or Sets PublishDate
		/// </summary>
		[DataMember(Name = "publishDate")]
		public DateTime? PublishDate { get; set; }

		/// <summary>
		/// Gets or Sets StatDate
		/// </summary>
		[DataMember(Name = "statDate")]
		public DateTime? StatDate { get; set; }

		/// <summary>
		/// Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class StatementBase {\n");
			sb.Append("  Id: ").Append(Id).Append('\n');
			sb.Append("  PublishDate: ").Append(PublishDate).Append('\n');
			sb.Append("  StatDate: ").Append(StatDate).Append('\n');
			sb.Append("}\n");
			return sb.ToString();
		}

		/// <summary>
		/// Returns the JSON string presentation of the object
		/// </summary>
		/// <returns>JSON string presentation of the object</returns>
		public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

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
			return obj.GetType() == GetType() && Equals((StatementBase)obj);
		}

		/// <summary>
		/// Returns true if StatementBase instances are equal
		/// </summary>
		/// <param name="other">Instance of StatementBase to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(StatementBase other) {
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
					PublishDate == other.PublishDate ||
					PublishDate != null &&
					PublishDate.Equals(other.PublishDate)
				) &&
				(
					StatDate == other.StatDate ||
					StatDate != null &&
					StatDate.Equals(other.StatDate)
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
				if (Id != null)
					hashCode = hashCode * 59 + Id.GetHashCode();
				if (PublishDate != null)
					hashCode = hashCode * 59 + PublishDate.GetHashCode();
				if (StatDate != null)
					hashCode = hashCode * 59 + StatDate.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(StatementBase left, StatementBase right) => Equals(left, right);

		public static bool operator !=(StatementBase left, StatementBase right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}