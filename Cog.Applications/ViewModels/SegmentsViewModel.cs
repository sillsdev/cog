using System;
using System.Collections.Generic;
using System.Linq;
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

		private readonly static Dictionary<string, int> CategorySortOrderLookup = new Dictionary<string, int>
			{
				{"Vowel", 0},
				{"Labial", 0},
				{"Coronal", 1},
				{"Dorsal", 2},
				{"Guttural", 3}
			};

		private readonly IProjectService _projectService;
		private readonly BulkObservableList<Segment> _domainSegments; 
		private readonly BindableList<SegmentViewModel> _segments;
		private readonly ReadOnlyObservableList<SegmentViewModel> _readonlySegments;
		private readonly BindableList<SegmentCategoryViewModel> _categories;
		private readonly ReadOnlyObservableList<SegmentCategoryViewModel> _readonlyCategories;
		private ReadOnlyMirroredList<Variety, SegmentsVarietyViewModel> _varieties;
		private ViewModelSyllablePosition _syllablePosition;

		public SegmentsViewModel(IProjectService projectService)
			: base("Segments")
		{
			_projectService = projectService;

			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Syllable position",
				new TaskAreaCommandViewModel("Onset", new RelayCommand(() => SyllablePosition = ViewModelSyllablePosition.Onset)),
				new TaskAreaCommandViewModel("Nucleus", new RelayCommand(() => SyllablePosition = ViewModelSyllablePosition.Nucleus)),
				new TaskAreaCommandViewModel("Coda", new RelayCommand(() => SyllablePosition = ViewModelSyllablePosition.Coda))));

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			Messenger.Default.Register<DomainModelChangedMessage>(this, msg => PopulateSegments());

			_domainSegments = new BulkObservableList<Segment>();
			_segments = new BindableList<SegmentViewModel>();
			_readonlySegments = new ReadOnlyObservableList<SegmentViewModel>(_segments);
			_categories = new BindableList<SegmentCategoryViewModel>();
			_readonlyCategories = new ReadOnlyObservableList<SegmentCategoryViewModel>(_categories);
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			CogProject project = _projectService.Project;
			Set("Varieties", ref _varieties, new ReadOnlyMirroredList<Variety, SegmentsVarietyViewModel>(project.Varieties, variety => new SegmentsVarietyViewModel(this, variety), vm => vm.DomainVariety));
			PopulateSegments();
		}

		private void PopulateSegments()
		{
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

			if (_syllablePosition == ViewModelSyllablePosition.Nucleus)
				_categories.Clear();
			else
				_categories.ReplaceAll(_segments.GroupBy(s => GetCategory(s.DomainSegment)).OrderBy(g => CategorySortOrderLookup[g.Key]).Select(g => new SegmentCategoryViewModel(g.Key, g)));
		}

		private string GetCategory(Segment segment)
		{
			return segment.Type == CogFeatureSystem.VowelType ? "Vowel"
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
