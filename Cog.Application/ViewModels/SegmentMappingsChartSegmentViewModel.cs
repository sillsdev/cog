using System.Collections.Generic;
using System.Linq;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Application.ViewModels
{
	public class SegmentMappingsChartSegmentViewModel : SegmentViewModel
	{
		private readonly ReadOnlyList<SegmentMappingsChartSegmentPairViewModel> _segmentPairs;

		public delegate SegmentMappingsChartSegmentViewModel Factory(Segment segment, SoundType type);

		public SegmentMappingsChartSegmentViewModel(IProjectService projectService, Segment segment, SoundType type)
			: base(segment, type)
		{
			FeatureSymbol symbol = type == SoundType.Consonant ? CogFeatureSystem.ConsonantType : CogFeatureSystem.VowelType;
			var segmentComparer = new SegmentComparer();
			var categoryComparer = new SegmentCategoryComparer();
			bool isEnabled = true;
			var segmentPairs = new List<SegmentMappingsChartSegmentPairViewModel>();
			foreach (Segment s in projectService.Project.Varieties.SelectMany(v => v.SegmentFrequencyDistribution.ObservedSamples).Where(s => s.Type == symbol)
				.Distinct().OrderBy(s => s.Category(), categoryComparer).ThenBy(s => s, segmentComparer).Concat((Segment) null))
			{
				if (EqualityComparer<Segment>.Default.Equals(segment, s))
					isEnabled = false;
				segmentPairs.Add(new SegmentMappingsChartSegmentPairViewModel(segment, s, isEnabled));
			}
			_segmentPairs = new ReadOnlyList<SegmentMappingsChartSegmentPairViewModel>(segmentPairs);
		}

		public ReadOnlyList<SegmentMappingsChartSegmentPairViewModel> SegmentPairs
		{
			get { return _segmentPairs; }
		}
	}
}
