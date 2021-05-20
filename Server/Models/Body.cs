using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Server.Models {
	/// <summary>
	/// </summary>
	[DataContract]
	public class Body : Configuration, IEquatable<Body> {
		/// <summary>
		///     Returns true if Body instances are equal
		/// </summary>
		/// <param name="other">Instance of Body to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(Body other) {
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return false;
		}

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class Body {\n");
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
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((Body)obj);
		}

		/// <summary>
		///     Gets the hash code
		/// </summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode() {
			const int hashCode = 41;
			// Suitable nullity checks etc, of course :)
			return hashCode;
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(Body left, Body right) => Equals(left, right);

		public static bool operator !=(Body left, Body right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}