using System;
using System.Linq;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Application.ViewModels
{
	public class BlairCognateIdentifierViewModel : ComponentSettingsViewModelBase
	{
		private readonly SegmentPool _segmentPool;
		private readonly IProjectService _projectService;

		private bool _ignoreRegularInsertionDeletion;
		private bool _regularConsEqual;
		private readonly SimilarSegmentMappingsOptionsViewModel _similarVowels;
		private readonly SimilarSegmentMappingsOptionsViewModel _similarConsonants;
		private readonly SegmentMappingsViewModel _ignoredMappings;

		public BlairCognateIdentifierViewModel(SegmentPool segmentPool, IProjectService projectService, SegmentMappingsViewModel ignoredMappings,
			SimilarSegmentMappingsOptionsViewModel.Factory similarSegmentMappingsFactory)
			: base("Blair")
		{
			_segmentPool = segmentPool;
			_projectService = projectService;
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
				foreach (Tuple<string, string> mapping in ignoredMappings.Mappings)
					_ignoredMappings.Mappings.Add(new SegmentMappingViewModel(_projectService.Project.Segmenter, mapping.Item1, mapping.Item2));
				var segmentMappings = (TypeSegmentMappings) blair.SimilarSegments;
				_similarVowels.SegmentMappings = segmentMappings.VowelMappings;
				_similarConsonants.SegmentMappings = segmentMappings.ConsonantMappings;
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

		public ComponentOptionsViewModel SimilarVowels
		{
			get { return _similarVowels; }
		}

		public ComponentOptionsViewModel SimilarConsonants
		{
			get { return _similarConsonants; }
		}

		public override object UpdateComponent()
		{
			var cognateIdentifier = new BlairCognateIdentifier(_segmentPool, _ignoreRegularInsertionDeletion, _regularConsEqual,
				new ListSegmentMappings(_projectService.Project.Segmenter, _ignoredMappings.Mappings.Select(m => Tuple.Create(m.Segment1, m.Segment2)), false),
				new TypeSegmentMappings((ISegmentMappings) _similarVowels.UpdateComponent(), (ISegmentMappings) _similarConsonants.UpdateComponent()));
			_projectService.Project.CognateIdentifiers[ComponentIdentifiers.PrimaryCognateIdentifier] = cognateIdentifier;
			return cognateIdentifier;
		}
	}
}
