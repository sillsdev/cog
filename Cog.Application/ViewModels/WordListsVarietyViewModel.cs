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
		public delegate WordListsVarietyViewModel Factory(WordListsViewModel parent, Variety variety);

		private readonly VarietyMeaningViewModelCollection _meanings;
		private readonly ICommand _switchToVarietyCommand;
		private bool _isValid;
		private readonly ICommand _goToNextInvalidWordCommand;
		private readonly WordListsViewModel _parent;
 
		public WordListsVarietyViewModel(IProjectService projectService, WordListsVarietyMeaningViewModel.Factory varietyMeaningFactory, WordListsViewModel parent, Variety variety)
			: base(variety)
		{
			_parent = parent;
			_meanings = new VarietyMeaningViewModelCollection(projectService.Project.Meanings, DomainVariety.Words, meaning => varietyMeaningFactory(this, meaning));
			_switchToVarietyCommand = new RelayCommand(() => Messenger.Default.Send(new SwitchViewMessage(typeof(VarietiesViewModel), DomainVariety)));
			_goToNextInvalidWordCommand = new RelayCommand(GotoNextInvalidWord);
			CheckForErrors();
		}

		private void GotoNextInvalidWord()
		{
			int startingIndex = _parent.SelectedVarietyMeaning != null && _parent.SelectedVarietyMeaning.Variety == this
				? (_meanings.IndexOf(_parent.SelectedVarietyMeaning) + 1) % _meanings.Count : _meanings.Count - 1;
			for (int i = 0; i < _meanings.Count; i++)
			{
				int index = (startingIndex + i) % _meanings.Count;
				if (_meanings[index].Words.Any(w => !w.IsValid))
				{
					_parent.SelectedVarietyMeaning = _meanings[index];
					break;
				}
			}
		}

		public ReadOnlyObservableList<WordListsVarietyMeaningViewModel> Meanings
		{
			get { return _meanings; }
		}

		public ICommand SwitchToVarietyCommand
		{
			get { return _switchToVarietyCommand; }
		}

		public ICommand GoToNextInvalidWordCommand
		{
			get { return _goToNextInvalidWordCommand; }
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
