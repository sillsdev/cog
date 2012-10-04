using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace SIL.Cog.ViewModels
{
	public class VarietyWordListsViewModel : VarietyViewModel
	{
		private readonly VarietySenseViewModelCollection<VarietySenseWordListsViewModel> _senses;
		private readonly ICommand _switchToVarietyCommand;
 
		public VarietyWordListsViewModel(CogProject project, Variety variety)
			: base(variety)
		{
			_senses = new VarietySenseViewModelCollection<VarietySenseWordListsViewModel>(project.Senses,
				ModelVariety.Words, sense => new VarietySenseWordListsViewModel(project, ModelVariety, sense, ModelVariety.Words[sense]));
			_switchToVarietyCommand = new RelayCommand(() => Messenger.Default.Send(new SwitchViewMessage(typeof(VarietiesViewModel), ModelVariety)));
		}

		public ObservableCollection<VarietySenseWordListsViewModel> Senses
		{
			get { return _senses; }
		}

		public ICommand SwitchToVarietyCommand
		{
			get { return _switchToVarietyCommand; }
		}
	}
}
