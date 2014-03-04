using System;
using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Application.Services;

namespace SIL.Cog.Application.ViewModels
{
	public enum FindField
	{
		[Description("Gloss")]
		Gloss,
		[Description("Form")]
		Form
	}

	public class FindViewModel : ViewModelBase
	{
		private string _string;
		private FindField _field;
		private readonly ICommand _findNextCommand;
		private readonly IDialogService _dialogService;

		public FindViewModel(IDialogService dialogService, Action find)
		{
			_dialogService = dialogService;
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

		internal void ShowSearchEndedMessage()
		{
			_dialogService.ShowMessage(this, "Find reached the starting point of the search.", "Cog");
		}

		public ICommand FindNextCommand
		{
			get { return _findNextCommand; }
		}
	}
}
