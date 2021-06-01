using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Tushare {
	/// <summary>
	/// </summary>
	public class TushareManager {
		/// <summary>
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public enum Api {
			/// <summary>
			/// </summary>
			[EnumMember(Value = "trade_cal")]
			TradeCalendar
		}

		/// <summary>
		/// </summary>
		public const string Host = "http://api.waditu.com";

		/// <summary>
		/// </summary>
		public readonly string Token;

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <param name="settings"></param>
		public TushareManager(string token, JsonSerializerSettings settings = null) {
			Token = token;
			SerializerSettings = settings;
		}

		private JsonSerializerSettings SerializerSettings { get; }

		private HttpClient HttpClient { get; } = new();

		/// <summary>
		/// </summary>
		/// <typeparam name="TRes"></typeparam>
		/// <typeparam name="TParam"></typeparam>
		/// <param name="apiName"></param>
		/// <param name="parameters"></param>
		/// <param name="fields"></param>
		/// <returns></returns>
		public async Task<ResponseWrapper<TRes>> SendRequest<TRes, TParam>(Api apiName, TParam parameters, string[] fields = null) {
			var content = new StringContent(
				JsonConvert.SerializeObject(
					new RequestWrapper<TParam> {
						ApiName = apiName,
						Token = Token,
						Parameters = parameters,
						Fields = fields
					},
					SerializerSettings
				),
				Encoding.UTF8,
				"application/json"
			);
			var resp = await HttpClient.PostAsync(Host, content);
			return JsonConvert.DeserializeObject<ResponseWrapper<TRes>>(await resp.Content.ReadAsStringAsync(), SerializerSettings);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TRes"></typeparam>
		/// <param name="apiName"></param>
		/// <param name="parameters"></param>
		/// <param name="fields"></param>
		/// <returns></returns>
		public Task<ResponseWrapper<TRes>> SendRequest<TRes>(Api apiName, Dictionary<string, string> parameters, string[] fields = null) => SendRequest<TRes, Dictionary<string, string>>(apiName, parameters, fields);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="apiName"></param>
		/// <param name="parameters"></param>
		/// <param name="fields"></param>
		/// <returns></returns>
		public Task<ResponseWrapper<object>> SendRequest(Api apiName, Dictionary<string, string> parameters, string[] fields = null) => SendRequest<object>(apiName, parameters, fields);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public async Task<bool> CheckTradeStatus(DateTime? date = null) {
			date ??= DateTime.Now;
			var response = await SendRequest(
				Api.TradeCalendar,
				new Dictionary<string, string>() {
					{"start_date", date.Value.ToString("yyyyMMdd")},
					{"end_date", date.Value.ToString("yyyyMMdd")}
				},
				new[] {"is_open"}
			);
			return ((JValue)response.Data.Items[0][0]).Value?.Equals(1L) ?? throw new NullReferenceException();
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		[DataContract]
		public class RequestWrapper<T> {
			/// <summary>
			/// </summary>
			[DataMember(Name = "api_name", IsRequired = true)]
			public Api ApiName;

			/// <summary>
			/// </summary>
			[DataMember(Name = "fields")]
			public string[] Fields;

			/// <summary>
			/// </summary>
			[DataMember(Name = "params")]
			public T Parameters;

			/// <summary>
			/// </summary>
			[DataMember(Name = "token", IsRequired = true)]
			public string Token;
		}

		/// <summary>
		///     Tushare response wrapper
		/// </summary>
		[DataContract]
		public class ResponseWrapper<T> {
			/// <summary>
			/// </summary>
			[DataMember(Name = "code")]
			public int Code { get; set; }

			/// <summary>
			/// </summary>
			[DataMember(Name = "msg")]
			public string Message { get; set; }

			/// <summary>
			/// </summary>
			[DataMember(Name = "data")]
			public ResponseData Data { get; set; }

			/// <summary>
			/// </summary>
			[DataContract]
			public class ResponseData {
				/// <summary>
				/// </summary>
				[DataMember(Name = "fields")]
				public string[] Fields { get; set; }

				/// <summary>
				/// </summary>
				[DataMember(Name = "items")]
				public JArray[] Items { get; set; }

				/// <summary>
				/// </summary>
				[DataMember(Name = "has_more")]
				public bool HasMore { get; set; }

				/// <summary>
				/// 
				/// </summary>
				[JsonIgnore]
				public List<T> Records {
					get {
						var records = new List<T>(Items.Length);
						var dict = Fields.ToDictionary<string, string, JValue>(field => field, _ => null);
						foreach (var item in Items) {
							for (int i = 0; i < Fields.Length; ++i)
								dict[Fields[i]] = item[i] as JValue;
							records.Add(JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(dict)));
						}
						return records;
					}
				}

				/// <summary>
				/// 
				/// </summary>
				[JsonIgnore]
				public List<Dictionary<string, object>> Dictionaries {
					get {
						var dicts = new List<Dictionary<string, object>>(Items.Length);
						foreach (var item in Items) {
							var dict = new Dictionary<string, object>();
							for (int i = 0; i < Fields.Length; ++i)
								dict[Fields[i]] = ((JValue)item[i]).Value;
							dicts.Add(dict);
						}
						return dicts;
					}
				}
			}
		}
	}
}