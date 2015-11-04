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
		private SegmentMappingsChartSegmentPairViewModel _selectedSegmentPair;
		private bool _segmentPairSelected;

		public SegmentMappingsChartViewModel(IProjectService projectService, SegmentMappingsChartSegmentPairViewModel.Factory segmentPairFactory, SegmentMappingViewModel.Factory mappingFactory,
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
				.Select(s => new SegmentMappingsChartSegmentViewModel(s, _soundType)).Concat(new SegmentMappingsChartSegmentViewModel(null, _soundType)).ToArray());
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
			foreach (SegmentMappingsChartSegmentViewModel segment1 in _segments)
			{
				bool isEnabled = true;
				foreach (SegmentMappingsChartSegmentViewModel segment2 in _segments)
				{
					if (EqualityComparer<Segment>.Default.Equals(segment1.DomainSegment, segment2.DomainSegment))
						isEnabled = false;

					int delta = segment1.DomainSegment == null || segment2.DomainSegment == null ? -1
						: aligner.Delta(segment1.DomainSegment.FeatureStruct, segment2.DomainSegment.FeatureStruct);
					SegmentMappingsChartSegmentPairViewModel segmentPair = segmentPairFactory(segment1, segment2, delta, isEnabled);
					segmentPair.MeetsThreshold = delta != -1 && delta <= _threshold;
					HashSet<UnorderedTuple<string, string>> pairMappings;
					if (mappingLookup.TryGetValue(UnorderedTuple.Create(segment1.StrRep, segment2.StrRep), out pairMappings))
						segmentPair.Mappings.Mappings.AddRange(pairMappings.Select(m => mappingFactory(m.Item1, m.Item2)));
					segment1.SegmentPairs.Add(segmentPair);
				}
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

		public SegmentMappingsChartSegmentPairViewModel SelectedSegmentPair
		{
			get { return _selectedSegmentPair; }
			set
			{
				if (Set(() => SelectedSegmentPair, ref _selectedSegmentPair, value))
					IsSegmentPairSelected = _selectedSegmentPair != null;
			}
		}

		public bool IsSegmentPairSelected
		{
			get { return _segmentPairSelected; }
			set { Set(() => IsSegmentPairSelected, ref _segmentPairSelected, value); }
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
						segmentPair.MeetsThreshold = segmentPair.Delta != -1 && segmentPair.Delta <= _threshold;
				}
			}
		}
	}
}
