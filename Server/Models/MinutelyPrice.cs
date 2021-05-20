using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Server.Models {
	/// <summary>
	/// </summary>
	[DataContract]
	public class MinutelyPrice : PriceBase, IEquatable<MinutelyPrice> {
		/// <summary>
		///     Gets or Sets Rehabilitation
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public enum RehabilitationEnum {
			/// <summary>
			///     Enum PreEnum for pre
			/// </summary>
			[EnumMember(Value = "pre")]
			PreEnum = 0,

			/// <summary>
			///     Enum PostEnum for post
			/// </summary>
			[EnumMember(Value = "post")]
			PostEnum = 1,

			/// <summary>
			///     Enum NoneEnum for none
			/// </summary>
			[EnumMember(Value = "none")]
			NoneEnum = 2
		}

		/// <summary>
		///     Gets or Sets Time
		/// </summary>
		[DataMember(Name = "time")]
		public DateTime? Time { get; }

		/// <summary>
		///     Gets or Sets Rehabilitation
		/// </summary>
		[DataMember(Name = "rehabilitation")]
		public RehabilitationEnum? Rehabilitation { get; }

		/// <summary>
		///     Returns true if MinutelyPrice instances are equal
		/// </summary>
		/// <param name="other">Instance of MinutelyPrice to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(MinutelyPrice other) {
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return
				(
					Time == other.Time ||
					Time != null &&
					Time.Equals(other.Time)
				) &&
				(
					Rehabilitation == other.Rehabilitation ||
					Rehabilitation != null &&
					Rehabilitation.Equals(other.Rehabilitation)
				);
		}

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class MinutelyPrice {\n");
			sb.Append("  Time: ").Append(Time).Append('\n');
			sb.Append("  Rehabilitation: ").Append(Rehabilitation).Append('\n');
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
			return obj.GetType() == GetType() && Equals((MinutelyPrice)obj);
		}

		/// <summary>
		///     Gets the hash code
		/// </summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode() {
			unchecked {
				var hashCode = 41;
				// Suitable nullity checks etc, of course :)
				if (Time != null)
					hashCode = hashCode * 59 + Time.GetHashCode();
				if (Rehabilitation != null)
					hashCode = hashCode * 59 + Rehabilitation.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(MinutelyPrice left, MinutelyPrice right) => Equals(left, right);

		public static bool operator !=(MinutelyPrice left, MinutelyPrice right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}