using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public enum CurrentVarietyPairState
	{
		SelectedAndCompared,
		SelectedAndNotCompared,
		NotSelected,
	}

	public class VarietyPairsViewModel : WorkspaceViewModelBase
	{
		private readonly IBusyService _busyService;
		private readonly IProjectService _projectService;
		private readonly IAnalysisService _analysisService;
		private readonly VarietyPairViewModel.Factory _varietyPairFactory;
		private ReadOnlyMirroredList<Variety, VarietyViewModel> _varieties;
		private ICollectionView _varietiesView1;
		private ICollectionView _varietiesView2;
		private VarietyViewModel _currentVariety1;
		private VarietyViewModel _currentVariety2;
		private VarietyPairViewModel _currentVarietyPair;
		private CurrentVarietyPairState _currentVarietyPairState;
		private readonly IExportService _exportService;
		private readonly IDialogService _dialogService;
		private readonly ICommand _findCommand;

		private string _sortPropertyName;
		private ListSortDirection _sortDirection;

		private FindViewModel _findViewModel;
		private WordPairsViewModel _startWordPairs;
		private readonly SimpleMonitor _selectedWordPairsMonitor;
		private bool _deferredResetCurrentVarietyPair;
		private bool _searchWrapped;

		public VarietyPairsViewModel(IProjectService projectService, IBusyService busyService, IDialogService dialogService, IExportService exportService, IAnalysisService analysisService,
			VarietyPairViewModel.Factory varietyPairFactory)
			: base("Variety Pairs")
		{
			_projectService = projectService;
			_busyService = busyService;
			_dialogService = dialogService;
			_exportService = exportService;
			_analysisService = analysisService;
			_varietyPairFactory = varietyPairFactory;

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			_sortPropertyName = "PhoneticSimilarityScore";
			_sortDirection = ListSortDirection.Descending;

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => SetCurrentVarietyPair());
			Messenger.Default.Register<DomainModelChangedMessage>(this, msg =>
				{
					if (msg.AffectsComparison)
						ClearComparison();
				});
			Messenger.Default.Register<PerformingComparisonMessage>(this, msg => ClearComparison());
			Messenger.Default.Register<SwitchViewMessage>(this, HandleSwitchView);

			_findCommand = new RelayCommand(Find);

			_selectedWordPairsMonitor = new SimpleMonitor();
			_currentVarietyPairState = CurrentVarietyPairState.NotSelected;
			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks", 
				new TaskAreaCommandViewModel("Compare this variety pair", new RelayCommand(PerformComparison)),
				new TaskAreaCommandViewModel("Find words", _findCommand),
				new TaskAreaItemsViewModel("Sort word pairs by", new TaskAreaCommandGroupViewModel(
					new TaskAreaCommandViewModel("Similarity", new RelayCommand(() => SortWordPairsBy("PhoneticSimilarityScore", ListSortDirection.Descending))),
					new TaskAreaCommandViewModel("Sense", new RelayCommand(() => SortWordPairsBy("Sense.Gloss", ListSortDirection.Ascending)))))));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export results for this variety pair", new RelayCommand(ExportVarietyPair))));
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			Set("Varieties", ref _varieties, new ReadOnlyMirroredList<Variety, VarietyViewModel>(_projectService.Project.Varieties, variety => new VarietyViewModel(variety), vm => vm.DomainVariety));
			_deferredResetCurrentVarietyPair = true;
		}

		private void HandleSwitchView(SwitchViewMessage msg)
		{
			if (msg.ViewModelType == GetType())
			{
				_busyService.ShowBusyIndicatorUntilUpdated();

				var pair = (VarietyPair) msg.DomainModels[0];
				CurrentVarietyPair = _varietyPairFactory(pair, true);
				Set(() => CurrentVariety1, ref _currentVariety1, _varieties[pair.Variety1]);
				Set(() => CurrentVariety2, ref _currentVariety2, _varieties[pair.Variety2]);
				CurrentVarietyPairState = pair.Variety1.VarietyPairs.Contains(pair.Variety2)
					? CurrentVarietyPairState.SelectedAndCompared : CurrentVarietyPairState.SelectedAndNotCompared;
			}
		}

		private void SortWordPairsBy(string propertyName, ListSortDirection sortDirection)
		{
			_sortPropertyName = propertyName;
			_sortDirection = sortDirection;
			if (_currentVarietyPair != null)
			{
				_currentVarietyPair.Cognates.UpdateSort(_sortPropertyName, _sortDirection);
				_currentVarietyPair.Noncognates.UpdateSort(_sortPropertyName, _sortDirection);
			}
		}

		private void ClearComparison()
		{
			ResetCurrentVarietyPair();
			CurrentVarietyPair = null;
			if (CurrentVarietyPairState == CurrentVarietyPairState.SelectedAndCompared)
				CurrentVarietyPairState = CurrentVarietyPairState.SelectedAndNotCompared;
		}

		protected override void OnIsCurrentChanged()
		{
			if (IsCurrent)
			{
				Messenger.Default.Send(new HookFindMessage(_findCommand));
			}
			else
			{
				_dialogService.CloseDialog(_findViewModel);
				Messenger.Default.Send(new HookFindMessage(null));
			}
		}

		private void PerformComparison()
		{
			if (_currentVarietyPairState == CurrentVarietyPairState.NotSelected || _currentVarietyPair != null)
				return;

			_busyService.ShowBusyIndicatorUntilUpdated();
			CogProject project = _projectService.Project;
			var pair = new VarietyPair(_currentVariety1.DomainVariety, _currentVariety2.DomainVariety);
			project.VarietyPairs.Add(pair);

			_analysisService.Compare(pair);

			CurrentVarietyPair = _varietyPairFactory(pair, true);
			CurrentVarietyPairState = CurrentVarietyPairState.SelectedAndCompared;
		}

		private void Find()
		{
			if ( _findViewModel != null)
				return;

			_findViewModel = new FindViewModel(_dialogService, FindNext);
			_findViewModel.PropertyChanged += (sender, args) => ResetSearch();
			_dialogService.ShowModelessDialog(this, _findViewModel, () => _findViewModel = null);
		}

		/// <summary>
		/// Finds the next.
		/// </summary>
		private void FindNext()
		{
			if (_currentVarietyPair == null || (_currentVarietyPair.Cognates.WordPairs.Count == 0 && _currentVarietyPair.Noncognates.WordPairs.Count == 0))
			{
				SearchEnded();
				return;
			}

			WordPairsViewModel cognates = _currentVarietyPair.Cognates;
			WordPairsViewModel noncognates = _currentVarietyPair.Noncognates;
			if (cognates.WordPairs.Count > 0 && cognates.SelectedWordPairs.Count == 0 && noncognates.SelectedWordPairs.Count == 0)
			{
				_startWordPairs = cognates;
			}
			else if (_startWordPairs == null)
			{
				_startWordPairs = cognates.SelectedWordPairs.Count > 0 ? cognates : noncognates;
			}

			WordPairsViewModel curWordPairs;
			if (cognates.SelectedWordPairs.Count > 0)
				curWordPairs = cognates;
			else if (noncognates.SelectedWordPairs.Count > 0)
				curWordPairs = noncognates;
			else
				curWordPairs = _startWordPairs;

			bool startAtBeginning = false;
			while (true)
			{
				using (_selectedWordPairsMonitor.Enter())
				{
					if (curWordPairs.FindNext(_findViewModel.Field, _findViewModel.String, _searchWrapped, startAtBeginning))
					{
						WordPairsViewModel otherWordPairs = curWordPairs == cognates ? noncognates : cognates;
						otherWordPairs.ClearPreviousSearchHit();
						break;
					}
				}

				if (curWordPairs == _startWordPairs)
				{
					if (_startWordPairs.IsSearching)
					{
						curWordPairs = _startWordPairs == cognates ? noncognates : cognates;
					}
					else
					{
						SearchEnded();
						break;
					}
				}
				else if (_startWordPairs.IsSearching)
				{
					curWordPairs = _startWordPairs;
					startAtBeginning = true;
					_searchWrapped = true;
				}
				else
				{
					SearchEnded();
					break;
				}
			}
		}

		private void ResetSearch()
		{
			_startWordPairs = null;
			if (_currentVarietyPair != null)
			{
				_currentVarietyPair.Cognates.ResetSearch();
				_currentVarietyPair.Noncognates.ResetSearch();
			}
			_searchWrapped = false;
		}

		private void SearchEnded()
		{
			_findViewModel.ShowSearchEndedMessage();
			ResetSearch();
		}

		private void ExportVarietyPair()
		{
			if (_currentVarietyPairState != CurrentVarietyPairState.SelectedAndCompared)
				return;

			_exportService.ExportVarietyPair(this, _currentVarietyPair.DomainVarietyPair);
		}

		private void ResetCurrentVarietyPair()
		{
			if (_varieties.Count > 0)
			{
				if (_varietiesView1 == null || _varietiesView2 == null)
				{
					_deferredResetCurrentVarietyPair = true;
				}
				else
				{
					Set(() => CurrentVariety1, ref _currentVariety1, _varietiesView1.Cast<VarietyViewModel>().First());
					if (_varieties.Count > 1)
						Set(() => CurrentVariety2, ref _currentVariety2, _varietiesView2.Cast<VarietyViewModel>().ElementAt(1));
					else
						Set(() => CurrentVariety2, ref _currentVariety2, _varietiesView2.Cast<VarietyViewModel>().First());
					SetCurrentVarietyPair();
				}
			}
			else
			{
				Set(() => CurrentVariety1, ref _currentVariety1, null);
				Set(() => CurrentVariety2, ref _currentVariety2, null);
				SetCurrentVarietyPair();
			}
		}

		public ICommand FindCommand
		{
			get { return _findCommand; }
		}

		public ReadOnlyObservableList<VarietyViewModel> Varieties
		{
			get { return _varieties; }
		}

		public ICollectionView VarietiesView1
		{
			get { return _varietiesView1; }
			set
			{
				if (Set(() => VarietiesView1, ref _varietiesView1, value))
				{
					_varietiesView1.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
					if (_deferredResetCurrentVarietyPair)
						ResetCurrentVarietyPair();
				}
			}
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
			set
			{
				if (Set(() => VarietiesView2, ref _varietiesView2, value))
				{
					_varietiesView2.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
					if (_deferredResetCurrentVarietyPair)
						ResetCurrentVarietyPair();
				}
			}
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
					_deferredResetCurrentVarietyPair = false;
					ResetSearch();
					if (oldCurVarietyPair != null)
					{
						oldCurVarietyPair.Cognates.SelectedWordPairs.CollectionChanged -= SelectedWordPairsChanged;
						oldCurVarietyPair.Noncognates.SelectedWordPairs.CollectionChanged -= SelectedWordPairsChanged;
					}

					if (_currentVarietyPair != null)
					{
						_currentVarietyPair.Cognates.UpdateSort(_sortPropertyName, _sortDirection);
						_currentVarietyPair.Noncognates.UpdateSort(_sortPropertyName, _sortDirection);
						_currentVarietyPair.Cognates.SelectedWordPairs.CollectionChanged += SelectedWordPairsChanged;
						_currentVarietyPair.Noncognates.SelectedWordPairs.CollectionChanged += SelectedWordPairsChanged;
					}
				}
			}
		}

		private void SelectedWordPairsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!_selectedWordPairsMonitor.Busy)
				ResetSearch();
		}

		public CurrentVarietyPairState CurrentVarietyPairState
		{
			get { return _currentVarietyPairState; }
			private set { Set(() => CurrentVarietyPairState, ref _currentVarietyPairState, value); }
		}

		private void SetCurrentVarietyPair()
		{
			if (_currentVariety1 != null && _currentVariety2 != null && _currentVariety1 != _currentVariety2)
			{
				VarietyPair pair;
				if (_currentVariety1.DomainVariety.VarietyPairs.TryGetValue(_currentVariety2.DomainVariety, out pair))
				{
					_busyService.ShowBusyIndicatorUntilUpdated();
					CurrentVarietyPair = _varietyPairFactory(pair, _currentVariety1.DomainVariety == pair.Variety1);
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
	}
}
