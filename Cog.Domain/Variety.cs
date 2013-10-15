using SIL.Collections;
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
		private readonly ObservableDictionary<SyllablePosition, FrequencyDistribution<Segment>> _segmentFreqDists;

		public Variety(string name)
		{
			_name = name;
			_words = new WordCollection(this);
			_varietyPairs = new VarietyVarietyPairCollection(this);
			_affixes = new BulkObservableList<Affix>();
			_regions = new BulkObservableList<GeographicRegion>();
			_segmentFreqDists = new ObservableDictionary<SyllablePosition, FrequencyDistribution<Segment>>();
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

		public ObservableDictionary<SyllablePosition, FrequencyDistribution<Segment>> SegmentFrequencyDistributions
		{
			get { return _segmentFreqDists; }
		}

		public override string ToString()
		{
			return _name;
		}
	}
}
