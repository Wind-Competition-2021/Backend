using System;
using System.Collections.Generic;
using System.Linq;
using BaoStock;
using Server.Models;
using Server.Utilities;
using Shared;
using Api = BaoStock.BaoStockManager.Api;

namespace Server.Managers {
	/// <summary>
	///     Manager of playback time stock quotes
	/// </summary>
	public class PlaybackQuotesManager {
		/// <summary>
		/// </summary>
		/// <param name="baoStock"></param>
		public PlaybackQuotesManager(BaoStockManager baoStock) => BaoStock = baoStock;

		/// <summary>
		/// </summary>
		public BaoStockManager BaoStock { get; }

		/// <summary>
		/// </summary>
		public Dictionary<string, Simulation> Simulations { get; } = new();

		/// <summary>
		/// </summary>
		public Dictionary<string, Dictionary<StockId, int>> TrendsLastIndices { get; } = new();

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public Simulation this[string token] => Simulations[token];

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public bool Contains(string token) => Simulations.ContainsKey(token);

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public bool Remove(string token) {
			TrendsLastIndices.Remove(token);
			return Simulations.Remove(token);
		}

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <param name="ids"></param>
		/// <param name="begin"></param>
		/// <param name="end"></param>
		/// <param name="timer"></param>
		public void Initialize(string token, string[] ids, DateTime begin, DateTime end, Timer timer = null) {
			var sources = new Source[ids.Length];
			var preClosing = new Dictionary<DateTime, int?>();
			for (int i = 0; i < ids.Length; ++i) {
				var daily = BaoStock.Fetch<DailyPrice[]>(Api.DailyPrice, ids[i], begin, end);
				foreach (var price in daily)
					preClosing[price.Date!.Value.Date] = price.PreClosing;
				sources[i] = new Source(
					-1,
					BaoStock.Fetch<MinutelyPrice[]>(Api.MinutelyPrice, ids[i], begin, end, 5)
						.Select(
							price => {
								RealtimePrice result = new(price) {
									Time = price.Time,
									PreClosing = preClosing[price.Time!.Value.Date]
								};
								return result;
							}
						)
						.ToArray()
				);
			}
			var simulation = new Simulation(sources, begin, end, preClosing.Keys.ToArray());
			if (timer is not null)
				simulation.Timer = timer;
			Simulations[token] = simulation;
			TrendsLastIndices[token] = new Dictionary<StockId, int>();
			simulation.Start(false);
		}

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public List<RealtimePrice> GetList(string token) => Simulations[token].Sources.Select(source => source.Current).Where(price => price is not null).ToList();

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public List<RealtimePrice> GetTrend(string token, StockId id) {
			var source = Simulations[token].Sources.FirstOrDefault(src => src.Quotes.FirstOrDefault()?.Id == id);
			if (source is null)
				throw new KeyNotFoundException($"Stock {id:b} doesn't exist in current playback list");
			var trendIndices = TrendsLastIndices[token];
			var result = trendIndices.ContainsKey(id)
				? source.Quotes.Skip(trendIndices[id]).Take(source.Index - trendIndices[id])
				: source.Quotes.Take(source.Index);
			trendIndices[id] = source.Index;
			return result.ToList();
		}

		/// <summary>
		/// </summary>
		public record Source(int Index, RealtimePrice[] Quotes) {
			/// <summary>
			/// </summary>
			public int Index { get; set; } = Index;

			/// <summary>
			/// </summary>
			public RealtimePrice Current => Index < 0 ? null : Quotes[Index];

			/// <summary>
			/// </summary>
			public RealtimePrice Next => Index < Length ? Quotes[Index + 1] : null;

			/// <summary>
			/// </summary>
			public int Length => Quotes.Length;
		}

		/// <summary>
		/// </summary>
		public class Simulation {
			#region Events
			/// <summary>
			/// </summary>
			public event EventHandler SimulationFinished = delegate { };
			#endregion

			#region Fields
			/// <summary>
			/// </summary>
			public static readonly TimeSpan TradeBeginTime = TimeSpan.FromHours(9.5);

