using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Services;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class VarietyVarietiesViewModel : VarietyViewModel
	{
		private readonly CogProject _project;
		private readonly IDialogService _dialogService;
		private readonly ObservableCollection<SegmentVarietyViewModel> _segments;
		private readonly VarietySenseViewModelCollection<VarietySenseViewModel> _senses;
		private readonly ViewModelCollection<AffixViewModel, Affix> _affixes;
		private AffixViewModel _currentAffix;
		private readonly ICommand _newAffixCommand;
		private readonly ICommand _removeAffixCommand;
 
		public VarietyVarietiesViewModel(IDialogService dialogService, CogProject project, Variety variety)
			: base(variety)
		{
			_project = project;
			_dialogService = dialogService;
			_segments = new ObservableCollection<SegmentVarietyViewModel>(variety.Segments.Select(seg => new SegmentVarietyViewModel(seg)));
			variety.Segments.CollectionChanged += SegmentsChanged;
			_senses = new VarietySenseViewModelCollection<VarietySenseViewModel>(_project.Senses, ModelVariety.Words, sense => new VarietySenseViewModel(sense, ModelVariety.Words[sense]));
			_affixes = new ViewModelCollection<AffixViewModel, Affix>(ModelVariety.Affixes, affix => new AffixViewModel(affix));
			if (_affixes.Count > 0)
				_currentAffix = _affixes[0];
			_newAffixCommand = new RelayCommand(ExecuteNewAffix);
			_removeAffixCommand = new RelayCommand(ExecuteRemoveAffix, CanExecuteRemoveAffix);
		}

		private void SegmentsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			DispatcherHelper.CheckBeginInvokeOnUI(() =>
				{
					switch (e.Action)
					{
						case NotifyCollectionChangedAction.Add:
							foreach (Segment segment in e.NewItems)
								_segments.Add(new SegmentVarietyViewModel(segment));
							break;

						case NotifyCollectionChangedAction.Remove:
							var removedSegments = new HashSet<Segment>(e.OldItems.Cast<Segment>());
							int numRemoved = 0;
							for (int i = _segments.Count - 1; i >= 0; i--)
							{
								if (removedSegments.Contains(_segments[i].ModelSegment))
								{
									_segments.RemoveAt(i);
									numRemoved++;
									if (numRemoved == removedSegments.Count)
										break;
								}
							}
							break;

						case NotifyCollectionChangedAction.Reset:
							_segments.Clear();
							break;
					}
				});
		}

		private void ExecuteNewAffix()
		{
			var vm = new NewAffixViewModel(_project);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				Shape shape;
				if (!_project.Segmenter.ToShape(vm.StrRep, out shape))
					shape = _project.Segmenter.EmptyShape;
				ModelVariety.Affixes.Add(new Affix(vm.StrRep, vm.Type == "Prefix" ? AffixType.Prefix : AffixType.Suffix, shape, vm.Category));
			}
		}

		private void ExecuteRemoveAffix()
		{
			ModelVariety.Affixes.Remove(CurrentAffix.ModelAffix);
		}

		private bool CanExecuteRemoveAffix()
		{
			return CurrentAffix != null;
		}

		public ObservableCollection<SegmentVarietyViewModel> Segments
		{
			get { return _segments; }
		}

		public ObservableCollection<VarietySenseViewModel> Senses
		{
			get { return _senses; }
		}

		public ObservableCollection<AffixViewModel> Affixes
		{
			get { return _affixes; }
		}

		public AffixViewModel CurrentAffix
		{
			get { return _currentAffix; }
			set { Set("CurrentAffix", ref _currentAffix, value); }
		}

		public ICommand NewAffixCommand
		{
			get { return _newAffixCommand; }
		}

		public ICommand RemoveAffixCommand
		{
			get { return _removeAffixCommand; }
		}
	}
}
