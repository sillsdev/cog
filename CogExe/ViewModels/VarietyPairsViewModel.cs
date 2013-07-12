using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Components;
using SIL.Cog.Services;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public enum CurrentVarietyPairState
	{
		SelectedAndCompared,
		SelectedAndNotCompared,
		NotSelected,
	}

	public class VarietyPairsViewModel : WorkspaceViewModelBase
	{
		private CogProject _project;
		private readonly IBusyService _busyService;
		private ReadOnlyMirroredList<Variety, VarietyViewModel> _varieties;
		private ListCollectionView _varietiesView1;
		private ListCollectionView _varietiesView2;
		private VarietyViewModel _currentVariety1;
		private VarietyViewModel _currentVariety2;
		private VarietyPairViewModel _currentVarietyPair;
		private CurrentVarietyPairState _currentVarietyPairState;
		private readonly IExportService _exportService;
		private readonly IDialogService _dialogService;

		private FindViewModel _findViewModel;
		private WordPairViewModel _startWordPair;
		private readonly SimpleMonitor _selectedWordPairsMonitor;

		public VarietyPairsViewModel(IBusyService busyService, IDialogService dialogService, IExportService exportService)
			: base("Variety Pairs")
		{
			_busyService = busyService;
			_dialogService = dialogService;
			_exportService = exportService;

			_selectedWordPairsMonitor = new SimpleMonitor();

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => SetCurrentVarietyPair());
			Messenger.Default.Register<ViewChangedMessage>(this, HandleViewChanged);
			Messenger.Default.Register<ModelChangingMessage>(this, HandleModelChanging);

			_currentVarietyPairState = CurrentVarietyPairState.NotSelected;
			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks", 
				new TaskAreaCommandViewModel("Perform comparison on this variety pair", new RelayCommand(PerformComparison)),
				new TaskAreaCommandViewModel("Find words", new RelayCommand(Find))));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export results for this variety pair", new RelayCommand(ExportVarietyPair))));
		}

		private void HandleModelChanging(ModelChangingMessage modelChangingMessage)
		{
			ResetCurrentVarietyPair();
			CurrentVarietyPair = null;
			CurrentVarietyPairState = CurrentVarietyPairState.SelectedAndNotCompared;
		}

		private void HandleViewChanged(ViewChangedMessage msg)
		{
			if (msg.OldViewModel == this && _findViewModel != null)
			{
				_dialogService.CloseDialog(_findViewModel);
				_findViewModel = null;
			}
		}

		private void PerformComparison()
		{
			if (_currentVarietyPairState == CurrentVarietyPairState.NotSelected || _currentVarietyPair != null)
				return;

			_busyService.ShowBusyIndicatorUntilUpdated();
			var pair = new VarietyPair(_currentVariety1.ModelVariety, _currentVariety2.ModelVariety);
			_project.VarietyPairs.Add(pair);

			var pipeline = new Pipeline<VarietyPair>(_project.GetComparisonProcessors());
			pipeline.Process(pair.ToEnumerable());

			CurrentVarietyPair = new VarietyPairViewModel(_project, pair, true);
			CurrentVarietyPairState = CurrentVarietyPairState.SelectedAndCompared;
		}

		private void Find()
		{
			if ( _findViewModel != null)
				return;

			_findViewModel = new FindViewModel(_dialogService, FindNext);
			_findViewModel.PropertyChanged += (sender, args) => _startWordPair = null;
			_dialogService.ShowModelessDialog(this, _findViewModel, () => _findViewModel = null);
		}

		private void FindNext()
		{
			if (_currentVarietyPair == null || (_currentVarietyPair.Cognates.WordPairs.Count == 0 && _currentVarietyPair.Noncognates.WordPairs.Count == 0))
			{
				SearchEnded();
				return;
			}
			WordPairsViewModel cognates = _currentVarietyPair.Cognates;
			WordPairsViewModel noncognates = _currentVarietyPair.Noncognates;
			if (cognates.SelectedWordPairs.Count == 0 && noncognates.SelectedWordPairs.Count == 0)
			{
				_startWordPair = noncognates.WordPairsView.Cast<WordPairViewModel>().Last();
			}
			else if (_startWordPair == null)
			{
				_startWordPair = cognates.SelectedWordPairs.Count > 0 ? cognates.SelectedWordPairs[0] : noncognates.SelectedWordPairs[0];
			}
			else if (cognates.SelectedWordPairs.Contains(_startWordPair) || noncognates.SelectedWordPairs.Contains(_startWordPair))
			{
				SearchEnded();
				return;
			}

			List<WordPairViewModel> wordPairs = cognates.WordPairsView.Cast<WordPairViewModel>().Concat(noncognates.WordPairsView.Cast<WordPairViewModel>()).ToList();
			WordPairViewModel curWordPair;
			if (cognates.SelectedWordPairs.Count > 0)
				curWordPair = cognates.SelectedWordPairs[0];
			else if (noncognates.SelectedWordPairs.Count > 0)
				curWordPair = noncognates.SelectedWordPairs[0];
			else
				curWordPair = _startWordPair;
			int wordPairIndex = wordPairs.IndexOf(curWordPair);
			do
			{
				wordPairIndex = (wordPairIndex + 1) % wordPairs.Count;
				curWordPair = wordPairs[wordPairIndex];
				bool match = false;
				switch (_findViewModel.Field)
				{
					case FindField.Word:
						match = curWordPair.ModelWordPair.Word1.StrRep.Contains(_findViewModel.String)
							|| curWordPair.ModelWordPair.Word2.StrRep.Contains(_findViewModel.String);
						break;

					case FindField.Sense:
						match = curWordPair.Sense.Gloss.Contains(_findViewModel.String);
						break;
				}
				if (match)
				{
					using (_selectedWordPairsMonitor.Enter())
					{
						cognates.SelectedWordPairs.Clear();
						noncognates.SelectedWordPairs.Clear();
						WordPairsViewModel vm = curWordPair.AreCognate ? cognates : noncognates;
						vm.SelectedWordPairs.Add(curWordPair);
					}
					return;
				}
			}
			while (_startWordPair != curWordPair);
			SearchEnded();
		}

		private void SearchEnded()
		{
			_findViewModel.ShowSearchEndedMessage();
			_startWordPair = null;
		}

		private void ExportVarietyPair()
		{
			if (_currentVarietyPairState != CurrentVarietyPairState.SelectedAndCompared)
				return;

			_exportService.ExportVarietyPair(this, _project, _currentVarietyPair.ModelVarietyPair);
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			Set("Varieties", ref _varieties, new ReadOnlyMirroredList<Variety, VarietyViewModel>(_project.Varieties, variety => new VarietyViewModel(variety), vm => vm.ModelVariety));
			Set("VarietiesView1", ref _varietiesView1, new ListCollectionView(_varieties) {SortDescriptions = {new SortDescription("Name", ListSortDirection.Ascending)}});
			Set("VarietiesView2", ref _varietiesView2, new ListCollectionView(_varieties) {SortDescriptions = {new SortDescription("Name", ListSortDirection.Ascending)}});
			ResetCurrentVarietyPair();
		}

		private void ResetCurrentVarietyPair()
		{
			if (_varieties.Count > 0)
			{
				Set(() => CurrentVariety1, ref _currentVariety1, (VarietyViewModel) _varietiesView1.GetItemAt(0));
				if (_varieties.Count > 1)
					Set(() => CurrentVariety2, ref _currentVariety2, (VarietyViewModel) _varietiesView2.GetItemAt(1));
				else
					Set(() => CurrentVariety2, ref _currentVariety2, (VarietyViewModel) _varietiesView2.GetItemAt(0));
				SetCurrentVarietyPair();
			}
			else
			{
				Set(() => CurrentVariety1, ref _currentVariety1, null);
				Set(() => CurrentVariety2, ref _currentVariety2, null);
				SetCurrentVarietyPair();
			}
		}

		public ReadOnlyObservableList<VarietyViewModel> Varieties
		{
			get { return _varieties; }
		}

		public ICollectionView VarietiesView1
		{
			get { return _varietiesView1; }
		}

		public VarietyViewModel CurrentVariety1
		{
			get { return _currentVariety1; }
			set
			{
				if (Set(() => CurrentVariety1, ref _currentVariety1, value))
					SetCurrentVarietyPair();
			}
		}

		public ICollectionView VarietiesView2
		{
			get { return _varietiesView2; }
		}

		public VarietyViewModel CurrentVariety2
		{
			get { return _currentVariety2; }
			set
			{
				if (Set(() => CurrentVariety2, ref _currentVariety2, value))
					SetCurrentVarietyPair();
			}
		}

		public VarietyPairViewModel CurrentVarietyPair
		{
			get { return _currentVarietyPair; }
			set
			{
				VarietyPairViewModel oldCurVarietyPair = _currentVarietyPair;
				if (Set(() => CurrentVarietyPair, ref _currentVarietyPair, value))
				{
					_startWordPair = null;
					if (oldCurVarietyPair != null)
					{
						oldCurVarietyPair.Cognates.SelectedWordPairs.CollectionChanged -= SelectedWordPairsChanged;
						oldCurVarietyPair.Noncognates.SelectedWordPairs.CollectionChanged -= SelectedWordPairsChanged;
					}

					if (_currentVarietyPair != null)
					{
						_currentVarietyPair.Cognates.SelectedWordPairs.CollectionChanged += SelectedWordPairsChanged;
						_currentVarietyPair.Noncognates.SelectedWordPairs.CollectionChanged += SelectedWordPairsChanged;
					}
				}
			}
		}

		private void SelectedWordPairsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!_selectedWordPairsMonitor.Busy)
				_startWordPair = null;
		}

		public CurrentVarietyPairState CurrentVarietyPairState
		{
			get { return _currentVarietyPairState; }
			set { Set(() => CurrentVarietyPairState, ref _currentVarietyPairState, value); }
		}

		private void SetCurrentVarietyPair()
		{
			if (_currentVariety1 != null && _currentVariety2 != null && _currentVariety1 != _currentVariety2)
			{
				VarietyPair pair;
				if (_currentVariety1.ModelVariety.VarietyPairs.TryGetValue(_currentVariety2.ModelVariety, out pair))
				{
					_busyService.ShowBusyIndicatorUntilUpdated();
					CurrentVarietyPair = new VarietyPairViewModel(_project, pair, _currentVariety1.ModelVariety == pair.Variety1);
					CurrentVarietyPairState = CurrentVarietyPairState.SelectedAndCompared;
				}
				else
				{
					CurrentVarietyPair = null;
					CurrentVarietyPairState = CurrentVarietyPairState.SelectedAndNotCompared;
				}
			}
			else
			{
				CurrentVarietyPair = null;
				CurrentVarietyPairState = CurrentVarietyPairState.NotSelected;
			}
		}

		public override bool SwitchView(Type viewType, IReadOnlyList<object> models)
		{
			if (viewType == typeof(VarietyPairsViewModel))
			{
				_busyService.ShowBusyIndicatorUntilUpdated();

				var pair = (VarietyPair) models[0];
				CurrentVarietyPair = new VarietyPairViewModel(_project, pair, true);
				Set(() => CurrentVariety1, ref _currentVariety1, _varieties[pair.Variety1]);
				Set(() => CurrentVariety2, ref _currentVariety2, _varieties[pair.Variety2]);
				CurrentVarietyPairState = pair.Variety1.VarietyPairs.Contains(pair.Variety2)
					? CurrentVarietyPairState.SelectedAndCompared : CurrentVarietyPairState.SelectedAndNotCompared;
				return true;
			}

			return false;
		}
	}
}
