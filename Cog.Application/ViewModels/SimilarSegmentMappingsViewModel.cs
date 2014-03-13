using System;
using System.Linq;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Application.ViewModels
{
	public class SimilarSegmentMappingsViewModel : ChangeTrackingViewModelBase
	{
		public delegate SimilarSegmentMappingsViewModel Factory(SoundType soundType);

		private readonly IProjectService _projectService;
		private readonly SegmentMappingsViewModel _mappings;
		private readonly SoundType _soundType;
		private int _threshold;
		private bool _implicitComplexSegments;
		
		public SimilarSegmentMappingsViewModel(IProjectService projectService, SegmentMappingsViewModel mappings, SoundType soundType)
		{
			_projectService = projectService;
			_mappings = mappings;
			_mappings.PropertyChanged += ChildPropertyChanged;
			_soundType = soundType;
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			_mappings.AcceptChanges();
		}

		public int Threshold
		{
			get { return _threshold; }
			set { SetChanged(() => Threshold, ref _threshold, value); }
		}

		public SegmentMappingsViewModel Mappings
		{
			get { return _mappings; }
		}

		public bool ImplicitComplexSegments
		{
			get { return _implicitComplexSegments; }
			set { SetChanged(() => ImplicitComplexSegments, ref _implicitComplexSegments, value); }
		}

		public SoundType SoundType
		{
			get { return _soundType; }
		}

		public UnionSegmentMappings SegmentMappings { get; set; }

		public void Setup()
		{
			_mappings.SelectedMapping = null;
			_mappings.Mappings.Clear();
			if (SegmentMappings == null)
			{
				Set(() => Threshold, ref _threshold, _soundType == SoundType.Vowel ? 500 : 600);
				Set(() => ImplicitComplexSegments, ref _implicitComplexSegments, false);
			}
			else
			{
				Set(() => Threshold, ref _threshold, ((ThresholdSegmentMappings) SegmentMappings.SegmentMappingsComponents[0]).Threshold);

				var listSegmentMappings = (ListSegmentMappings) SegmentMappings.SegmentMappingsComponents[1];
				foreach (Tuple<string, string> mapping in listSegmentMappings.Mappings)
					_mappings.Mappings.Add(new SegmentMappingViewModel(_projectService.Project.Segmenter, mapping.Item1, mapping.Item2));
				Set(() => ImplicitComplexSegments, ref _implicitComplexSegments, listSegmentMappings.ImplicitComplexSegments);
			}
		}

		public void UpdateComponent()
		{
			var thresholdSegmentMappings = new ThresholdSegmentMappings(_projectService.Project, _threshold, ComponentIdentifiers.PrimaryWordAligner);
			var listSegmentMappings = new ListSegmentMappings(_projectService.Project.Segmenter, _mappings.Mappings.Select(m => Tuple.Create(m.Segment1, m.Segment2)), _implicitComplexSegments);
			SegmentMappings = new UnionSegmentMappings(new ISegmentMappings[] {thresholdSegmentMappings, listSegmentMappings});
		}
	}
}
