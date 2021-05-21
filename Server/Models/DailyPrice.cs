using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Server.Models {
	/// <summary>
	/// </summary>
	[DataContract]
	public class DailyPrice : PriceBase, IEquatable<DailyPrice> {
		/// <summary>
		///     Gets or Sets Date
		/// </summary>
		[DataMember(Name = "date")]
		public DateTime? Date { get; set; }

		/// <summary>
		///     Gets or Sets PreClosing
		/// </summary>
		[DataMember(Name = "preClosing")]
		public int? PreClosing { get; set; }

		/// <summary>
		///     Gets or Sets Rehabilitation
		/// </summary>
		[DataMember(Name = "rehabilitation")]
		public RehabilitationType? Rehabilitation { get; set; }

		/// <summary>
		///     Gets or Sets TurnoverRate
		/// </summary>
		[DataMember(Name = "turnoverRate")]
		public int? TurnoverRate { get; set; }

		/// <summary>
		///     price/earnings ratio
		/// </summary>
		/// <value>price/earnings ratio</value>
		[DataMember(Name = "per")]
		public int? Per { get; set; }

		/// <summary>
		///     price to sales ratio
		/// </summary>
		/// <value>price to sales ratio</value>
		[DataMember(Name = "psr")]
		public int? Psr { get; set; }

		/// <summary>
		///     price cash flow ratio
		/// </summary>
		/// <value>price cash flow ratio</value>
		[DataMember(Name = "pcfr")]
		public int? Pcfr { get; set; }

		/// <summary>
		///     price to book ratio
		/// </summary>
		/// <value>price to book ratio</value>
		[DataMember(Name = "pbr")]
		public int? Pbr { get; set; }

		/// <summary>
		///     Gets or Sets Stopped
		/// </summary>
		[DataMember(Name = "stopped")]
		public bool? Stopped { get; set; }

		/// <summary>
		///     Gets or Sets SpecialTreatment
		/// </summary>
		[DataMember(Name = "specialTreatment")]
		public bool? SpecialTreatment { get; set; }

		/// <summary>
		///     Returns true if DailyPrice instances are equal
		/// </summary>
		/// <param name="other">Instance of DailyPrice to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(DailyPrice other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return
				Equals((PriceBase)other) &&
				(
					Date == other.Date ||
					Date != null &&
					Date.Equals(other.Date)
				) &&
				(
					PreClosing == other.PreClosing ||
					PreClosing != null &&
					PreClosing.Equals(other.PreClosing)
				) &&
				(
					Rehabilitation == other.Rehabilitation ||
					Rehabilitation != null &&
					Rehabilitation.Equals(other.Rehabilitation)
				) &&
				(
					TurnoverRate == other.TurnoverRate ||
					TurnoverRate != null &&
					TurnoverRate.Equals(other.TurnoverRate)
				) &&
				(
					Per == other.Per ||
					Per != null &&
					Per.Equals(other.Per)
				) &&
				(
					Psr == other.Psr ||
					Psr != null &&
					Psr.Equals(other.Psr)
				) &&
				(
					Pcfr == other.Pcfr ||
					Pcfr != null &&
					Pcfr.Equals(other.Pcfr)
				) &&
				(
					Pbr == other.Pbr ||
					Pbr != null &&
					Pbr.Equals(other.Pbr)
				) &&
				(
					Stopped == other.Stopped ||
					Stopped != null &&
					Stopped.Equals(other.Stopped)
				) &&
				(
					SpecialTreatment == other.SpecialTreatment ||
					SpecialTreatment != null &&
					SpecialTreatment.Equals(other.SpecialTreatment)
				);
		}

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class DailyPrice {\n");
			sb.Append("  Date: ").Append(Date).Append('\n');
			sb.Append("  PreClosing: ").Append(PreClosing).Append('\n');
			sb.Append("  Rehabilitation: ").Append(Rehabilitation).Append('\n');
			sb.Append("  TurnoverRate: ").Append(TurnoverRate).Append('\n');
			sb.Append("  Per: ").Append(Per).Append('\n');
			sb.Append("  Psr: ").Append(Psr).Append('\n');
			sb.Append("  Pcfr: ").Append(Pcfr).Append('\n');
			sb.Append("  Pbr: ").Append(Pbr).Append('\n');
			sb.Append("  Stopped: ").Append(Stopped).Append('\n');
			sb.Append("  SpecialTreatment: ").Append(SpecialTreatment).Append('\n');
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
			return obj.GetType() == GetType() && Equals((DailyPrice)obj);
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
				if (Date != null)
					hashCode = hashCode * 59 + Date.GetHashCode();
				if (Rehabilitation != null)
					hashCode = hashCode * 59 + Rehabilitation.GetHashCode();
				if (TurnoverRate != null)
					hashCode = hashCode * 59 + TurnoverRate.GetHashCode();
				if (Per != null)
					hashCode = hashCode * 59 + Per.GetHashCode();
				if (Psr != null)
					hashCode = hashCode * 59 + Psr.GetHashCode();
				if (Pcfr != null)
					hashCode = hashCode * 59 + Pcfr.GetHashCode();
				if (Pbr != null)
					hashCode = hashCode * 59 + Pbr.GetHashCode();
				if (Stopped != null)
					hashCode = hashCode * 59 + Stopped.GetHashCode();
				if (SpecialTreatment != null)
					hashCode = hashCode * 59 + SpecialTreatment.GetHashCode();
				return hashCode;
			}
		}

		#region Operators
		#pragma warning disable 1591

		public static bool operator ==(DailyPrice left, DailyPrice right) => Equals(left, right);

		public static bool operator !=(DailyPrice left, DailyPrice right) => !Equals(left, right);

		#pragma warning restore 1591
		#endregion Operators
	}
}