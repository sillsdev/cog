using System;
using System.Linq;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Applications.ViewModels
{
	public class BlairCognateIdentifierViewModel : ComponentSettingsViewModelBase
	{
		private readonly IProjectService _projectService;

		private bool _ignoreRegularInsertionDeletion;
		private bool _regularConsEqual;
		private readonly SimilarSegmentMappingsOptionsViewModel _similarSegments;
		private readonly SegmentMappingsViewModel _ignoredMappings;

		public BlairCognateIdentifierViewModel(IProjectService projectService, SegmentMappingsViewModel ignoredMappings, SimilarSegmentMappingsOptionsViewModel similarSegments)
			: base("Blair")
		{
			_projectService = projectService;
			_ignoredMappings = ignoredMappings;
			_ignoredMappings.PropertyChanged += ChildPropertyChanged;
			_similarSegments = similarSegments;
			_similarSegments.PropertyChanged += ChildPropertyChanged;
		}

		public override void Setup()
		{
			_ignoredMappings.CurrentMapping = null;
			_ignoredMappings.Mappings.Clear();

			IProcessor<VarietyPair> cognateIdentifier = _projectService.Project.VarietyPairProcessors["cognateIdentifier"];
			var blair = cognateIdentifier as BlairCognateIdentifier;
			if (blair == null)
			{
				Set(() => IgnoreRegularInsertionDeletion, ref _ignoreRegularInsertionDeletion, false);
				Set(() => RegularConsonantsEqual, ref _regularConsEqual, false);
				_similarSegments.SegmentMappings = null;
			}
			else
			{
				Set(() => IgnoreRegularInsertionDeletion, ref _ignoreRegularInsertionDeletion, blair.IgnoreRegularInsertionDeletion);
				Set(() => RegularConsonantsEqual, ref _regularConsEqual, blair.RegularConsonantEqual);
				var ignoredMappings = (ListSegmentMappings) blair.IgnoredMappings;
				foreach (Tuple<string, string> mapping in ignoredMappings.Mappings)
					_ignoredMappings.Mappings.Add(new SegmentMappingViewModel(_projectService.Project.Segmenter, mapping.Item1, mapping.Item2));
				_similarSegments.SegmentMappings = (TypeSegmentMappings) blair.SimilarSegments;
			}
			_similarSegments.Setup();
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			_ignoredMappings.AcceptChanges();
			_similarSegments.AcceptChanges();
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

		public ComponentOptionsViewModel SimilarSegments
		{
			get { return _similarSegments; }
		}

		public override object UpdateComponent()
		{
			var cognateIdentifier = new BlairCognateIdentifier(_projectService.Project, _ignoreRegularInsertionDeletion, _regularConsEqual,
				"primary", new ListSegmentMappings(_projectService.Project.Segmenter, _ignoredMappings.Mappings.Select(m => Tuple.Create(m.Segment1, m.Segment2)), false),
				(ISegmentMappings) _similarSegments.UpdateComponent());
			_projectService.Project.VarietyPairProcessors["cognateIdentifier"] = cognateIdentifier;
			return cognateIdentifier;
		}
	}
}
