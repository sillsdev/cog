using System.Collections.Generic;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain
{
	public class SegmentPool
	{
		private readonly Dictionary<string, Segment> _segments;

		public SegmentPool()
		{
			_segments = new Dictionary<string, Segment>();
			_segments["#"] = Segment.Anchor;
		}

		public Segment Get(ShapeNode node)
		{
			if (node == null)
				return null;

			Segment segment;
			if (!_segments.TryGetValue(node.StrRep(), out segment))
			{
				FeatureStruct fs = node.Annotation.FeatureStruct.DeepClone();
				fs.RemoveValue(CogFeatureSystem.OriginalStrRep);
				fs.Freeze();
				segment = new Segment(fs);
				_segments[node.StrRep()] = segment;
			}
			return segment;
		}

		public Segment GetExisting(ShapeNode node)
		{
			return GetExisting(node.StrRep());
		}

		public Segment GetExisting(string strRep)
		{
			return _segments[strRep];
		}

		public bool TryGetExisting(string strRep, out Segment segment)
		{
			return _segments.TryGetValue(strRep, out segment);
		}
	}
}
