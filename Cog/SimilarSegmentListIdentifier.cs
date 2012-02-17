using System.Collections.Generic;
using System.IO;
using SIL.Machine;

namespace SIL.Cog
{
	public class SimilarSegmentListIdentifier : IProcessor<VarietyPair>
	{
		private readonly Dictionary<string, HashSet<string>> _similarSegments;

		public SimilarSegmentListIdentifier(string path)
		{
			_similarSegments = new Dictionary<string, HashSet<string>>();
			using (var reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					line = line.Trim();
					if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
						continue;

					int index = line.IndexOf('\t');
					string seg1 = line.Substring(0, index);
					string seg2 = line.Substring(index + 1);
					UpdateSimilarSegments(seg1, seg2);
					UpdateSimilarSegments(seg2, seg1);
				}
			}


			foreach (string seg1 in _similarSegments.Keys)
			{
				foreach (string seg2 in _similarSegments.Keys)
				{
					if (seg1 == seg2)
						continue;

					string seg = seg1 + seg2;
					HashSet<string> segments;
					if (!_similarSegments.TryGetValue(seg, out segments))
					{
						segments = new HashSet<string>(_similarSegments[seg1]);
						segments.UnionWith(_similarSegments[seg2]);
						segments.Add(seg1);
						segments.Add(seg2);
						_similarSegments[seg] = segments;
						foreach (string otherSeg in segments)
							UpdateSimilarSegments(otherSeg, seg);
					}
				}
			}
		}

		private void UpdateSimilarSegments(string seg1, string seg2)
		{
			HashSet<string> segments = _similarSegments.GetValue(seg1, () => new HashSet<string>());
			segments.Add(seg2);
		}

		public void Process(VarietyPair varietyPair)
		{
			foreach (Segment seg1 in varietyPair.Variety1.Segments)
			{
				foreach (Segment seg2 in varietyPair.Variety2.Segments)
				{
					if (seg1.StrRep == seg2.StrRep)
						continue;

					HashSet<string> segments;
					if (_similarSegments.TryGetValue(seg1.StrRep, out segments))
					{
						if (segments.Contains(seg2.StrRep))
							varietyPair.AddSimilarSegment(seg1, seg2);
					}
				}
			}
		}
	}
}
