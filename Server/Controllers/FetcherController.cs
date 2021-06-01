using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Server.Controllers {
	/// <summary>
	/// 
	/// </summary>
	[ApiController]
	public class FetcherController : ControllerBase {
		/// <summary>
		/// Initialize controller with injected objects
		/// </summary>
		/// <param name="fetcher">Injected python fetcher process</param>
		/// <param name="settings">Injected serializer settings</param>
		protected FetcherController(Process fetcher, JsonSerializerSettings settings) {
			Fetcher = fetcher;
			SerializerSettings = settings;
		}

		/// <summary>
		/// Python fetcher process
		/// </summary>
		protected Process Fetcher { get; }

		/// <summary>
		/// Serializer settings
		/// </summary>
		protected JsonSerializerSettings SerializerSettings { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="apiName"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		protected T Fetch<T>(string apiName, params string[] args) {
			T result;
			string command = args == null ? apiName : $"{apiName} {string.Join(" ", args)}".TrimEnd();
			lock (Fetcher) {
				Fetcher.StandardInput.WriteLine(command);
				string json = Fetcher.StandardOutput.ReadLine();
				result = JsonConvert.DeserializeObject<T>(json, SerializerSettings);
			}
			return result;
		}
	}
}