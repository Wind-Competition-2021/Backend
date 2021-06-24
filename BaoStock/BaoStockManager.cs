using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using StackExchange.Redis;
using Newtonsoft.Json;
using Shared;
using Console = Colorful.Console;

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
		/// <param name="redis"></param>
		/// <param name="settings">Injected serializer settings</param>
		public BaoStockManager(IDatabase redis = null, JsonSerializerSettings settings = null) {
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
			Redis = redis ?? ConnectionMultiplexer.Connect("localhost:6379").GetDatabase();
			SerializerSettings = settings;
		}

		/// <summary>
		///     Python fetcher process
		/// </summary>
		public Process Process { get; }

		/// <summary>
		/// </summary>
		public IDatabase Redis { get; }

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
			if (Redis.KeyExists(command)) {
				Console.WriteLine($"[Cached(BaoStock)] {command}", Color.Yellow);
				Redis.KeyExpireAsync(command, SelectCacheOptions(api));
				return Redis.ObjectGet<T>(command);
			}
			else
				lock (Process) {
					Process.StandardInput.WriteLine(command);
					string json = Process.StandardOutput.ReadLine();
					var result = JsonConvert.DeserializeObject<T>(json ?? string.Empty, SerializerSettings);
					Redis.ObjectSetAsync(command, result).ContinueWith(_ => Redis.KeyExpireAsync(command, SelectCacheOptions(api)));
					return result;
				}
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

		private static ExpirationDate? SelectCacheOptions(Api api)
			=> api switch {
				Api.StockList                                                                                       => null,
				Api.StockInfo                                                                                       => new ExpirationDate(TimeSpan.FromDays(7)),
				Api.Profitability or Api.OperationalCapability or Api.GrowthAbility or Api.CashFlow or Api.Solvency => new ExpirationDate(TimeSpan.FromDays(7)),
				Api.TradeStatus                                                                                     => new ExpirationDate(DateTime.Now.Date.AddDays(1)),
				_                                                                                                   => new ExpirationDate(TimeSpan.FromDays(1))
			};
	}
}