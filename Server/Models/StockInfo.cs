using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Server.Models {
	/// <summary>
	///     Detailed information of a stock
	/// </summary>
	[DataContract]
	public class StockInfo : StockBasicInfo, IEquatable<StockInfo> {
		/// <summary>
		///     Gets or Sets Type
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public enum TypeEnum {
			/// <summary>
			///     This security is a stock
			/// </summary>
			[EnumMember(Value = "stock")]
			StockEnum = 0,

			/// <summary>
			///     This security is an index
			/// </summary>
			[EnumMember(Value = "index")]
			IndexEnum = 1,

			/// <summary>
			///     Other types
			/// </summary>
			[EnumMember(Value = "other")]
			OtherEnum = 2
		}

		/// <summary>
		///     Gets or Sets Type
		/// </summary>
		[Required]
		[DataMember(Name = "type")]
		public TypeEnum? Type { get; set; }

		/// <summary>
		///     Gets or Sets Industry
		/// </summary>
		[DataMember(Name = "industry")]
		public string Industry { get; set; }

		/// <summary>
		///     Gets or Sets Classification
		/// </summary>
		[DataMember(Name = "classification")]
		public string Classification { get; set; }

		/// <summary>
		///     Gets or Sets ListedDate
		/// </summary>
		[Required]
		[DataMember(Name = "listedDate")]
		public DateTime? ListedDate { get; set; }

		/// <summary>
		///     Gets or Sets DelistedDate
		/// </summary>
		[DataMember(Name = "delistedDate")]
		public DateTime? DelistedDate { get; set; }

		/// <summary>
		///     Returns true if StockInfo instances are equal
		/// </summary>
		/// <param name="other">Instance of StockInfo to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(StockInfo other) {
			if (ReferenceEquals(null, other))
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
					Name == other.Name ||
					Name != null &&
					Name.Equals(other.Name)
				) &&
				(
					Type == other.Type ||
					Type != null &&
					Type.Equals(other.Type)
				) &&
				(
					Industry == other.Industry ||
					Industry != null &&
					Industry.Equals(other.Industry)
				) &&
				(
					Classification == other.Classification ||
					Classification != null &&
					Classification.Equals(other.Classification)
				) &&
				(
					ListedDate == other.ListedDate ||
					ListedDate != null &&
					ListedDate.Equals(other.ListedDate)
				) &&
				(
					DelistedDate == other.DelistedDate ||
					DelistedDate != null &&
					DelistedDate.Equals(other.DelistedDate)
				);
		}

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class StockInfo {\n");
			sb.Append("  Id: ").Append(Id).Append('\n');
			sb.Append("  Name: ").Append(Name).Append('\n');
			sb.Append("  Type: ").Append(Type).Append('\n');
			sb.Append("  Industry: ").Append(Industry).Append('\n');
			sb.Append("  Classification: ").Append(Classification).Append('\n');
			sb.Append("  ListedDate: ").Append(ListedDate).Append('\n');
			sb.Append("  DelistedDate: ").Append(DelistedDate).Append('\n');
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
			return obj.GetType() == GetType() && Equals((StockInfo)obj);
		}

		/// <summary>
		///     Gets the hash code
		/// </summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode() {
			unchecked// Overflow is fine, just wrap
			{
				var hashCode = 41;
				// Suitable nullity checks etc, of course :)
				if (Id != null)
					hashCode = hashCode * 59 + Id.GetHashCode();
				if (Name != null)
					hashCode = hashCode * 59 + Name.GetHashCode();
				if (Type != null)
					hashCode = hashCode * 59 + Type.GetHashCode();
				if (Industry != null)
					hashCode = hashCode * 59 + Industry.GetHashCode();
				if (Classification != null)
					hashCode = hashCode * 59 + Classification.GetHashCode();
				if (ListedDate != null)
					hashCode = hashCode * 59 + ListedDate.GetHashCode();
				if (DelistedDate != null)
					hashCode = hashCode * 59 + DelistedDate.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(StockInfo left, StockInfo right) => Equals(left, right);

		public static bool operator !=(StockInfo left, StockInfo right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}