using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Collections;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class WordListsVarietyViewModel : VarietyViewModel
	{
		public delegate WordListsVarietyViewModel Factory(Variety variety);

		private readonly VarietySenseViewModelCollection _senses;
		private readonly ICommand _switchToVarietyCommand;
 
		public WordListsVarietyViewModel(IProjectService projectService, WordListsVarietySenseViewModel.Factory varietySenseFactory, Variety variety)
			: base(variety)
		{
			_senses = new VarietySenseViewModelCollection(projectService.Project.Senses, DomainVariety.Words, sense => varietySenseFactory(this, sense));
			_switchToVarietyCommand = new RelayCommand(() => Messenger.Default.Send(new SwitchViewMessage(typeof(VarietiesViewModel), DomainVariety)));
		}

		public ReadOnlyObservableList<WordListsVarietySenseViewModel> Senses
		{
			get { return _senses; }
		}

		public ICommand SwitchToVarietyCommand
		{
			get { return _switchToVarietyCommand; }
		}
	}
}
