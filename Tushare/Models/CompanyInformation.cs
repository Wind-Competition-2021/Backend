using System.Runtime.Serialization;
using Newtonsoft.Json;
using Shared;

namespace Tushare.Models {
	[DataContract]
	public class CompanyInformation {
		[JsonConverter(typeof(StockIdConverter))]
		[DataMember(Name = "ts_code")]
		public StockId Id { get; set; }

		[DataMember(Name = "chairman")]
		public string LegalRepresentative { get; set; }

		[DataMember(Name = "manager")]
		public string GeneralManager { get; set; }

		[DataMember(Name = "secretary")]
		public string Secretary { get; set; }

		[DataMember(Name = "reg_capital")]
		public string RegisterCapital { get; set; }

		[DataMember(Name = "setup_date")]
		public string SetupDate { get; set; }

		[DataMember(Name = "province")]
		public string Province { get; set; }

		[DataMember(Name = "city")]
		public string City { get; set; }

		[DataMember(Name = "introduction")]
		public string Introduction { get; set; }

		[DataMember(Name = "website")]
		public string Website { get; set; }

		[DataMember(Name = "email")]
		public string Email { get; set; }

		[DataMember(Name = "office")]
		public string Office { get; set; }

		[DataMember(Name = "employees")]
		public int? EmployeeCount { get; set; }

		[DataMember(Name = "main_business")]
		public string MainBusiness { get; set; }

		[DataMember(Name = "business_scope")]
		public string BusinessScope { get; set; }
	}
}