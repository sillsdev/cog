using SIL.Cog.GraphAlgorithms;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	public class GlobalSegmentGridVertex : GridVertex
	{
		private readonly GlobalSegmentViewModel _globalSegment;

		public GlobalSegmentGridVertex(GlobalSegmentViewModel globalSegment)
		{
			_globalSegment = globalSegment;
		}

		public GlobalSegmentViewModel GlobalSegment
		{
			get { return _globalSegment; }
		}

		public override string ToString()
		{
			return _globalSegment.StrRep;
		}
	}
}
