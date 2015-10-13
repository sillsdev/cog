using System;
using System.Collections.Specialized;
using System.ComponentModel;
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
	public enum VarietyPairState
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
		private MirroredBindableList<Variety, VarietyViewModel> _varieties;
		private ICollectionView _varietiesView1;
		private ICollectionView _varietiesView2;
		private VarietyViewModel _selectedVariety1;
		private VarietyViewModel _selectedVariety2;
		private VarietyPairViewModel _selectedVarietyPair;
		private VarietyPairState _varietyPairState;
		private readonly IExportService _exportService;
		private readonly IDialogService _dialogService;
		private readonly ICommand _findCommand;

		private string _sortPropertyName;
		private ListSortDirection _sortDirection;

		private FindViewModel _findViewModel;
		private WordPairsViewModel _startWordPairs;
		private readonly SimpleMonitor _selectedWordPairsMonitor;
		private bool _deferredResetSelectedVarietyPair;
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

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => SetSelectedVarietyPair());
			Messenger.Default.Register<DomainModelChangedMessage>(this, msg =>
				{
					if (msg.AffectsComparison)
						ClearComparison();
				});
			Messenger.Default.Register<PerformingComparisonMessage>(this, msg => ClearComparison());
			Messenger.Default.Register<SwitchViewMessage>(this, HandleSwitchView);

			_findCommand = new RelayCommand(Find);

			_selectedWordPairsMonitor = new SimpleMonitor();
			_varietyPairState = VarietyPairState.NotSelected;
			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks", 
				new TaskAreaCommandViewModel("Compare this variety pair", new RelayCommand(PerformComparison)),
				new TaskAreaCommandViewModel("Find words", _findCommand),
				new TaskAreaItemsViewModel("Sort word pairs by", new TaskAreaCommandGroupViewModel(
					new TaskAreaCommandViewModel("Similarity", new RelayCommand(() => SortWordPairsBy("PhoneticSimilarityScore", ListSortDirection.Descending))),
					new TaskAreaCommandViewModel("Gloss", new RelayCommand(() => SortWordPairsBy("Meaning.Gloss", ListSortDirection.Ascending)))))));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export results for this variety pair", new RelayCommand(ExportVarietyPair))));
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			_deferredResetSelectedVarietyPair = true;
			_varietiesView1 = null;
			_varietiesView2 = null;
			Set("Varieties", ref _varieties, new MirroredBindableList<Variety, VarietyViewModel>(_projectService.Project.Varieties, variety => new VarietyViewModel(variety), vm => vm.DomainVariety));
		}

		private void HandleSwitchView(SwitchViewMessage msg)
		{
			if (msg.ViewModelType == GetType())
			{
				_busyService.ShowBusyIndicatorUntilFinishDrawing();

				var pair = (VarietyPair) msg.DomainModels[0];
				SelectedVarietyPair = _varietyPairFactory(pair, true);
				Set(() => SelectedVariety1, ref _selectedVariety1, _varieties[pair.Variety1]);
				Set(() => SelectedVariety2, ref _selectedVariety2, _varieties[pair.Variety2]);
				VarietyPairState = pair.Variety1.VarietyPairs.Contains(pair.Variety2)
					? VarietyPairState.SelectedAndCompared : VarietyPairState.SelectedAndNotCompared;
				if (msg.DomainModels.Count > 1)
				{
					var meaning = (Meaning) msg.DomainModels[1];
					_selectedVarietyPair.Cognates.SelectedWordPairs.Clear();
					_selectedVarietyPair.Cognates.SelectedWordPairs.AddRange(_selectedVarietyPair.Cognates.WordPairs.Where(wp => wp.Meaning.DomainMeaning == meaning));
					_selectedVarietyPair.Noncognates.SelectedWordPairs.Clear();
					_selectedVarietyPair.Noncognates.SelectedWordPairs.AddRange(_selectedVarietyPair.Noncognates.WordPairs.Where(wp => wp.Meaning.DomainMeaning == meaning));
				}
			}
		}

		private void SortWordPairsBy(string propertyName, ListSortDirection sortDirection)
		{
			_sortPropertyName = propertyName;
			_sortDirection = sortDirection;
			if (_selectedVarietyPair != null)
			{
				_selectedVarietyPair.Cognates.UpdateSort(_sortPropertyName, _sortDirection);
				_selectedVarietyPair.Noncognates.UpdateSort(_sortPropertyName, _sortDirection);
			}
		}

		private void ClearComparison()
		{
			ResetSelectedVarietyPair();
			SelectedVarietyPair = null;
			if (VarietyPairState == VarietyPairState.SelectedAndCompared)
				VarietyPairState = VarietyPairState.SelectedAndNotCompared;
		}

		protected override void OnIsSelectedChanged()
		{
			if (IsSelected)
			{
				Messenger.Default.Send(new HookFindMessage(_findCommand));
			}
			else if (_findViewModel != null)
			{
				_dialogService.CloseDialog(_findViewModel);
				Messenger.Default.Send(new HookFindMessage(null));
			}
		}

		private void PerformComparison()
		{
			if (_varietyPairState == VarietyPairState.NotSelected || _selectedVarietyPair != null)
				return;

			_busyService.ShowBusyIndicatorUntilFinishDrawing();
			CogProject project = _projectService.Project;
			var pair = new VarietyPair(_selectedVariety1.DomainVariety, _selectedVariety2.DomainVariety);
			project.VarietyPairs.Add(pair);

			_analysisService.Compare(pair);

			SelectedVarietyPair = _varietyPairFactory(pair, true);
			VarietyPairState = VarietyPairState.SelectedAndCompared;
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
			if (_selectedVarietyPair == null || (_selectedVarietyPair.Cognates.WordPairs.Count == 0 && _selectedVarietyPair.Noncognates.WordPairs.Count == 0))
			{
				SearchEnded();
				return;
			}

			WordPairsViewModel cognates = _selectedVarietyPair.Cognates;
			WordPairsViewModel noncognates = _selectedVarietyPair.Noncognates;
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
			if (_selectedVarietyPair != null)
			{
				_selectedVarietyPair.Cognates.ResetSearch();
				_selectedVarietyPair.Noncognates.ResetSearch();
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
			if (_varietyPairState != VarietyPairState.SelectedAndCompared)
				return;

			_exportService.ExportVarietyPair(this, _selectedVarietyPair.DomainVarietyPair);
		}

		private void ResetSelectedVarietyPair()
		{
			if (_varieties != null && _varieties.Count > 0)
			{
				if (_varietiesView1 == null || _varietiesView2 == null)
				{
					_deferredResetSelectedVarietyPair = true;
				}
				else
				{
					Set(() => SelectedVariety1, ref _selectedVariety1, _varietiesView1.Cast<VarietyViewModel>().First());
					if (_varieties.Count > 1)
						Set(() => SelectedVariety2, ref _selectedVariety2, _varietiesView2.Cast<VarietyViewModel>().ElementAt(1));
					else
						Set(() => SelectedVariety2, ref _selectedVariety2, _varietiesView2.Cast<VarietyViewModel>().First());
					SetSelectedVarietyPair();
				}
			}
			else
			{
				Set(() => SelectedVariety1, ref _selectedVariety1, null);
				Set(() => SelectedVariety2, ref _selectedVariety2, null);
				SetSelectedVarietyPair();
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
					if (_deferredResetSelectedVarietyPair)
						ResetSelectedVarietyPair();
				}
			}
		}

		public VarietyViewModel SelectedVariety1
		{
			get { return _selectedVariety1; }
			set
			{
				if (Set(() => SelectedVariety1, ref _selectedVariety1, value))
					SetSelectedVarietyPair();
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
					if (_deferredResetSelectedVarietyPair)
						ResetSelectedVarietyPair();
				}
			}
		}

		public VarietyViewModel SelectedVariety2
		{
			get { return _selectedVariety2; }
			set
			{
				if (Set(() => SelectedVariety2, ref _selectedVariety2, value))
					SetSelectedVarietyPair();
			}
		}

		public VarietyPairViewModel SelectedVarietyPair
		{
			get { return _selectedVarietyPair; }
			set
			{
				VarietyPairViewModel oldCurVarietyPair = _selectedVarietyPair;
				if (Set(() => SelectedVarietyPair, ref _selectedVarietyPair, value))
				{
					_deferredResetSelectedVarietyPair = false;
					ResetSearch();
					if (oldCurVarietyPair != null)
					{
						oldCurVarietyPair.Cognates.SelectedWordPairs.CollectionChanged -= SelectedWordPairsChanged;
						oldCurVarietyPair.Noncognates.SelectedWordPairs.CollectionChanged -= SelectedWordPairsChanged;
					}

					if (_selectedVarietyPair != null)
					{
						_selectedVarietyPair.Cognates.UpdateSort(_sortPropertyName, _sortDirection);
						_selectedVarietyPair.Noncognates.UpdateSort(_sortPropertyName, _sortDirection);
						_selectedVarietyPair.Cognates.SelectedWordPairs.CollectionChanged += SelectedWordPairsChanged;
						_selectedVarietyPair.Noncognates.SelectedWordPairs.CollectionChanged += SelectedWordPairsChanged;
					}
				}
			}
		}

		private void SelectedWordPairsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!_selectedWordPairsMonitor.Busy)
				ResetSearch();
		}

		public VarietyPairState VarietyPairState
		{
			get { return _varietyPairState; }
			private set { Set(() => VarietyPairState, ref _varietyPairState, value); }
		}

		private void SetSelectedVarietyPair()
		{
			if (_selectedVariety1 != null && _selectedVariety2 != null && _selectedVariety1 != _selectedVariety2)
			{
				VarietyPair pair;
				if (_selectedVariety1.DomainVariety.VarietyPairs.TryGetValue(_selectedVariety2.DomainVariety, out pair))
				{
					_busyService.ShowBusyIndicatorUntilFinishDrawing();
					SelectedVarietyPair = _varietyPairFactory(pair, _selectedVariety1.DomainVariety == pair.Variety1);
					VarietyPairState = VarietyPairState.SelectedAndCompared;
				}
				else
				{
					SelectedVarietyPair = null;
					VarietyPairState = VarietyPairState.SelectedAndNotCompared;
				}
			}
			else
			{
				SelectedVarietyPair = null;
				VarietyPairState = VarietyPairState.NotSelected;
			}
		}
	}
}
