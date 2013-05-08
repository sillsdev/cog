using System.Collections.ObjectModel;
using SIL.Cog.Statistics;
using SIL.Collections;

namespace SIL.Cog
{
	public class Variety : NotifyPropertyChangedBase
	{
		private readonly WordCollection _words;
		private readonly SegmentCollection _segments;
		private readonly VarietyVarietyPairCollection _varietyPairs;
		private readonly BulkObservableCollection<Affix> _affixes;
		private string _name;
		private readonly ObservableCollection<GeographicRegion> _regions;
		private readonly FrequencyDistribution<Segment> _segmentFreqDist;
		private readonly MaxLikelihoodProbabilityDistribution<Segment> _segmentProbDist; 

		public Variety(string name)
		{
			_name = name;
			_words = new WordCollection(this);
			_segments = new SegmentCollection(this);
			_varietyPairs = new VarietyVarietyPairCollection(this);
			_affixes = new BulkObservableCollection<Affix>();
			_regions = new ObservableCollection<GeographicRegion>();
			_segmentFreqDist = new FrequencyDistribution<Segment>();
			_segmentProbDist = new MaxLikelihoodProbabilityDistribution<Segment>(_segmentFreqDist);
		}

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				OnPropertyChanged("Name");
			}
		}

		public SegmentCollection Segments
		{
			get { return _segments; }
		}

		public WordCollection Words
		{
			get { return _words; }
		}

		public VarietyVarietyPairCollection VarietyPairs
		{
			get { return _varietyPairs; }
		}

		public BulkObservableCollection<Affix> Affixes
		{
			get { return _affixes; }
		}

		public ObservableCollection<GeographicRegion> Regions
		{
			get { return _regions; }
		}

		public FrequencyDistribution<Segment> SegmentFrequencyDistribution
		{
			get { return _segmentFreqDist; }
		}

		public IProbabilityDistribution<Segment> SegmentProbabilityDistribution
		{
			get { return _segmentProbDist; }
		}

		public override string ToString()
		{
			return _name;
		}
	}
}
