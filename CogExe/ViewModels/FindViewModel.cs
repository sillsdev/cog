using System;
using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace SIL.Cog.ViewModels
{
	public enum FindField
	{
		[Description("Word")]
		Word,
		[Description("Sense")]
		Sense
	}

	public class FindViewModel : ViewModelBase
	{
		private string _string;
		private FindField _field;
		private readonly ICommand _findNextCommand;

		public FindViewModel(Action find)
		{
			_findNextCommand = new RelayCommand(find, () => !string.IsNullOrEmpty(_string));
		}

		public string String
		{
			get { return _string; }
			set { Set(() => String, ref _string, value); }
		}

		public FindField Field
		{
			get { return _field; }
			set { Set(() => Field, ref _field, value); }
		}

		public ICommand FindNextCommand
		{
			get { return _findNextCommand; }
		}
	}
}
