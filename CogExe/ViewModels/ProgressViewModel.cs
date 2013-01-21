using System;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class ProgressViewModel : ViewModelBase
	{
		private readonly Action _action;
		private int _value;
		private string _text;
		private string _timeRemaining;
		private DateTime _firstTime;

		public ProgressViewModel(Action action)
		{
			_action = action;
		}

		public string Text
		{
			get { return _text; }
			set { Set(() => Text, ref _text, value); }
		}

		public int Value
		{
			get { return _value; }
			set
			{
				Set(() => Value, ref _value, value);
				DateTime now = DateTime.Now;
				TimeSpan span = now - _firstTime;
				var remaining = new TimeSpan((span.Ticks / value) * (100 - value));
				if (remaining.TotalMinutes >= 1.5)
					TimeRemaining = string.Format("About {0} minutes remaining", (int) Math.Round(remaining.TotalMinutes, MidpointRounding.AwayFromZero));
				else
					TimeRemaining = string.Format("About {0} seconds remaining", (int) Math.Round(remaining.TotalSeconds, MidpointRounding.AwayFromZero));
			}
		}

		public string TimeRemaining
		{
			get { return _timeRemaining; }
			set { Set(() => TimeRemaining, ref _timeRemaining, value); }
		}

		public void Execute()
		{
			_action();
			_firstTime = DateTime.Now;
		}
	}
}
