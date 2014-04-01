using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Application.Collections;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Application.ViewModels
{
	public class WordListsVarietyViewModel : VarietyViewModel
	{
		public delegate WordListsVarietyViewModel Factory(Variety variety);

		private readonly VarietyMeaningViewModelCollection _meanings;
		private readonly ICommand _switchToVarietyCommand;
		private bool _isValid;
 
		public WordListsVarietyViewModel(IProjectService projectService, WordListsVarietyMeaningViewModel.Factory varietyMeaningFactory, Variety variety)
			: base(variety)
		{
			_meanings = new VarietyMeaningViewModelCollection(projectService.Project.Meanings, DomainVariety.Words, meaning => varietyMeaningFactory(this, meaning));
			_switchToVarietyCommand = new RelayCommand(() => Messenger.Default.Send(new SwitchViewMessage(typeof(VarietiesViewModel), DomainVariety)));
			CheckForErrors();
		}

		public ReadOnlyObservableList<WordListsVarietyMeaningViewModel> Meanings
		{
			get { return _meanings; }
		}

		public ICommand SwitchToVarietyCommand
		{
			get { return _switchToVarietyCommand; }
		}

		public bool IsValid
		{
			get { return _isValid; }
			private set { Set(() => IsValid, ref _isValid, value); }
		}

		internal void CheckForErrors()
		{
			IsValid = _meanings.SelectMany(m => m.Words).All(w => w.IsValid);
		}
	}
}
