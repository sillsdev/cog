using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class WordListsVarietyViewModel : VarietyViewModel
	{
		private readonly VarietySenseViewModelCollection _senses;
		private readonly ICommand _switchToVarietyCommand;
 
		public WordListsVarietyViewModel(CogProject project, Variety variety)
			: base(variety)
		{
			_senses = new VarietySenseViewModelCollection(project.Senses,
				ModelVariety.Words, sense =>
					{
						var vm = new VarietySenseViewModel(project, ModelVariety, sense, ModelVariety.Words[sense]);
						vm.PropertyChanged += ChildPropertyChanged;
						return vm;
					});
			_switchToVarietyCommand = new RelayCommand(() => Messenger.Default.Send(new SwitchViewMessage(typeof(VarietiesViewModel), ModelVariety)));
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			ChildrenAcceptChanges(_senses);
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
