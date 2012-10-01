using System;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class ProgressViewModel : ViewModelBase
	{
		private readonly string _displayName;
		private readonly Action<ProgressViewModel> _action;
		private bool _isCompleted;
		private int _value;

		public ProgressViewModel(string displayName, Action<ProgressViewModel> action)
		{
			_displayName = displayName;
			_action = action;
		}

		public string DisplayName
		{
			get { return _displayName; }
		}

		public int Value
		{
			get { return _value; }
			set { Set("Value", ref _value, value); }
		}

		public bool IsCompleted
		{
			get { return _isCompleted; }
			set { Set("IsCompleted", ref _isCompleted, value); }
		}

		public void Execute()
		{
			_action(this);
		}
	}
}
