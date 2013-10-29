using SIL.Machine;
using SIL.Machine.NgramModeling;

namespace SIL.Cog.Domain.Components
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

		public bool IsMapped(ShapeNode leftNode1, Ngram<Segment> target1, ShapeNode rightNode1, ShapeNode leftNode2, Ngram<Segment> target2, ShapeNode rightNode2)
		{
			if (target1.Count == 0 || target2.Count == 0)
				return false;

			IWordAligner aligner = _project.WordAligners[_alignerID];

			foreach (Segment seg1 in target1)
			{
				foreach (Segment seg2 in target2)
				{
					if (aligner.Delta(seg1.FeatureStruct, seg2.FeatureStruct) <= _threshold)
						return true;
				}
			}

			return false;
		}
	}
}
