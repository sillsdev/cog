using SIL.Machine.FeatureModel;
using SIL.Machine.Statistics;
using SIL.ObjectModel;

namespace SIL.Cog.Domain
{
	public class Variety : ObservableObject
	{
		private string _name;
		private FrequencyDistribution<Segment> _segmentFreqDist; 

		public Variety(string name)
		{
			_name = name;
			Words = new WordCollection(this);
			VarietyPairs = new VarietyVarietyPairCollection(this);
			Affixes = new BulkObservableList<Affix>();
			Regions = new BulkObservableList<GeographicRegion>();
			SyllablePositionSegmentFrequencyDistributions = new ObservableDictionary<FeatureSymbol, FrequencyDistribution<Segment>>();
		}

		public string Name
		{
			get { return _name; }
			set
			{
				Collection?.ChangeVarietyName(this, value);
				Set(() => Name, ref _name, value);
			}
		}

		public WordCollection Words { get; }
		public VarietyVarietyPairCollection VarietyPairs { get; }
		public BulkObservableList<Affix> Affixes { get; }
		public BulkObservableList<GeographicRegion> Regions { get; }

		public FrequencyDistribution<Segment> SegmentFrequencyDistribution
		{
			get { return _segmentFreqDist; }
			set { Set(() => SegmentFrequencyDistribution, ref _segmentFreqDist, value); }
		}

		public ObservableDictionary<FeatureSymbol, FrequencyDistribution<Segment>> SyllablePositionSegmentFrequencyDistributions { get; }

		internal VarietyCollection Collection { get; set; }

		public override string ToString()
		{
			return _name;
		}
	}
}
