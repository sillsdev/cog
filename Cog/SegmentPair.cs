namespace SIL.Cog
{
	public class SegmentPair
	{
		private readonly string _u;
		private readonly string _v;
		private readonly int _linkCount;
		private readonly double _corrProb;

		public SegmentPair(string u, string v, int linkCount, double corrProb)
		{
			_u = u;
			_v = v;
			_linkCount = linkCount;
			_corrProb = corrProb;
		}

		public string U
		{
			get { return _u; }
		}

		public string V
		{
			get { return _v; }
		}

		public int LinkCount
		{
			get { return _linkCount; }
		}

		public double CorrespondenceProbability
		{
			get { return _corrProb; }
		}
	}
}
