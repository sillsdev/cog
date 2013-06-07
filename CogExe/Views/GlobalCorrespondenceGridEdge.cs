using GraphSharp;
using SIL.Cog.GraphAlgorithms;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	public class GlobalCorrespondenceGridEdge : WeightedEdge<GridVertex>
	{
		private readonly GlobalCorrespondenceViewModel _globalCorrespondence;

		public GlobalCorrespondenceGridEdge(GlobalSegmentGridVertex source, GlobalSegmentGridVertex target, GlobalCorrespondenceViewModel globalCorrespondence)
			: base(source, target, globalCorrespondence.Frequency)
		{
			_globalCorrespondence = globalCorrespondence;
		}

		public GlobalCorrespondenceViewModel GlobalCorrespondence
		{
			get { return _globalCorrespondence; }
		}
	}
}
