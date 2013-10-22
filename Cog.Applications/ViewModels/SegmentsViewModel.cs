using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Applications.ViewModels
{
	public class SegmentsViewModel : WorkspaceViewModelBase
	{
		private readonly static Dictionary<string, string> PlaceCategoryLookup = new Dictionary<string, string>
			{
				{"bilabial", "Labial"},
				{"labiodental", "Labial"},
				{"dental", "Coronal"},
				{"alveolar", "Coronal"},
				{"retroflex", "Coronal"},
				{"palato-alveolar", "Coronal"},
				{"alveolo-palatal", "Coronal"},
				{"palatal", "Dorsal"},
				{"velar", "Dorsal"},
				{"uvular", "Dorsal"},
				{"pharyngeal", "Guttural"},
				{"epiglottal", "Guttural"},
				{"glottal", "Guttural"}
			};

		private readonly static Dictionary<string, string> HeightCategoryLookup = new Dictionary<string, string>
			{
				{"close-vowel", "Close"},
				{"mid-vowel", "Mid"},
				{"open-vowel", "Open"}
			};

		private readonly static Dictionary<string, int> CategorySortOrderLookup = new Dictionary<string, int>
			{
				{"Close", 0},
				{"Mid", 1},
				{"Open", 2},
				{"Labial", 0},
				{"Coronal", 1},
				{"Dorsal", 2},
				{"Guttural", 3}
			};

		private readonly IProjectService _projectService;
		private readonly IBusyService _busyService;
		private readonly BulkObservableList<Segment> _domainSegments; 
		private readonly BindableList<SegmentViewModel> _segments;
		private readonly ReadOnlyObservableList<SegmentViewModel> _readonlySegments;
		private readonly BindableList<SegmentCategoryViewModel> _categories;
		private readonly ReadOnlyObservableList<SegmentCategoryViewModel> _readonlyCategories;
		private ReadOnlyMirroredList<Variety, SegmentsVarietyViewModel> _varieties;
		private SyllablePosition _syllablePosition;
		private VarietySegmentViewModel _selectedSegment;
		private readonly BindableList<WordViewModel> _currentWords;
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
					new TaskAreaCommandViewModel("Sense", new RelayCommand(() => SortWordsBy("Sense.Gloss", ListSortDirection.Ascending))),
					new TaskAreaCommandViewModel("Form", new RelayCommand(() => SortWordsBy("StrRep", ListSortDirection.Ascending)))))));

			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export segment frequencies", new RelayCommand(() => _exportService.ExportSegmentFrequencies(this, _syllablePosition)))));

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			Messenger.Default.Register<DomainModelChangedMessage>(this, msg =>
				{
					if (msg.AffectsComparison)
						PopulateSegments();
				});

			_currentWords = new BindableList<WordViewModel>();
			_observedWords = wordsFactory(_currentWords);
			_domainSegments = new BulkObservableList<Segment>();
			_segments = new BindableList<SegmentViewModel>();
			_readonlySegments = new ReadOnlyObservableList<SegmentViewModel>(_segments);
			_categories = new BindableList<SegmentCategoryViewModel>();
			_readonlyCategories = new ReadOnlyObservableList<SegmentCategoryViewModel>(_categories);
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
			Set("Varieties", ref _varieties, new ReadOnlyMirroredList<Variety, SegmentsVarietyViewModel>(project.Varieties, variety => new SegmentsVarietyViewModel(this, variety), vm => vm.DomainVariety));
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

		private void PopulateSegments()
		{
			var comparer = new SegmentComparer();
			_busyService.ShowBusyIndicatorUntilUpdated();
			using (_domainSegments.BulkUpdate())
			using (_segments.BulkUpdate())
			{
				_domainSegments.Clear();
				_segments.Clear();
				foreach (Segment segment in _projectService.Project.Varieties
					.SelectMany(v => v.SyllablePositionSegmentFrequencyDistributions[DomainSyllablePosition].ObservedSamples)
					.Distinct().OrderBy(s => CategorySortOrderLookup[GetCategory(s)]).ThenBy(s => s, comparer))
				{
					_domainSegments.Add(segment);
					_segments.Add(new SegmentViewModel(segment));
				}
			}

			_categories.ReplaceAll(_segments.GroupBy(s => GetCategory(s.DomainSegment)).OrderBy(g => CategorySortOrderLookup[g.Key]).Select(g => new SegmentCategoryViewModel(g.Key, g)));
			HasSegments = _segments.Count > 0;
		}

		private string GetCategory(Segment segment)
		{
			FeatureStruct fs = segment.FeatureStruct;
			if (segment.IsComplex)
				fs = segment.FeatureStruct.GetValue(CogFeatureSystem.First);

			return segment.Type == CogFeatureSystem.VowelType ? HeightCategoryLookup[((FeatureSymbol) fs.GetValue<SymbolicFeatureValue>("manner")).ID]
				: PlaceCategoryLookup[((FeatureSymbol) fs.GetValue<SymbolicFeatureValue>("place")).ID];
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
					_busyService.ShowBusyIndicatorUntilUpdated();
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

		private class SegmentComparer : IComparer<Segment>
		{
			private static readonly Dictionary<string, int> PlaceSortOrderLookup = new Dictionary<string, int>
				{
					{"bilabial", 0},
					{"labiodental", 1},
					{"dental", 2},
					{"alveolar", 3},
					{"retroflex", 4},
					{"palato-alveolar", 5},
					{"alveolo-palatal", 6},
					{"palatal", 7},
					{"velar", 8},
					{"uvular", 9},
					{"pharyngeal", 10},
					{"epiglottal", 11},
					{"glottal", 12}
				};

			private static readonly Dictionary<string, int> MannerSortOrderLookup = new Dictionary<string, int>
				{
					{"stop", 0},
					{"affricate", 1},
					{"fricative", 2},
					{"approximant", 3},
					{"trill", 4},
					{"flap", 5}
				};

			private static readonly Dictionary<string, int> NasalSortOrderLookup = new Dictionary<string, int>
				{
					{"nasal-", 0},
					{"nasal+", 1}
				};

			private static readonly Dictionary<string, int> LateralSortOrderLookup = new Dictionary<string, int>
				{
					{"lateral-", 0},
					{"lateral+", 1}
				};

			private static readonly Dictionary<string, int> VoiceSortOrderLookup = new Dictionary<string, int>
				{
					{"voice-", 0},
					{"voice+", 1}
				};

			private static readonly Dictionary<string, int> HeightSortOrderLookup = new Dictionary<string, int>
				{
					{"close", 0},
					{"near-close", 1},
					{"close-mid", 2},
					{"mid", 3},
					{"open-mid", 4},
					{"near-open", 5},
					{"open", 6}
				};

			private static readonly Dictionary<string, int> BacknessSortOrderLookup = new Dictionary<string, int>
				{
					{"front", 0},
					{"near-front", 1},
					{"central", 2},
					{"near-back", 3},
					{"back", 4}
				};

			private static readonly Dictionary<string, int> RoundSortOrderLookup = new Dictionary<string, int>
				{
					{"round-", 0},
					{"round+", 1}
				};

			private static readonly Tuple<string, Dictionary<string, int>>[] ConsonantFeatureSortOrder =
				{
					Tuple.Create("lateral", LateralSortOrderLookup),
					Tuple.Create("nasal", NasalSortOrderLookup),
					Tuple.Create("manner", MannerSortOrderLookup),
					Tuple.Create("place", PlaceSortOrderLookup),
					Tuple.Create("voice", VoiceSortOrderLookup)
				};

			private static readonly Tuple<string, Dictionary<string, int>>[] VowelFeatureSortOrder =
				{
					Tuple.Create("backness", BacknessSortOrderLookup),
					Tuple.Create("height", HeightSortOrderLookup),
					Tuple.Create("round", RoundSortOrderLookup)
				};

			public int Compare(Segment x, Segment y)
			{
				Tuple<string, Dictionary<string, int>>[] features = x.Type == CogFeatureSystem.ConsonantType ? ConsonantFeatureSortOrder : VowelFeatureSortOrder;

				FeatureStruct fsx = x.FeatureStruct;
				if (x.IsComplex)
					fsx = x.FeatureStruct.GetValue(CogFeatureSystem.First);
				FeatureStruct fsy = y.FeatureStruct;
				if (y.IsComplex)
					fsy = y.FeatureStruct.GetValue(CogFeatureSystem.First);

				foreach (Tuple<string, Dictionary<string, int>> feature in features)
				{
					int res = Compare(feature.Item1, feature.Item2, fsx, fsy);
					if (res != 0)
						return res;
				}

				return string.Compare(x.StrRep, y.StrRep, StringComparison.Ordinal);
			}

			private int Compare(string feature, Dictionary<string, int> sortOrderLookup, FeatureStruct fsx, FeatureStruct fsy)
			{
				int valx = sortOrderLookup[((FeatureSymbol) fsx.GetValue<SymbolicFeatureValue>(feature)).ID];
				int valy = sortOrderLookup[((FeatureSymbol) fsy.GetValue<SymbolicFeatureValue>(feature)).ID];
				return valx.CompareTo(valy);
			}
		}
	}
}
