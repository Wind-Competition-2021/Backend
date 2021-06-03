using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Shared;

namespace Server.Models {
	/// <summary>
	///     User configuration
	/// </summary>
	[DataContract]
	public class Configuration : IEquatable<Configuration> {
		/// <summary>
		///     The list of stocks pinned by user
		/// </summary>
		[JsonProperty(ItemConverterType = typeof(StockIdConverter))]
		[DataMember(Name = "pinnedStocks")]
		public List<StockId> PinnedStocks { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember(Name = "playbackSpeed")]
		[Range(1, int.MaxValue)]
		public int? PlaybackSpeed { get; set; }

		/// <summary>
		///     List and trend refresh interval
		/// </summary>
		[DataMember(Name = "refreshInterval")]
		public RefreshInterval RefreshInterval { get; set; }

		/// <summary>
		///     Returns true if Configuration instances are equal
		/// </summary>
		/// <param name="other">Instance of Configuration to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(Configuration other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return
				(
					PinnedStocks == other.PinnedStocks ||
					PinnedStocks != null &&
					PinnedStocks.SequenceEqual(other.PinnedStocks)
				) &&
				(
					RefreshInterval == other.RefreshInterval ||
					RefreshInterval != null &&
					RefreshInterval.Equals(other.RefreshInterval)
				);
		}

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class Configuration {\n");
			sb.Append("  PinnedStocks: ").Append(PinnedStocks).Append('\n');
			sb.Append("  RefreshInterval: ").Append(RefreshInterval).Append('\n');
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
			return obj.GetType() == GetType() && Equals((Configuration)obj);
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
				if (PinnedStocks != null)
					hashCode = hashCode * 59 + PinnedStocks.GetHashCode();
				if (RefreshInterval != null)
					hashCode = hashCode * 59 + RefreshInterval.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(Configuration left, Configuration right) => Equals(left, right);

		public static bool operator !=(Configuration left, Configuration right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}