using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace SIL.Cog.ViewModels
{
	public class VarietyWordListsViewModel : ViewModelBase
	{
		private readonly Variety _variety;
		private readonly VarietySenseViewModelCollection<VarietySenseWordListsViewModel> _senses;
		private readonly ICommand _switchToVarietyCommand;
 
		public VarietyWordListsViewModel(CogProject project, Variety variety)
		{
			_variety = variety;
			_senses = new VarietySenseViewModelCollection<VarietySenseWordListsViewModel>(project.Senses, _variety.Words, sense => new VarietySenseWordListsViewModel(project, _variety, sense, _variety.Words[sense]));
			_switchToVarietyCommand = new RelayCommand(() => Messenger.Default.Send(new SwitchViewMessage(typeof(VarietiesViewModel), _variety)));
		}

		public Variety ModelVariety
		{
			get { return _variety; }
		}

		public string Name
		{
			get { return _variety.Name; }
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
