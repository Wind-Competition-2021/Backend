using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Server.Models {
	/// <summary>
	/// </summary>
	[DataContract]
	public class StockInfo : StockBasicInfo, IEquatable<StockInfo> {
		/// <summary>
		///     Gets or Sets Type
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public enum SecurityType {
			/// <summary>
			///     Enum StockEnum for stock
			/// </summary>
			[EnumMember(Value = "stock")]
			Stock = 0,

			/// <summary>
			///     Enum IndexEnum for index
			/// </summary>
			[EnumMember(Value = "index")]
			Index = 1,

			/// <summary>
			///     Enum OtherEnum for other
			/// </summary>
			[EnumMember(Value = "other")]
			Other = 2
		}

		/// <summary>
		///     Gets or Sets Type
		/// </summary>
		[Required]
		[DataMember(Name = "type")]
		public SecurityType? Type { get; set; }

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
		///     Gets or Sets RegisteredCapital
		/// </summary>
		[DataMember(Name = "registeredCapital")]
		public string RegisteredCapital { get; set; }

		/// <summary>
		///     Gets or Sets LegalRepresentative
		/// </summary>
		[DataMember(Name = "legalRepresentative")]
		public string LegalRepresentative { get; set; }

		/// <summary>
		///     Gets or Sets GeneralManager
		/// </summary>
		[DataMember(Name = "generalManager")]
		public string GeneralManager { get; set; }

		/// <summary>
		///     Gets or Sets Secretary
		/// </summary>
		[DataMember(Name = "secretary")]
		public string Secretary { get; set; }

		/// <summary>
		///     Gets or Sets EmployeeCount
		/// </summary>
		[DataMember(Name = "employeeCount")]
		public int? EmployeeCount { get; set; }

		/// <summary>
		///     Gets or Sets Province
		/// </summary>
		[DataMember(Name = "province")]
		public string Province { get; set; }

		/// <summary>
		///     Gets or Sets City
		/// </summary>
		[DataMember(Name = "city")]
		public string City { get; set; }

		/// <summary>
		///     Gets or Sets Office
		/// </summary>
		[DataMember(Name = "office")]
		public string Office { get; set; }

		/// <summary>
		///     Gets or Sets Email
		/// </summary>
		[DataMember(Name = "email")]
		public string Email { get; set; }

		/// <summary>
		///     Gets or Sets Website
		/// </summary>
		[DataMember(Name = "website")]
		public string Website { get; set; }

		/// <summary>
		///     Gets or Sets BusinessScope
		/// </summary>
		[DataMember(Name = "businessScope")]
		public string BusinessScope { get; set; }

		/// <summary>
		///     Gets or Sets MainBusiness
		/// </summary>
		[DataMember(Name = "mainBusiness")]
		public string MainBusiness { get; set; }

		/// <summary>
		///     Gets or Sets Introduction
		/// </summary>
		[DataMember(Name = "introduction")]
		public string Introduction { get; set; }

		/// <summary>
		///     Returns true if StockInfo instances are equal
		/// </summary>
		/// <param name="other">Instance of StockInfo to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(StockInfo other) {
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
				) &&
				(
					RegisteredCapital == other.RegisteredCapital ||
					RegisteredCapital != null &&
					RegisteredCapital.Equals(other.RegisteredCapital)
				) &&
				(
					LegalRepresentative == other.LegalRepresentative ||
					LegalRepresentative != null &&
					LegalRepresentative.Equals(other.LegalRepresentative)
				) &&
				(
					GeneralManager == other.GeneralManager ||
					GeneralManager != null &&
					GeneralManager.Equals(other.GeneralManager)
				) &&
				(
					Secretary == other.Secretary ||
					Secretary != null &&
					Secretary.Equals(other.Secretary)
				) &&
				(
					EmployeeCount == other.EmployeeCount ||
					EmployeeCount != null &&
					EmployeeCount.Equals(other.EmployeeCount)
				) &&
				(
					Province == other.Province ||
					Province != null &&
					Province.Equals(other.Province)
				) &&
				(
					City == other.City ||
					City != null &&
					City.Equals(other.City)
				) &&
				(
					Office == other.Office ||
					Office != null &&
					Office.Equals(other.Office)
				) &&
				(
					Email == other.Email ||
					Email != null &&
					Email.Equals(other.Email)
				) &&
				(
					Website == other.Website ||
					Website != null &&
					Website.Equals(other.Website)
				) &&
				(
					BusinessScope == other.BusinessScope ||
					BusinessScope != null &&
					BusinessScope.Equals(other.BusinessScope)
				) &&
				(
					MainBusiness == other.MainBusiness ||
					MainBusiness != null &&
					MainBusiness.Equals(other.MainBusiness)
				) &&
				(
					Introduction == other.Introduction ||
					Introduction != null &&
					Introduction.Equals(other.Introduction)
				);
		}

		/// <summary>
		///     Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("class StockInfo {\n");
			sb.Append("  Type: ").Append(Type).Append('\n');
			sb.Append("  Industry: ").Append(Industry).Append('\n');
			sb.Append("  Classification: ").Append(Classification).Append('\n');
			sb.Append("  ListedDate: ").Append(ListedDate).Append('\n');
			sb.Append("  DelistedDate: ").Append(DelistedDate).Append('\n');
			sb.Append("  RegisteredCapital: ").Append(RegisteredCapital).Append('\n');
			sb.Append("  LegalRepresentative: ").Append(LegalRepresentative).Append('\n');
			sb.Append("  GeneralManager: ").Append(GeneralManager).Append('\n');
			sb.Append("  Secretary: ").Append(Secretary).Append('\n');
			sb.Append("  EmployeeCount: ").Append(EmployeeCount).Append('\n');
			sb.Append("  Province: ").Append(Province).Append('\n');
			sb.Append("  City: ").Append(City).Append('\n');
			sb.Append("  Office: ").Append(Office).Append('\n');
			sb.Append("  Email: ").Append(Email).Append('\n');
			sb.Append("  Website: ").Append(Website).Append('\n');
			sb.Append("  BusinessScope: ").Append(BusinessScope).Append('\n');
			sb.Append("  MainBusiness: ").Append(MainBusiness).Append('\n');
			sb.Append("  Introduction: ").Append(Introduction).Append('\n');
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
				int hashCode = 41;
				// Suitable nullity checks etc, of course :)
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
				if (RegisteredCapital != null)
					hashCode = hashCode * 59 + RegisteredCapital.GetHashCode();
				if (LegalRepresentative != null)
					hashCode = hashCode * 59 + LegalRepresentative.GetHashCode();
				if (GeneralManager != null)
					hashCode = hashCode * 59 + GeneralManager.GetHashCode();
				if (Secretary != null)
					hashCode = hashCode * 59 + Secretary.GetHashCode();
				if (EmployeeCount != null)
					hashCode = hashCode * 59 + EmployeeCount.GetHashCode();
				if (Province != null)
					hashCode = hashCode * 59 + Province.GetHashCode();
				if (City != null)
					hashCode = hashCode * 59 + City.GetHashCode();
				if (Office != null)
					hashCode = hashCode * 59 + Office.GetHashCode();
				if (Email != null)
					hashCode = hashCode * 59 + Email.GetHashCode();
				if (Website != null)
					hashCode = hashCode * 59 + Website.GetHashCode();
				if (BusinessScope != null)
					hashCode = hashCode * 59 + BusinessScope.GetHashCode();
				if (MainBusiness != null)
					hashCode = hashCode * 59 + MainBusiness.GetHashCode();
				if (Introduction != null)
					hashCode = hashCode * 59 + Introduction.GetHashCode();
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