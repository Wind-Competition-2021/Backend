using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Server.Models {
	/// <summary>
	///     Stock basic information
	/// </summary>
	[DataContract]
	public class StockBasicInfo : IEquatable<StockBasicInfo> {
		/// <summary>
		///     Stock id
		/// </summary>
		[Required]
		[DataMember(Name = "id")]
		public string Id { get; set; }

		/// <summary>
		///     Stock name
		/// </summary>
		[Required]
		[DataMember(Name = "name")]
		public string Name { get; set; }

		/// <summary>
		///     Returns true if StockBasicInfo instances are equal
		/// </summary>
		/// <param name="other">Instance of StockBasicInfo to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(StockBasicInfo other) {
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
					Name == other.Name ||
					Name != null &&
					Name.Equals(other.Name)
				);
		}

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class StockBasicInfo {\n");
			sb.Append("  Id: ").Append(Id).Append('\n');
			sb.Append("  Name: ").Append(Name).Append('\n');
			sb.Append("}\n");
			return sb.ToString();
		}

		/// <summary>
		///     Returns the JSON string presentation of the object
		/// </summary>
		/// <returns>JSON string presentation of the object</returns>
		public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

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
			return obj.GetType() == GetType() && Equals((StockBasicInfo)obj);
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
				if (Id != null)
					hashCode = hashCode * 59 + Id.GetHashCode();
				if (Name != null)
					hashCode = hashCode * 59 + Name.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(StockBasicInfo left, StockBasicInfo right) => Equals(left, right);

		public static bool operator !=(StockBasicInfo left, StockBasicInfo right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}