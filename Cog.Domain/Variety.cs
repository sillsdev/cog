using SIL.Collections;
using SIL.Machine.FeatureModel;
using SIL.Machine.Statistics;

namespace SIL.Cog.Domain
{
	public class Variety : ObservableObject
	{
		private readonly WordCollection _words;
		private readonly VarietyVarietyPairCollection _varietyPairs;
		private readonly BulkObservableList<Affix> _affixes;
		private string _name;
		private readonly BulkObservableList<GeographicRegion> _regions;
		private readonly ObservableDictionary<FeatureSymbol, FrequencyDistribution<Segment>> _syllablePositionSegmentFreqDists;
		private FrequencyDistribution<Segment> _segmentFreqDist; 

		public Variety(string name)
		{
			_name = name;
			_words = new WordCollection(this);
			_varietyPairs = new VarietyVarietyPairCollection(this);
			_affixes = new BulkObservableList<Affix>();
			_regions = new BulkObservableList<GeographicRegion>();
			_syllablePositionSegmentFreqDists = new ObservableDictionary<FeatureSymbol, FrequencyDistribution<Segment>>();
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

		public ObservableDictionary<FeatureSymbol, FrequencyDistribution<Segment>> SyllablePositionSegmentFrequencyDistributions
		{
			get { return _syllablePositionSegmentFreqDists; }
		}

		public override string ToString()
		{
			return _name;
		}
	}
}
