using System.Collections.Generic;
using GalaSoft.MvvmLight;
using GraphSharp;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.ViewModels
{
	public class GlobalCorrespondenceEdge : ViewModelBase, IWeightedEdge<GridVertex>
	{
		private readonly GlobalSegmentVertex _segment1;
		private readonly GlobalSegmentVertex _segment2;
		private bool _isSelected;
		private readonly List<WordPair> _wordPairs; 

		public GlobalCorrespondenceEdge(GlobalSegmentVertex segment1, GlobalSegmentVertex segment2)
		{
			_segment1 = segment1;
			_segment2 = segment2;
			_wordPairs = new List<WordPair>();
		}

		public GridVertex Source
		{
			get { return _segment1; }
		}

		public GridVertex Target
		{
			get { return _segment2; }
		}

		public double Weight
		{
			get { return Frequency; }
		}

		public int Frequency { get; internal set; }

		public double NormalizedFrequency { get; internal set; }

		public bool IsSelected
		{
			get { return _isSelected; }
			set { Set(() => IsSelected, ref _isSelected, value); }
		}

		internal IList<WordPair> DomainWordPairs
		{
			get { return _wordPairs; }
		}
	}
}
