using System.Linq;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Collections;

namespace SIL.Cog.Application.ViewModels
{
	public class BlairCognateIdentifierViewModel : ComponentSettingsViewModelBase
	{
		private readonly SegmentPool _segmentPool;
		private readonly IProjectService _projectService;
		private readonly SegmentMappingViewModel.Factory _mappingFactory;

		private bool _ignoreRegularInsertionDeletion;
		private bool _regularConsEqual;
		private readonly SimilarSegmentMappingsViewModel _similarVowels;
		private readonly SimilarSegmentMappingsViewModel _similarConsonants;
		private readonly SegmentMappingsViewModel _ignoredMappings;

		public BlairCognateIdentifierViewModel(SegmentPool segmentPool, IProjectService projectService, SegmentMappingsViewModel ignoredMappings,
			SimilarSegmentMappingsViewModel.Factory similarSegmentMappingsFactory, SegmentMappingViewModel.Factory mappingFactory)
			: base("Blair")
		{
			_segmentPool = segmentPool;
			_projectService = projectService;
			_mappingFactory = mappingFactory;
			_ignoredMappings = ignoredMappings;
			_ignoredMappings.PropertyChanged += ChildPropertyChanged;
			_similarVowels = similarSegmentMappingsFactory(SoundType.Vowel);
			_similarVowels.PropertyChanged += ChildPropertyChanged;
			_similarConsonants = similarSegmentMappingsFactory(SoundType.Consonant);
			_similarConsonants.PropertyChanged += ChildPropertyChanged;
		}

		public override void Setup()
		{
			_ignoredMappings.SelectedMapping = null;
			_ignoredMappings.Mappings.Clear();

			ICognateIdentifier cognateIdentifier = _projectService.Project.CognateIdentifiers[ComponentIdentifiers.PrimaryCognateIdentifier];
			var blair = cognateIdentifier as BlairCognateIdentifier;
			if (blair == null)
			{
				Set(() => IgnoreRegularInsertionDeletion, ref _ignoreRegularInsertionDeletion, false);
				Set(() => RegularConsonantsEqual, ref _regularConsEqual, false);
				_similarVowels.SegmentMappings = null;
				_similarConsonants.SegmentMappings = null;
			}
			else
			{
				Set(() => IgnoreRegularInsertionDeletion, ref _ignoreRegularInsertionDeletion, blair.IgnoreRegularInsertionDeletion);
				Set(() => RegularConsonantsEqual, ref _regularConsEqual, blair.RegularConsonantEqual);
				var ignoredMappings = (ListSegmentMappings) blair.IgnoredMappings;
				foreach (UnorderedTuple<string, string> mapping in ignoredMappings.Mappings)
					_ignoredMappings.Mappings.Add(_mappingFactory(mapping.Item1, mapping.Item2));
				var segmentMappings = (TypeSegmentMappings) blair.SimilarSegments;
				_similarVowels.SegmentMappings = (UnionSegmentMappings) segmentMappings.VowelMappings;
				_similarConsonants.SegmentMappings = (UnionSegmentMappings) segmentMappings.ConsonantMappings;
			}
			_similarVowels.Setup();
			_similarConsonants.Setup();
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			_ignoredMappings.AcceptChanges();
			_similarVowels.AcceptChanges();
			_similarConsonants.AcceptChanges();
		}

		public bool IgnoreRegularInsertionDeletion
		{
			get { return _ignoreRegularInsertionDeletion; }
			set { SetChanged(() => IgnoreRegularInsertionDeletion, ref _ignoreRegularInsertionDeletion, value); }
		}

		public bool RegularConsonantsEqual
		{
			get { return _regularConsEqual; }
			set { SetChanged(() => RegularConsonantsEqual, ref _regularConsEqual, value); }
		}

		public SegmentMappingsViewModel IgnoredMappings
		{
			get { return _ignoredMappings; }
		}

		public SimilarSegmentMappingsViewModel SimilarVowels
		{
			get { return _similarVowels; }
		}

		public SimilarSegmentMappingsViewModel SimilarConsonants
		{
			get { return _similarConsonants; }
		}

		public override object UpdateComponent()
		{
			_similarVowels.UpdateComponent();
			_similarConsonants.UpdateComponent();
			var cognateIdentifier = new BlairCognateIdentifier(_segmentPool, _ignoreRegularInsertionDeletion, _regularConsEqual,
				new ListSegmentMappings(_projectService.Project.Segmenter, _ignoredMappings.Mappings.Select(m => UnorderedTuple.Create(m.Segment1, m.Segment2)), false),
				new TypeSegmentMappings(_similarVowels.SegmentMappings, _similarConsonants.SegmentMappings));
			_projectService.Project.CognateIdentifiers[ComponentIdentifiers.PrimaryCognateIdentifier] = cognateIdentifier;
			return cognateIdentifier;
		}
	}
}
