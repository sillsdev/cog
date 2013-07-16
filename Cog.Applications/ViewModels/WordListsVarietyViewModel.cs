using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class WordListsVarietyViewModel : VarietyViewModel
	{
		private readonly VarietySenseViewModelCollection _senses;
		private readonly ICommand _switchToVarietyCommand;
 
		public WordListsVarietyViewModel(IBusyService busyService, CogProject project, Variety variety)
			: base(variety)
		{
			_senses = new VarietySenseViewModelCollection(project.Senses, DomainVariety.Words,
				sense => new VarietySenseViewModel(busyService, project, this, sense, DomainVariety.Words[sense]));
			_switchToVarietyCommand = new RelayCommand(() => Messenger.Default.Send(new SwitchViewMessage(typeof(VarietiesViewModel), DomainVariety)));
		}

		public ReadOnlyObservableList<VarietySenseViewModel> Senses
		{
			get { return _senses; }
		}

		public ICommand SwitchToVarietyCommand
		{
			get { return _switchToVarietyCommand; }
		}
	}
}
