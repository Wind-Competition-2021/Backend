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
		/// Initialize controller with injected objects
		/// </summary>
		/// <param name="settings">Injected serializer settings</param>
		public BaoStockManager(JsonSerializerSettings settings) {
			string fetcherRoot = Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent!.FullName, "BaoStock");
			string venvPath = Path.Combine(fetcherRoot, "Venv");
			string pythonPath = Directory.GetDirectories(venvPath)
				.Select(path => Directory.GetFiles(path, "python*"))
				.Aggregate(null as string, (path, next) => next.Length > 0 ? next[0] : path);
			var processInfo = new ProcessStartInfo {
				FileName = $"\"{pythonPath ?? throw new FileNotFoundException("Python not found in Venv")}\"",
				Arguments = $"\"{Path.Combine(fetcherRoot, "BaoStock.py")}\"",
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				StandardOutputEncoding = Encoding.UTF8,
				RedirectStandardError = true
			};
			Process = Process.Start(processInfo);
			Process!.Exited += (_, _) => {
				Console.Error.WriteLine("BaoStock exited", Color.Red);
				if (!Process.StandardError.EndOfStream)
					Console.Error.WriteLine(Process.StandardError.ReadToEnd(), Color.Red);
				Process = Process.Start(processInfo);
			};
			SerializerSettings = settings;
		}

		/// <summary>
		/// Python fetcher process
		/// </summary>
		public Process Process { get; private set; }

		/// <summary>
		/// Serializer settings
		/// </summary>
		public JsonSerializerSettings SerializerSettings { get; }

		/// <summary>
		/// 
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