using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Shared;
using Tushare.Models;

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
			TradeCalendar,

			/// <summary>
			/// </summary>
			[EnumMember(Value = "stock_company")]
			CompanyInformation
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
		/// <param name="cache"></param>
		/// <param name="settings"></param>
		public TushareManager(string token, IMemoryCache cache = null, JsonSerializerSettings settings = null) {
			Token = token;
			Cache = cache ??
				new MemoryCache(
					new MemoryCacheOptions {
						SizeLimit = 4L << 30
					}
				);
			;
			SerializerSettings = settings;
		}

		private IMemoryCache Cache { get; }

		private JsonSerializerSettings SerializerSettings { get; }

		private HttpClient HttpClient { get; } = new();

		/// <summary>
		/// </summary>
		/// <typeparam name="TRes"></typeparam>
		/// <typeparam name="TParam"></typeparam>
		/// <param name="api"></param>
		/// <param name="parameters"></param>
		/// <param name="fields"></param>
		/// <returns></returns>
		public Task<ResponseWrapper<TRes>> SendRequest<TRes, TParam>(Api api, TParam parameters, string[] fields = null) {
			string reqBody = JsonConvert.SerializeObject(
				new RequestWrapper<TParam> {
					ApiName = api,
					Token = Token,
					Parameters = parameters,
					Fields = fields
				},
				SerializerSettings
			);
			return Cache.GetOrCreateAsync(
				reqBody,
				async entry => {
					var content = new StringContent(reqBody, Encoding.UTF8, "application/json");
					var resp = await HttpClient.PostAsync(Host, content);
					string respBody = await resp.Content.ReadAsStringAsync();
					entry.SetOptions(SelectCacheOptions(api));
					entry.Size = respBody.Length;
					return JsonConvert.DeserializeObject<ResponseWrapper<TRes>>(await resp.Content.ReadAsStringAsync(), SerializerSettings);
				}
			);
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="TRes"></typeparam>
		/// <param name="api"></param>
		/// <param name="parameters"></param>
		/// <param name="fields"></param>
		/// <returns></returns>
		public Task<ResponseWrapper<TRes>> SendRequest<TRes>(Api api, Dictionary<string, string> parameters, string[] fields = null) => SendRequest<TRes, Dictionary<string, string>>(api, parameters, fields);

		/// <summary>
		/// </summary>
		/// <param name="api"></param>
		/// <param name="parameters"></param>
		/// <param name="fields"></param>
		/// <returns></returns>
		public Task<ResponseWrapper<object>> SendRequest(Api api, Dictionary<string, string> parameters, string[] fields = null) => SendRequest<object>(api, parameters, fields);

		/// <summary>
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public async Task<bool> CheckTradeStatus(DateTime? date = null) {
			date ??= DateTime.Now;
			var response = await SendRequest(
				Api.TradeCalendar,
				new Dictionary<string, string> {
					{"start_date", date.Value.ToString("yyyyMMdd")},
					{"end_date", date.Value.ToString("yyyyMMdd")}
				},
				new[] {"is_open"}
			);
			return ((JValue)response.Data.Items[0][0]).Value?.Equals(1L) ?? throw new NullReferenceException();
		}

		public async Task<CompanyInformation> GetCompanyInformation(StockId id) {
			var response = await SendRequest<CompanyInformation>(
				Api.CompanyInformation,
				new Dictionary<string, string> {
					{"ts_code", id.ToString("t")}
				},
				new[] {"ts_code", "chairman", "manager", "secretary", "reg_capital", "setup_date", "province", "city", "introduction", "website", "email", "office", "employees", "main_business", "business_scope"}
			);
			return response.Data.Records?.FirstOrDefault();
		}

		private static MemoryCacheEntryOptions SelectCacheOptions(Api api)
			=> api switch {
				Api.TradeCalendar => new MemoryCacheEntryOptions {
					Priority = CacheItemPriority.High,
					SlidingExpiration = TimeSpan.FromDays(7)
				},
				Api.CompanyInformation => new MemoryCacheEntryOptions {
					Priority = CacheItemPriority.High,
					SlidingExpiration = TimeSpan.FromDays(7)
				},
				_ => new MemoryCacheEntryOptions {
					SlidingExpiration = TimeSpan.FromMinutes(30)
				}
			};

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