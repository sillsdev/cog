using SIL.Collections;

namespace SIL.Cog.Domain
{
	public class GlobalSoundCorrespondence : ObservableObject
	{
		private readonly Segment _segment1;
		private readonly Segment _segment2;
		private int _frequency;
		private readonly BulkObservableList<WordPair> _wordPairs; 

		public GlobalSoundCorrespondence(Segment segment1, Segment segment2)
		{
			_segment1 = segment1;
			_segment2 = segment2;
			_wordPairs = new BulkObservableList<WordPair>();
		}

		public Segment Segment1
		{
			get { return _segment1; }
		}

		public Segment Segment2
		{
			get { return _segment2; }
		}

		public int Frequency
		{
			get { return _frequency; }
			set { Set(() => Frequency, ref _frequency, value); }
		}

		public BulkObservableList<WordPair> WordPairs
		{
			get { return _wordPairs; }
		}
	}
}
