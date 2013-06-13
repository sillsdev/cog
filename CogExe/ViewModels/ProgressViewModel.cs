using System;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace SIL.Cog.ViewModels
{
	public class ProgressViewModel : ViewModelBase
	{
		private readonly Action<ProgressViewModel> _action;
		private int _value;
		private string _text;
		private string _timeRemaining;
		private DateTime _firstTime;
		private bool _executing;
		private bool _canceled;
		private readonly ICommand _cancelCommand;

		public ProgressViewModel(Action<ProgressViewModel> action)
		{
			_action = action;
			_cancelCommand = new RelayCommand(() => Canceled = true);
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
				if (remaining.Ticks == 0)
					TimeRemaining = "";
				else if (remaining.TotalMinutes >= 1.5)
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

		public bool Executing
		{
			get { return _executing; }
			set { Set(() => Executing, ref _executing, value); }
		}

		public bool Canceled
		{
			get { return _canceled; }
			set { Set(() => Canceled, ref _canceled, value); }
		}

		public ICommand CancelCommand
		{
			get { return _cancelCommand; }
		}

		public void Execute()
		{
			Executing = true;
			Task.Factory.StartNew(() =>
				{
					_action(this);
					Executing = false;
				});
			_firstTime = DateTime.Now;
		}
	}
}
