using SIL.Cog.Domain.Statistics;
using SIL.Collections;

namespace SIL.Cog.Domain
{
	public class Variety : ObservableObject
	{
		private readonly WordCollection _words;
		private readonly VarietyVarietyPairCollection _varietyPairs;
		private readonly BulkObservableList<Affix> _affixes;
		private string _name;
		private readonly BulkObservableList<GeographicRegion> _regions;
		private FrequencyDistribution<Segment> _segmentFreqDist;
		private IProbabilityDistribution<Segment> _segmentProbDist;
		private readonly SegmentPool _segmentPool;

		public Variety(string name)
		{
			_name = name;
			_words = new WordCollection(this);
			_varietyPairs = new VarietyVarietyPairCollection(this);
			_affixes = new BulkObservableList<Affix>();
			_regions = new BulkObservableList<GeographicRegion>();
			_segmentPool = new SegmentPool();
		}

		public string Name
		{
			get { return _name; }
			set { Set(() => Name, ref _name, value); }
		}

		public WordCollection Words
		{
			get { return _words; }
		}

		public VarietyVarietyPairCollection VarietyPairs
		{
			get { return _varietyPairs; }
		}

		public BulkObservableList<Affix> Affixes
		{
			get { return _affixes; }
		}

		public BulkObservableList<GeographicRegion> Regions
		{
			get { return _regions; }
		}

		public FrequencyDistribution<Segment> SegmentFrequencyDistribution
		{
			get { return _segmentFreqDist; }
			set { Set(() => SegmentFrequencyDistribution, ref _segmentFreqDist, value); }
		}

		public IProbabilityDistribution<Segment> SegmentProbabilityDistribution
		{
			get { return _segmentProbDist; }
			set { Set(() => SegmentProbabilityDistribution, ref _segmentProbDist, value); }
		}

		public SegmentPool SegmentPool
		{
			get { return _segmentPool; }
		}

		public override string ToString()
		{
			return _name;
		}
	}
}
