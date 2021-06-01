using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Server.Models {
	/// <summary>
	/// </summary>
	public class StockId : IEquatable<StockId> {
		private static readonly Regex Pattern = new(@"^(?:(?<exchange>[a-z]{2})\.(?<code>\d{6}))|(?:(?<code>\d{6})\.(?<exchange>[a-z]{2}))$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		public StockId(string id) {
			var result = Pattern.Match(id);
			if (!result.Success)
				throw new FormatException($"Parameter {nameof(id)} doesn't match required pattern");
			Exchange = result.Groups["exchange"].Value.ToLower();
			Code = result.Groups["code"].Value;
		}

		/// <summary>
		/// </summary>
		/// <param name="exchange"></param>
		/// <param name="code"></param>
		public StockId(
			[RegularExpression(@"^[a-z]{2}$")] string exchange,
			[RegularExpression(@"^\d{6}$")] string code
		) {
			Exchange = exchange;
			Code = code;
		}

		/// <summary>
		/// </summary>
		public string Exchange { get; }

		/// <summary>
		/// </summary>
		public string Code { get; }

		/// <summary>
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(StockId other) => Code == other?.Code && Exchange == other?.Exchange;

		/// <inheritdoc />
		public override bool Equals(object obj) {
			if (obj is null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((StockId)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode() => HashCode.Combine(Exchange, Code);

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		public static implicit operator string(StockId id) => id.ToString();

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		public static implicit operator StockId(string id) => new(id);

		/// <summary>
		/// </summary>
		/// <param name="one"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static bool operator ==(StockId one, StockId other) => one != null && one.Equals(other);

		/// <summary>
		/// </summary>
		/// <param name="one"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static bool operator !=(StockId one, StockId other) => !(one == other);

		/// <summary>
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		public string ToString(string format = "prefix") => format == "suffix" ? $"{Code}.{Exchange}" : $"{Exchange}.{Code}";
	}

	/// <inheritdoc />
	public class StockIdConverter : JsonConverter<StockId> {
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, StockId value, JsonSerializer serializer) => writer.WriteValue(value.ToString());

		/// <inheritdoc />
		public override StockId ReadJson(JsonReader reader, Type objectType, StockId existingValue, bool hasExistingValue, JsonSerializer serializer) => new(reader.Value as string);
	}
}