using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Shared {
	public struct ExpirationDate {
		public enum Type : byte {
			DateTime,
			FixedTimeSpan,
			SlidingTimeSpan
		}

		private DateTime _value;

		public ExpirationDate(DateTime date) {
			ExpirationType = Type.DateTime;
			_value = date;
		}

		public ExpirationDate(TimeSpan duration, bool sliding = true) {
			_value = (sliding ? DateTime.MinValue : DateTime.Now) + duration;
			ExpirationType = sliding ? Type.SlidingTimeSpan : Type.FixedTimeSpan;
		}

		public Type ExpirationType { get; private set; }

		public DateTime? AbsoluteExpiration {
			get => ExpirationType == Type.SlidingTimeSpan ? null : _value;
			set {
				if (value.HasValue) {
					ExpirationType = Type.DateTime;
					_value = value.Value;
				}
			}
		}

		public TimeSpan? SlidingExpiration {
			get => ExpirationType == Type.SlidingTimeSpan ? _value - DateTime.MinValue : null;
			set {
				if (value.HasValue) {
					ExpirationType = Type.SlidingTimeSpan;
					_value = DateTime.MinValue + value.Value;
				}
			}
		}
	}

	public static class RedisExtension {
		/// <inheritdoc cref="IDatabase.KeyExpire(RedisKey,TimeSpan?,CommandFlags)"/>
		public static bool KeyExpire(this IDatabase redis, RedisKey key, ExpirationDate? expiry, CommandFlags flag = CommandFlags.None)
			=> !expiry.HasValue
				? redis.KeyPersist(key, flag)
				: expiry.Value.AbsoluteExpiration != null
					? redis.KeyExpire(key, expiry.Value.AbsoluteExpiration, flag)
					: redis.KeyExpire(key, expiry.Value.SlidingExpiration, flag);

		/// <inheritdoc cref="IDatabaseAsync.KeyExpireAsync(RedisKey,TimeSpan?,CommandFlags)"/>
		public static Task<bool> KeyExpireAsync(this IDatabase redis, RedisKey key, ExpirationDate? expiry, CommandFlags flag = CommandFlags.None)
			=> !expiry.HasValue
				? redis.KeyPersistAsync(key, flag)
				: expiry.Value.AbsoluteExpiration != null
					? redis.KeyExpireAsync(key, expiry.Value.AbsoluteExpiration, flag)
					: redis.KeyExpireAsync(key, expiry.Value.SlidingExpiration, flag);

		public static T ObjectGet<T>(this IDatabase redis, RedisKey key, CommandFlags flags = CommandFlags.None) => JsonConvert.DeserializeObject<T>(redis.StringGet(key, flags));

		public static Task<T> ObjectGetAsync<T>(this IDatabase redis, RedisKey key, CommandFlags flags = CommandFlags.None) => redis.StringGetAsync(key, flags).ContinueWith(task => JsonConvert.DeserializeObject<T>(task.Result));

		public static bool ObjectSet<T>(this IDatabase redis, RedisKey key, T value, When when = When.Always, CommandFlags flags = CommandFlags.None) => redis.StringSet(key, JsonConvert.SerializeObject(value), null, when, flags);

		public static Task<bool> ObjectSetAsync<T>(this IDatabase redis, RedisKey key, T value, When when = When.Always, CommandFlags flags = CommandFlags.None) => redis.StringSetAsync(key, JsonConvert.SerializeObject(value), null, when, flags);
	}
}