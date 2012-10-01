using GalaSoft.MvvmLight.Messaging;

namespace SIL.Cog.ViewModels
{
	public class DataMasterViewModel : MasterViewModel
	{
		private readonly WordListsViewModel _wordListsViewModel;
		private readonly VarietiesViewModel _varietiesViewModel;
		private readonly SensesViewModel _sensesViewModel;
		private readonly DataSettingsViewModel _dataSettingsViewModel;

		public DataMasterViewModel(WordListsViewModel wordListsViewModel, VarietiesViewModel varietiesViewModel, SensesViewModel sensesViewModel,
			DataSettingsViewModel dataSettingsViewModel)
			: base("Data", wordListsViewModel, varietiesViewModel, sensesViewModel, dataSettingsViewModel)
		{
			_wordListsViewModel = wordListsViewModel;
			_varietiesViewModel = varietiesViewModel;
			_sensesViewModel = sensesViewModel;
			_dataSettingsViewModel = dataSettingsViewModel;

			CurrentView = _wordListsViewModel;
			Messenger.Default.Register<SwitchViewMessage>(this, SwitchView);
		}

		public override void Initialize(CogProject project)
		{
			_wordListsViewModel.Initialize(project);
			_varietiesViewModel.Initialize(project);
			_sensesViewModel.Initialize(project);
			_dataSettingsViewModel.Initialize(project);
		}

		private void SwitchView(SwitchViewMessage message)
		{
			if (message.ViewModelType == typeof(VarietiesViewModel))
			{
				_varietiesViewModel.SwitchVariety((Variety) message.Model);
				CurrentView = _varietiesViewModel;
			}
		}
	}
}
