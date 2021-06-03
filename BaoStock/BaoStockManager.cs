using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BaoStock {
	public class BaoStockManager {
		/// <summary>
		///     Initialize controller with injected objects
		/// </summary>
		/// <param name="settings">Injected serializer settings</param>
		public BaoStockManager(JsonSerializerSettings settings) {
			string curRoot = Directory.GetCurrentDirectory();
			Process = new Process() {
				StartInfo = new ProcessStartInfo {
					FileName = $"py",
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
			SerializerSettings = settings;
		}

		/// <summary>
		///     Python fetcher process
		/// </summary>
		public Process Process { get; private set; }

		/// <summary>
		///     Serializer settings
		/// </summary>
		public JsonSerializerSettings SerializerSettings { get; }

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="apiName"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public T Fetch<T>(string apiName, params string[] args) {
			T result;
			string command = args == null ? apiName : $"{apiName} {string.Join(" ", args)}".TrimEnd();
			lock (Process) {
				Process.StandardInput.WriteLine(command);
				string json = Process.StandardOutput.ReadLine();
				result = JsonConvert.DeserializeObject<T>(json ?? string.Empty, SerializerSettings);
			}
			return result;
		}
	}
}