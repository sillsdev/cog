using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class GlobalCorrespondenceViewModel : ViewModelBase
	{
		private readonly GlobalSegmentViewModel _segment1;
		private readonly GlobalSegmentViewModel _segment2;
		private readonly int _freq;
		private readonly double _normalizedFreq;
		private bool _isSelected;
		private readonly ReadOnlyList<WordPairViewModel> _wordPairs; 

		public GlobalCorrespondenceViewModel(GlobalSegmentViewModel segment1, GlobalSegmentViewModel segment2, int freq, double normalizedFreq, IEnumerable<WordPairViewModel> wordPairs)
		{
			_segment1 = segment1;
			_segment2 = segment2;
			_freq = freq;
			_normalizedFreq = normalizedFreq;
			_wordPairs = new ReadOnlyList<WordPairViewModel>(wordPairs.ToArray());
		}

		public GlobalSegmentViewModel Segment1
		{
			get { return _segment1; }
		}

		public GlobalSegmentViewModel Segment2
		{
			get { return _segment2; }
		}

		public int Frequency
		{
			get { return _freq; }
		}

		public double NormalizedFrequency
		{
			get { return _normalizedFreq; }
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set { Set(() => IsSelected, ref _isSelected, value); }
		}

		public ReadOnlyList<WordPairViewModel> WordPairs
		{
			get { return _wordPairs; }
		}
	}
}
