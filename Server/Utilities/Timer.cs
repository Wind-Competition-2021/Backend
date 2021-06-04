using System;
using System.Timers;

namespace Server.Utilities {
	/// <summary>
	/// Handles recurring events in an application with dynamic interval.
	/// </summary>
	public class Timer : System.Timers.Timer {
		/// <summary>
		/// 
		/// </summary>
		protected ElapsedEventHandler IntervalElapsed = delegate { };

		private readonly Func<int, TimeSpan> _getInterval = null;
		private int _count = 0;

		/// <inheritdoc/>
		public Timer() : base() => AutoReset = false;

		/// <inheritdoc/>
		public Timer(double interval) : base(interval) => AutoReset = false;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="interval"></param>
		public Timer(TimeSpan interval) : this(interval.TotalMilliseconds) { }

		/// <param name="delay">The time, in milliseconds, to delay before the first Elapsed event fires</param>
		/// <param name="interval"></param>
		public Timer(TimeSpan delay, TimeSpan interval) : this(interval) {
			Immediate = delay.Ticks == 0;
			if (!Immediate) {
				Interval = delay;
				_getInterval = _ => interval;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="getInterval"></param>
		public Timer(Func<int, TimeSpan> getInterval) : base() {
			_getInterval = getInterval;
			AutoReset = false;
		}

		/// <summary>
		/// 
		/// </summary>
		public bool Immediate { get; init; } = false;

		/// <summary>
		/// 
		/// </summary>
		public new TimeSpan Interval {
			get => TimeSpan.FromMilliseconds(base.Interval);
			set => base.Interval = value.TotalMilliseconds;
		}

		/// <inheritdoc cref="System.Timers.Timer.Enabled"/>
		public new bool Enabled {
			get => base.Enabled;
			set {
				if (!base.Enabled && value && Immediate && _count == 0)
					IntervalElapsed(this, null);
				base.Enabled = value;
			}
		}

		/// <inheritdoc cref="System.Timers.Timer.Elapsed"/>
		public new event ElapsedEventHandler Elapsed {
			add {
				var handler = ModifyEventHandler(value);
				IntervalElapsed += handler;
				base.Elapsed += handler;
			}
			remove {
				var handler = ModifyEventHandler(value);
				IntervalElapsed -= handler;
				base.Elapsed -= handler;
			}
		}

		/// <inheritdoc cref="System.Timers.Timer.Start"/>
		public new void Start() => Enabled = true;

		/// <inheritdoc cref="System.Timers.Timer.Stop"/>
		public new void Stop() => Enabled = false;

		/// <inheritdoc cref="System.Timers.Timer.Close"/>
		public new void Close() {
			_count = 0;
			base.Close();
		}

		private ElapsedEventHandler ModifyEventHandler(ElapsedEventHandler handler)
			=> (sender, e) => {
				++_count;
				if (_getInterval != null)
					Interval = _getInterval(_count);
				Enabled = true;
				handler.Invoke(sender, e);
			};
	}
}