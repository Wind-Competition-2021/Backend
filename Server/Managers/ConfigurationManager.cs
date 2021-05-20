using System;
using System.Collections.Generic;
using System.Text;
using Server.Models;

namespace Server.Managers {
	/// <summary>
	///     Manager of user configurations
	/// </summary>
	public class ConfigurationManager {
		private const string CharSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		private static readonly Random Random = new();

		/// <summary>
		///     Default configuration
		/// </summary>
		public static Configuration DefaultConfiguration
			=> new() {
				PinnedStocks = new List<string>(),
				RefreshInterval = new RefreshInterval {
					List = TimeSpan.FromSeconds(5),
					Trend = TimeSpan.FromMinutes(1)
				}
			};

		/// <summary>
		///     Generate random token
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
		///     Configuration collection
		/// </summary>
		protected Dictionary<string, Configuration> Configurations { get; } = new();

		/// <summary>
		///     Get user configuration by token
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public Configuration this[string token] {
			get => Configurations[token];
			set => Configurations[token] = value;
		}

		/// <summary>
		///     Check whether token exists
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public bool Contains(string token) => Configurations.ContainsKey(token);

		/// <summary>
		///     Add a configuration
		/// </summary>
		/// <param name="token"></param>
		/// <param name="config">Default configuration will be used if ignored</param>
		public void Add(string token, Configuration config = null) => Configurations.Add(token, config ?? DefaultConfiguration);

		/// <summary>
		///     Generate a new random token that doesn't conflict with existing ones
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