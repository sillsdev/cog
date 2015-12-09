using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Application.Collections;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Application.ViewModels
{
	public class SegmentsViewModel : WorkspaceViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly IBusyService _busyService;
		private readonly BulkObservableList<Segment> _domainSegments; 
		private readonly BulkObservableList<SegmentViewModel> _segments;
		private readonly ReadOnlyBindableList<SegmentViewModel> _readonlySegments;
		private readonly BulkObservableList<SegmentCategoryViewModel> _categories;
		private readonly ReadOnlyBindableList<SegmentCategoryViewModel> _readonlyCategories;
		private MirroredBindableList<Variety, SegmentsVarietyViewModel> _varieties;
		private SyllablePosition _syllablePosition;
		private VarietySegmentViewModel _selectedSegment;
		private readonly BulkObservableList<WordViewModel> _currentWords;
		private readonly WordsViewModel _observedWords;
		private readonly WordViewModel.Factory _wordFactory;
		private readonly ICommand _findCommand;
		private readonly IDialogService _dialogService;
		private readonly IExportService _exportService;
		private bool _hasSegments;

		private string _sortPropertyName;
		private ListSortDirection _sortDirection;

		private FindViewModel _findViewModel;

		public SegmentsViewModel(IProjectService projectService, IDialogService dialogService, IBusyService busyService, IExportService exportService, WordsViewModel.Factory wordsFactory,
			WordViewModel.Factory wordFactory)
			: base("Segments")
		{
			_projectService = projectService;
			_busyService = busyService;
			_dialogService = dialogService;
			_exportService = exportService;
			_wordFactory = wordFactory;

			_findCommand = new RelayCommand(Find);

			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Syllable position",
				new TaskAreaCommandViewModel("Onset", new RelayCommand(() => SyllablePosition = SyllablePosition.Onset)),
				new TaskAreaCommandViewModel("Nucleus", new RelayCommand(() => SyllablePosition = SyllablePosition.Nucleus)),
				new TaskAreaCommandViewModel("Coda", new RelayCommand(() => SyllablePosition = SyllablePosition.Coda))));

			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
				new TaskAreaCommandViewModel("Find words", _findCommand),
				new TaskAreaItemsViewModel("Sort words by", new TaskAreaCommandGroupViewModel(
					new TaskAreaCommandViewModel("Meaning", new RelayCommand(() => SortWordsBy("Meaning.Gloss", ListSortDirection.Ascending))),
					new TaskAreaCommandViewModel("Form", new RelayCommand(() => SortWordsBy("StrRep", ListSortDirection.Ascending)))))));

			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export segment frequencies", new RelayCommand(ExportSegmentFrequencies, CanExportSegmentFrequencies))));

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			Messenger.Default.Register<DomainModelChangedMessage>(this, msg =>
				{
					if (msg.AffectsComparison)
						PopulateSegments();
				});

			_currentWords = new BulkObservableList<WordViewModel>();
			_observedWords = wordsFactory(new ReadOnlyBindableList<WordViewModel>(_currentWords));
			_domainSegments = new BulkObservableList<Segment>();
			_segments = new BulkObservableList<SegmentViewModel>();
			_readonlySegments = new ReadOnlyBindableList<SegmentViewModel>(_segments);
			_categories = new BulkObservableList<SegmentCategoryViewModel>();
			_readonlyCategories = new ReadOnlyBindableList<SegmentCategoryViewModel>(_categories);
		}

		private void SortWordsBy(string propertyName, ListSortDirection sortDirection)
		{
			_sortPropertyName = propertyName;
			_sortDirection = sortDirection;
			_observedWords.UpdateSort(_sortPropertyName, _sortDirection);
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			CogProject project = _projectService.Project;
			Set("Varieties", ref _varieties, new MirroredBindableList<Variety, SegmentsVarietyViewModel>(project.Varieties, variety => new SegmentsVarietyViewModel(this, variety), vm => vm.DomainVariety));
			PopulateSegments();
		}

		private void Find()
		{
			if ( _findViewModel != null)
				return;

			_findViewModel = new FindViewModel(_dialogService, FindNext);
			_findViewModel.PropertyChanged += (sender, args) => _observedWords.ResetSearch();
			_dialogService.ShowModelessDialog(this, _findViewModel, () => _findViewModel = null);
		}

		private void FindNext()
		{
			if (!_observedWords.FindNext(_findViewModel.Field, _findViewModel.String))
				_findViewModel.ShowSearchEndedMessage();
		}

		private bool CanExportSegmentFrequencies()
		{
			return _projectService.Project.Varieties.Count > 0 && _projectService.Project.Meanings.Count > 0;
		}

		private void ExportSegmentFrequencies()
		{
			_exportService.ExportSegmentFrequencies(this, _syllablePosition);
		}

		private void PopulateSegments()
		{
			var segmentComparer = new SegmentComparer();
			var categoryComparer = new SegmentCategoryComparer();
			_busyService.ShowBusyIndicatorUntilFinishDrawing();
			using (_domainSegments.BulkUpdate())
			using (_segments.BulkUpdate())
			{
				_domainSegments.Clear();
				_segments.Clear();
				foreach (Segment segment in _projectService.Project.Varieties
					.SelectMany(v => v.SyllablePositionSegmentFrequencyDistributions[DomainSyllablePosition].ObservedSamples)
					.Distinct().OrderBy(s => s.Category(), categoryComparer).ThenBy(s => s, segmentComparer))
				{
					_domainSegments.Add(segment);
					_segments.Add(new SegmentViewModel(segment));
				}
			}

			_categories.ReplaceAll(_segments.GroupBy(s => s.DomainSegment.Category()).OrderBy(g => g.Key, categoryComparer).Select(g => new SegmentCategoryViewModel(g.Key, g)));
			HasSegments = _segments.Count > 0;
		}

		public bool HasSegments
		{
			get { return _hasSegments; }
			set { Set(() => HasSegments, ref _hasSegments, value); }
		}

		public ReadOnlyObservableList<SegmentCategoryViewModel> Categories
		{
			get { return _readonlyCategories; }
		}

		public ReadOnlyObservableList<SegmentViewModel> Segments
		{
			get { return _readonlySegments; }
		}

		public ReadOnlyObservableList<SegmentsVarietyViewModel> Varieties
		{
			get { return _varieties; }
		}

		public ICommand FindCommand
		{
			get { return _findCommand; }
		}

		public VarietySegmentViewModel SelectedSegment
		{
			get { return _selectedSegment; }
			set
			{
				if (Set(() => SelectedSegment, ref _selectedSegment, value))
				{
					_busyService.ShowBusyIndicatorUntilFinishDrawing();
					using (_currentWords.BulkUpdate())
					{
						_currentWords.Clear();
						if (_selectedSegment != null)
						{
							foreach (Word word in _selectedSegment.Variety.DomainVariety.Words)
							{
								WordViewModel vm = _wordFactory(word);
								bool add = false;
								foreach (WordSegmentViewModel seg in vm.Segments.Where(s => !s.IsBoundary && !s.IsNotInOriginal))
								{
									if (seg.DomainNode.StrRep() == _selectedSegment.StrRep)
									{
										FeatureSymbol pos = null;
										switch (_syllablePosition)
										{
											case SyllablePosition.Onset:
												pos = CogFeatureSystem.Onset;
												break;
											case SyllablePosition.Nucleus:
												pos = CogFeatureSystem.Nucleus;
												break;
											case SyllablePosition.Coda:
												pos = CogFeatureSystem.Coda;
												break;
										}

										SymbolicFeatureValue curPos;
										if (seg.DomainNode.Annotation.FeatureStruct.TryGetValue(CogFeatureSystem.SyllablePosition, out curPos) && (FeatureSymbol) curPos == pos)
										{
											seg.IsSelected = true;
											add = true;
										}
									}
								}
								if (add)
									_currentWords.Add(vm);
							}
						}
					}
				}
			}
		}

		public WordsViewModel ObservedWords
		{
			get { return _observedWords; }
		}

		public SyllablePosition SyllablePosition
		{
			get { return _syllablePosition; }
			set
			{
				if (Set(() => SyllablePosition, ref _syllablePosition, value))
					PopulateSegments();
			}
		}

		internal FeatureSymbol DomainSyllablePosition
		{
			get
			{
				switch (_syllablePosition)
				{
					case SyllablePosition.Onset:
						return CogFeatureSystem.Onset;
					case SyllablePosition.Nucleus:
						return CogFeatureSystem.Nucleus;
					case SyllablePosition.Coda:
						return CogFeatureSystem.Coda;
				}
				return null;
			}
		}

		internal ObservableList<Segment> DomainSegments
		{
			get { return _domainSegments; }
		}
	}
}
