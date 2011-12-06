namespace SIL.Cog
{
	public class SegmentPair
	{
		private readonly string _u;
		private readonly string _v;
		private readonly int _coocurCount;

		public SegmentPair(string u, string v, int cooccurCount)
		{
			_u = u;
			_v = v;
			_coocurCount = cooccurCount;
		}

		public string U
		{
			get { return _u; }
		}

		public string V
		{
			get { return _v; }
		}

		public int CooccurCount
		{
			get { return _coocurCount; }
		}

		public double Score { get; set; }
		public int LinkCount { get; set; }
		public double CorrespondenceProbability { get; set; }
	}
}
