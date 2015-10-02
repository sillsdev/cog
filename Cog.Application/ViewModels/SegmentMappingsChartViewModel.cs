using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Collections;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Application.ViewModels
{
	public class SegmentMappingsChartViewModel : ViewModelBase
	{
		public delegate SegmentMappingsChartViewModel Factory(IEnumerable<SegmentMappingViewModel> mappings, SoundType soundType, int threshold);

		private readonly ReadOnlyList<SegmentMappingsChartSegmentViewModel> _segments;
		private readonly ReadOnlyList<SegmentCategoryViewModel> _categories;
		private int _threshold;
		private readonly SoundType _soundType;

		public SegmentMappingsChartViewModel(IProjectService projectService, SegmentMappingsChartSegmentViewModel.Factory segmentFactory,
			IEnumerable<SegmentMappingViewModel> mappings, SoundType soundType, int threshold)
		{
			_threshold = threshold;

			_soundType = soundType;
			FeatureSymbol segmentType;
			switch (_soundType)
			{
				case SoundType.Consonant:
					segmentType = CogFeatureSystem.ConsonantType;
					break;
				case SoundType.Vowel:
					segmentType = CogFeatureSystem.VowelType;
					break;
				default:
					throw new InvalidEnumArgumentException();
			}

			var segmentComparer = new SegmentComparer();
			var categoryComparer = new SegmentCategoryComparer();
			_segments = new ReadOnlyList<SegmentMappingsChartSegmentViewModel>(projectService.Project.Varieties.SelectMany(v => v.SegmentFrequencyDistribution.ObservedSamples)
				.Where(s => s.Type == segmentType).Distinct().OrderBy(s => s.Category(), categoryComparer).ThenBy(s => s, segmentComparer)
				.Select(s => segmentFactory(s, _soundType)).Concat(segmentFactory(null, _soundType)).ToArray());
			_categories = new ReadOnlyList<SegmentCategoryViewModel>(_segments.GroupBy(s => s.DomainSegment == null ? string.Empty : s.DomainSegment.Category())
				.OrderBy(g => g.Key, categoryComparer).Select(g => new SegmentCategoryViewModel(g.Key, g)).ToArray());

			var mappingLookup = new Dictionary<UnorderedTuple<string, string>, HashSet<UnorderedTuple<string, string>>>();
			foreach (SegmentMappingViewModel mapping in mappings)
			{
				string seg1, seg2;
				FeatureSymbol leftEnv1, rightEnv1, leftEnv2, rightEnv2;
				if (ListSegmentMappings.Normalize(projectService.Project.Segmenter, mapping.Segment1, out seg1, out leftEnv1, out rightEnv1)
				    && ListSegmentMappings.Normalize(projectService.Project.Segmenter, mapping.Segment2, out seg2, out leftEnv2, out rightEnv2))
				{
					UnorderedTuple<string, string> key = UnorderedTuple.Create(seg1, seg2);
					HashSet<UnorderedTuple<string, string>> m = mappingLookup.GetValue(key, () => new HashSet<UnorderedTuple<string, string>>());
					m.Add(UnorderedTuple.Create(mapping.Segment1, mapping.Segment2));
				}
			}

			IWordAligner aligner = projectService.Project.WordAligners[ComponentIdentifiers.PrimaryWordAligner];
			foreach (SegmentMappingsChartSegmentPairViewModel segmentPair in _segments.SelectMany(s => s.SegmentPairs).Where(sp => sp.IsEnabled))
			{
				HashSet<UnorderedTuple<string, string>> pairMappings;
				if (mappingLookup.TryGetValue(UnorderedTuple.Create(segmentPair.StrRep1, segmentPair.StrRep2), out pairMappings))
					segmentPair.Mappings.UnionWith(pairMappings);
				segmentPair.Delta = segmentPair.DomainSegment1 == null || segmentPair.DomainSegment2 == null ? int.MaxValue
					: aligner.Delta(segmentPair.DomainSegment1.FeatureStruct, segmentPair.DomainSegment2.FeatureStruct);
				segmentPair.MeetsThreshold = segmentPair.Delta <= _threshold;
			}
		}

		public string Title
		{
			get
			{
				string typeStr;
				switch (_soundType)
				{
					case SoundType.Consonant:
						typeStr = "Consonants";
						break;
					case SoundType.Vowel:
						typeStr = "Vowels";
						break;
					default:
						throw new InvalidEnumArgumentException();
				}

				return string.Format("Edit Similar {0} Chart", typeStr);
			}
		}

		public ReadOnlyList<SegmentCategoryViewModel> Categories
		{
			get { return _categories; }
		}

		public ReadOnlyList<SegmentMappingsChartSegmentViewModel> Segments
		{
			get { return _segments; }
		}

		public int Threshold
		{
			get { return _threshold; }
			set
			{
				if (Set(() => Threshold, ref _threshold, value))
				{
					foreach (SegmentMappingsChartSegmentPairViewModel segmentPair in _segments.SelectMany(s => s.SegmentPairs).Where(sp => sp.IsEnabled))
						segmentPair.MeetsThreshold = segmentPair.Delta <= _threshold;
				}
			}
		}
	}
}
