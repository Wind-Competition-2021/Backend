using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Server.Models {
	/// <summary>
	///     Refresh interval, used in Configuration
	/// </summary>
	[DataContract]
	public class ConfigurationRefreshInterval : IEquatable<ConfigurationRefreshInterval> {
		/// <summary>
		///     Realtime stock list refreshment interval, unit: second
		/// </summary>
		[DataMember(Name = "list")]
		public int? List { get; set; }

		/// <summary>
		///     Realtime single stock trend refreshment interval, unit: second
		/// </summary>
		[DataMember(Name = "single")]
		public int? Single { get; set; }

		/// <summary>
		///     Returns true if ConfigurationRefreshInterval instances are equal
		/// </summary>
		/// <param name="other">Instance of ConfigurationRefreshInterval to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(ConfigurationRefreshInterval other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return
				(
					List == other.List ||
					List != null &&
					List.Equals(other.List)
				) &&
				(
					Single == other.Single ||
					Single != null &&
					Single.Equals(other.Single)
				);
		}

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class ConfigurationRefreshInterval {\n");
			sb.Append("  _List: ").Append(List).Append('\n');
			sb.Append("  Single: ").Append(Single).Append('\n');
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
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((ConfigurationRefreshInterval)obj);
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
				if (List != null)
					hashCode = hashCode * 59 + List.GetHashCode();
				if (Single != null)
					hashCode = hashCode * 59 + Single.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(ConfigurationRefreshInterval left, ConfigurationRefreshInterval right) => Equals(left, right);

		public static bool operator !=(ConfigurationRefreshInterval left, ConfigurationRefreshInterval right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}