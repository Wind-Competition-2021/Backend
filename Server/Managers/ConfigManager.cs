using System;
using System.Collections.Generic;
using System.Text;
using Server.Models;

namespace Server.Managers {
	/// <summary>
	/// </summary>
	public class ConfigManager {
		private static readonly Random Random = new();
		private static readonly string CharSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

		/// <summary>
		/// </summary>
		public static Configuration DefaultConfiguration
			=> new() {
				PinnedStocks = new List<string>(),
				RefreshInterval = new ConfigurationRefreshInterval {
					List = 5,
					Single = 5
				}
			};

		/// <summary>
		/// </summary>
		public static string RandomToken {
			get {
				StringBuilder builder = new();
				builder.Clear();
				for (var i = 0; i < 16; ++i)
					builder.Append(CharSet[Random.Next() % CharSet.Length]);
				return builder.ToString();
			}
		}

		/// <summary>
		/// </summary>
		protected Dictionary<string, Configuration> Configurations { get; } = new();

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public Configuration this[string token] {
			get => Configurations[token];
			set => Configurations[token] = value;
		}

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public bool Contains(string token) => Configurations.ContainsKey(token);

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <param name="config"></param>
		public void Add(string token, Configuration config = null) => Configurations.Add(token, config ?? DefaultConfiguration);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public string GetNewToken() {
			var token = RandomToken;
			while (Contains(token))
				token = RandomToken;
			return token;
		}
	}
}