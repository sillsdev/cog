using System.Collections.Concurrent;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain
{
	public class SegmentPool
	{
		private readonly ConcurrentDictionary<string, Segment> _segments;

		public SegmentPool()
		{
			_segments = new ConcurrentDictionary<string, Segment>();
			_segments["#"] = Segment.Anchor;
		}

		public Segment Get(ShapeNode node)
		{
			if (node == null)
				return null;

			return _segments.GetOrAdd(node.StrRep(), s =>
				{
					FeatureStruct fs = node.Annotation.FeatureStruct.DeepClone();
					fs.RemoveValue(CogFeatureSystem.OriginalStrRep);
					fs.RemoveValue(CogFeatureSystem.SyllablePosition);
					fs.Freeze();
					return new Segment(fs);
				});
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

		public void Reset()
		{
			_segments.Clear();
			_segments["#"] = Segment.Anchor;
		}
	}
}