			/// <summary>
			/// </summary>
			public static readonly TimeSpan TradeEndTime = TimeSpan.FromHours(15);

			private int _tradeDayIndex;

			private bool _finished;

			private bool _stopped;

			private Timer _timer;
			#endregion

			#region Constructors
			/// <summary>
			/// </summary>
			public Simulation() { }

			/// <summary>
			/// </summary>
			/// <param name="sources"></param>
			/// <param name="begin"></param>
			/// <param name="end"></param>
			/// <param name="tradeDays"></param>
			public Simulation(IEnumerable<Source> sources, DateTime begin, DateTime end, DateTime[] tradeDays) {
				Sources = sources;
				TradeDays = tradeDays;
				if (tradeDays?.Length > 0) {
					BeginTime = begin.Date == TradeDays[0] && begin.TimeOfDay >= TradeBeginTime ? begin : TradeDays[0] + TradeBeginTime;
					if (end.TimeOfDay == TimeSpan.Zero)
						end = end.AddDays(1);
					EndTime = end.Date == TradeDays[^1] && end.TimeOfDay <= TradeEndTime ? end : TradeDays[^1] + TradeEndTime;
				}
			}
			#endregion

			#region Properties
			/// <summary>
			/// </summary>
			public bool Started { get; private set; }

			/// <summary>
			/// </summary>
			public bool Stopped {
				get => _stopped;
				set {
					if (_stopped == value)
						return;
					_stopped = value;
					if (value)
						Timer?.Stop();
					else
						Timer?.Start();
				}
			}

			/// <summary>
			/// </summary>
			public bool Finished {
				get => _finished;
				set {
					if (_finished == value)
						return;
					_finished = value;
					if (value)
						OnSimulationFinished(new EventArgs());
				}
			}

			/// <summary>
			/// </summary>
			public Timer Timer {
				get => _timer;
				set {
					_timer = value;
					_timer.Elapsed += (_, _) => Tick();
				}
			}

			/// <summary>
			/// </summary>
			public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(5);

			/// <summary>
			/// </summary>
			public IEnumerable<Source> Sources { get; init; }

			/// <summary>
			/// </summary>
			public DateTime Now { get; private set; }

			private DateTime BeginTime { get; }

			private DateTime EndTime { get; }

			private DateTime[] TradeDays { get; }
			#endregion

			#region Methods
			/// <summary>
			///     Start the simulation
			/// </summary>
			public void Start(bool startTimer = true) {
				if (!Started) {
					_tradeDayIndex = 0;
					Started = true;
					Finished = false;
					if (TradeDays.Length == 0)
						Finished = true;
					else
						Now = BeginTime;
					foreach (var source in Sources)
						source.Index = -1;
					Tick(TimeSpan.Zero);
				}
				Stopped = false;
				if (startTimer)
					Timer?.Start();
			}

			/// <summary>
			/// </summary>
			public void Stop() {
				Stopped = true;
				Timer?.Stop();
			}

			/// <summary>
			/// </summary>
			public void Close() => Timer?.Close();

			/// <summary>
			/// </summary>
			/// <param name="duration"></param>
			public bool Tick(TimeSpan duration) {
				if (Finished)
					return false;
				Now += duration;
				if (Now >= EndTime || _tradeDayIndex == TradeDays.Length - 1 && Now.TimeOfDay >= TradeEndTime)
					Finished = true;
				else if (Now.TimeOfDay >= TradeEndTime) {
					Now += TradeDays[_tradeDayIndex + 1] - TradeDays[_tradeDayIndex] - (TradeEndTime - TradeBeginTime);
					++_tradeDayIndex;
				}
				foreach (var source in Sources) {
					if (source.Index == source.Length - 1)
						continue;
					for (; source.Index + 1 < source.Length && source.Next.Time <= Now; ++source.Index) { }
				}
				return true;
			}

			/// <summary>
			/// </summary>
			public bool Tick() => Tick(Duration);

			/// <summary>
			/// </summary>
			/// <param name="e"></param>
			protected void OnSimulationFinished(EventArgs e) {
				SimulationFinished(this, e);
				Timer?.Close();
			}
			#endregion
		}
	}
}