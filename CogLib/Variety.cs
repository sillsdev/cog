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

		public Variety(string name)
		{
			_name = name;
			_words = new WordCollection(this);
			_segments = new SegmentCollection();
			_varietyPairs = new VarietyVarietyPairCollection(this);
			_affixes = new BulkObservableCollection<Affix>();
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

		public override string ToString()
		{
			return _name;
		}
	}
}
