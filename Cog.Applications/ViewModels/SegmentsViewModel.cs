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
		private ViewModelSyllablePosition _syllablePosition;
		private VarietySegmentViewModel _currentSegment;
		private readonly BindableList<WordViewModel> _currentWords;
		private readonly WordsViewModel _observedWords;
		private readonly WordViewModel.Factory _wordFactory;
		private readonly ICommand _findCommand;
		private readonly IDialogService _dialogService;

		private string _sortPropertyName;
		private ListSortDirection _sortDirection;

		private FindViewModel _findViewModel;

		public SegmentsViewModel(IProjectService projectService, IDialogService dialogService, IBusyService busyService, WordsViewModel.Factory wordsFactory, WordViewModel.Factory wordFactory)
			: base("Segments")
		{
			_projectService = projectService;
			_busyService = busyService;
			_dialogService = dialogService;
			_wordFactory = wordFactory;

			_findCommand = new RelayCommand(Find);

			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Syllable position",
				new TaskAreaCommandViewModel("Onset", new RelayCommand(() => SyllablePosition = ViewModelSyllablePosition.Onset)),
				new TaskAreaCommandViewModel("Nucleus", new RelayCommand(() => SyllablePosition = ViewModelSyllablePosition.Nucleus)),
				new TaskAreaCommandViewModel("Coda", new RelayCommand(() => SyllablePosition = ViewModelSyllablePosition.Coda))));

			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
					new TaskAreaCommandViewModel("Find words", _findCommand),
					new TaskAreaItemsViewModel("Sort words by", new TaskAreaCommandGroupViewModel(
						new TaskAreaCommandViewModel("Sense", new RelayCommand(() => SortWordsBy("Sense.Gloss", ListSortDirection.Ascending))),
						new TaskAreaCommandViewModel("Form", new RelayCommand(() => SortWordsBy("StrRep", ListSortDirection.Ascending)))))));

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			Messenger.Default.Register<DomainModelChangedMessage>(this, msg => PopulateSegments());

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
			_busyService.ShowBusyIndicatorUntilUpdated();
			using (_domainSegments.BulkUpdate())
			using (_segments.BulkUpdate())
			{
				_domainSegments.Clear();
				_segments.Clear();
				foreach (Segment segment in _projectService.Project.Varieties
					.SelectMany(v => v.SegmentFrequencyDistributions[DomainSyllablePosition].ObservedSamples)
					.Distinct().Where(s => !s.IsComplex()).OrderBy(s => CategorySortOrderLookup[GetCategory(s)]).ThenBy(s => s.StrRep))
				{
					_domainSegments.Add(segment);
					_segments.Add(new SegmentViewModel(segment));
				}
			}

			_categories.ReplaceAll(_segments.GroupBy(s => GetCategory(s.DomainSegment)).OrderBy(g => CategorySortOrderLookup[g.Key]).Select(g => new SegmentCategoryViewModel(g.Key, g)));
		}

		private string GetCategory(Segment segment)
		{
			return segment.Type == CogFeatureSystem.VowelType ? HeightCategoryLookup[((FeatureSymbol) segment.FeatureStruct.GetValue<SymbolicFeatureValue>("manner")).ID]
				: PlaceCategoryLookup[((FeatureSymbol) segment.FeatureStruct.GetValue<SymbolicFeatureValue>("place")).ID];
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

		public VarietySegmentViewModel CurrentSegment
		{
			get { return _currentSegment; }
			set
			{
				if (Set(() => CurrentSegment, ref _currentSegment, value))
				{
					_busyService.ShowBusyIndicatorUntilUpdated();
					using (_currentWords.BulkUpdate())
					{
						_currentWords.Clear();
						if (_currentSegment != null)
						{
							foreach (Word word in _currentSegment.Variety.DomainVariety.Words)
							{
								WordViewModel vm = _wordFactory(word);
								bool add = false;
								foreach (WordSegmentViewModel seg in vm.Segments)
								{
									if (seg.StrRep == _currentSegment.StrRep)
									{
										bool correctPosition = false;
										switch (_syllablePosition)
										{
											case ViewModelSyllablePosition.Onset:
												correctPosition = seg.DomainNode.Annotation.Parent.Children.First == seg.DomainNode.Annotation;
												break;
											case ViewModelSyllablePosition.Nucleus:
												correctPosition = true;
												break;
											case ViewModelSyllablePosition.Coda:
												correctPosition = seg.DomainNode.Annotation.Parent.Children.Last == seg.DomainNode.Annotation;
												break;
										}

										if (correctPosition)
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

		public ViewModelSyllablePosition SyllablePosition
		{
			get { return _syllablePosition; }
			set
			{
				if (Set(() => SyllablePosition, ref _syllablePosition, value))
					PopulateSegments();
			}
		}

		internal SyllablePosition DomainSyllablePosition
		{
			get
			{
				switch (_syllablePosition)
				{
					case ViewModelSyllablePosition.Onset:
						return Domain.SyllablePosition.Onset;
					case ViewModelSyllablePosition.Nucleus:
						return Domain.SyllablePosition.Nucleus;
					case ViewModelSyllablePosition.Coda:
						return Domain.SyllablePosition.Coda;
				}
				return Domain.SyllablePosition.Anywhere;
			}
		}

		internal ObservableList<Segment> DomainSegments
		{
			get { return _domainSegments; }
		}
	}
}
