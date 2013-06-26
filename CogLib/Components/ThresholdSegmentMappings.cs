namespace SIL.Cog.Components
{
	public class ThresholdSegmentMappings : ISegmentMappings
	{
		private readonly CogProject _project;
		private readonly string _alignerID;
		private readonly int _threshold;

		public ThresholdSegmentMappings(CogProject project, int threshold, string alignerID)
		{
			_project = project;
			_threshold = threshold;
			_alignerID = alignerID;
		}

		public int Threshold
		{
			get { return _threshold; }
		}

		public string AlignerID
		{
			get { return _alignerID; }
		}

		public bool IsMapped(Segment seg1, Segment seg2)
		{
			IWordAligner aligner = _project.WordAligners[_alignerID];
			return aligner.Delta(seg1.FeatureStruct, seg2.FeatureStruct) <= _threshold;
		}
	}
}
