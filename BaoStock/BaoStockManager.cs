using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Shared;

namespace BaoStock {
	public class BaoStockManager {
		public enum Api : byte {
			[EnumMember(Value = "getStockList")]
			StockList,

			[EnumMember(Value = "getStockInfo")]
			StockInfo,

			[EnumMember(Value = "getMinutelyPrice")]
			MinutelyPrice,

			[EnumMember(Value = "getDailyPrice")]
			DailyPrice,

			[EnumMember(Value = "getWeeklyPrice")]
			WeeklyPrice,

			[EnumMember(Value = "checkTradeStatus")]
			TradeStatus,

			[EnumMember(Value = "getTradeCalendar")]
			TradeCalendar,

			[EnumMember(Value = "getProfitability")]
			Profitability,

			[EnumMember(Value = "getOperationalCapability")]
			OperationalCapability,

			[EnumMember(Value = "getGrowthAbility")]
			GrowthAbility,

			[EnumMember(Value = "getSolvency")]
			Solvency,

			[EnumMember(Value = "getCashFlow")]
			CashFlow,

			[EnumMember(Value = "getPerformanceReport")]
			PerformanceReport,

			[EnumMember(Value = "getPerformanceForecast")]
			PerformanceForecast,

			[EnumMember(Value = "exit")]
			Exit
		}

		/// <summary>
		///     Initialize controller with injected objects
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="settings">Injected serializer settings</param>
		public BaoStockManager(IMemoryCache cache = null, JsonSerializerSettings settings = null) {
			string curRoot = Directory.GetCurrentDirectory();
			Process = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "py",
					Arguments = $"-3.9 \"{Path.Combine(curRoot, "BaoStock.py")}\"",
					UseShellExecute = false,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					StandardOutputEncoding = Encoding.UTF8,
					RedirectStandardError = true
				}
			};
			Process.Exited += (_, _) => {
				Console.Error.WriteLine("BaoStock exited", Color.Red);
				if (!Process.StandardError.EndOfStream)
					Console.Error.WriteLine(Process.StandardError.ReadToEnd(), Color.Red);
				Process.Start();
			};
			Process.Start();
			Cache = cache ??
				new MemoryCache(
					new MemoryCacheOptions {
						SizeLimit = 4L << 30
					}
				);
			SerializerSettings = settings;
		}

		/// <summary>
		///     Python fetcher process
		/// </summary>
		public Process Process { get; }

		/// <summary>
		/// </summary>
		public IMemoryCache Cache { get; }

		/// <summary>
		///     Serializer settings
		/// </summary>
		public JsonSerializerSettings SerializerSettings { get; }

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="api"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public T Fetch<T>(Api api, params string[] args) {
			string apiName = api.GetAttribute<EnumMemberAttribute>().Value;
			string command = args == null ? apiName : $"{apiName} {string.Join(" ", args)}".TrimEnd();
			var result = Cache.GetOrCreate(
				command,
				entry => {
					lock (Process) {
						Process.StandardInput.WriteLine(command);
						string json = Process.StandardOutput.ReadLine();
						entry.SetOptions(SelectCacheOptions(api));
						entry.Size = json?.Length;
						return JsonConvert.DeserializeObject<T>(json ?? string.Empty, SerializerSettings);
					}
				}
			);
			return result;
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="api"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public T Fetch<T>(Api api, params object[] args) {
			string[] arguments = args.Select(
					argument => argument switch {
						DateTime arg => arg.ToString("yyyy-MM-dd"),
						StockId arg  => arg.ToString("b"),
						null         => null,
						_            => argument.ToString()
					}
				)
				.ToArray();
			return Fetch<T>(api, arguments);
		}

		private static MemoryCacheEntryOptions SelectCacheOptions(Api api)
			=> api switch {
				Api.StockList => new MemoryCacheEntryOptions {
					Priority = CacheItemPriority.NeverRemove
				},
				Api.StockInfo => new MemoryCacheEntryOptions {
					Priority = CacheItemPriority.High,
					SlidingExpiration = TimeSpan.FromDays(7)
				},
				Api.Profitability or Api.OperationalCapability or Api.GrowthAbility or Api.CashFlow or Api.Solvency => new MemoryCacheEntryOptions {
					Priority = CacheItemPriority.High,
					SlidingExpiration = TimeSpan.FromDays(7)
				},
				Api.TradeStatus => new MemoryCacheEntryOptions {
					AbsoluteExpiration = DateTime.SpecifyKind(DateTime.Now.Date.AddDays(1), DateTimeKind.Local)
				},
				_ => new MemoryCacheEntryOptions {
					SlidingExpiration = TimeSpan.FromDays(1)
				}
			};
	}
}