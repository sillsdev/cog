using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.NgramModeling;

namespace SIL.Cog.Domain.Components
{
	public class ListSegmentMappings : ISegmentMappings
	{
		private readonly List<Tuple<string, string>> _mappings;

		private readonly Dictionary<string, HashSet<string>> _mappingLookup;

		public ListSegmentMappings(Segmenter segmenter, IEnumerable<Tuple<string, string>> mappings)
		{
			_mappings = mappings.ToList();

			_mappingLookup = new Dictionary<string, HashSet<string>>();
			foreach (Tuple<string, string> mapping in _mappings)
			{
				string str1, str2;
				if (Normalize(segmenter, mapping.Item1, out str1) && Normalize(segmenter, mapping.Item2, out str2))
				{
					HashSet<string> segments = _mappingLookup.GetValue(str1, () => new HashSet<string>());
					segments.Add(str2);
					segments = _mappingLookup.GetValue(str2, () => new HashSet<string>());
					segments.Add(str1);
				}
			}
		}

		private bool Normalize(Segmenter segmenter, string segment, out string normalizedSegment)
		{
			normalizedSegment = null;
			if (segment == "#")
			{
				// ignore
			}
			else if (segment.StartsWith("#"))
			{
				string normalized;
				if (segmenter.NormalizeSegmentString(segment.Remove(0, 1), out normalized))
					normalizedSegment = "#" + normalized;
			}
			else if (segment.EndsWith("#"))
			{
				string normalized;
				if (segmenter.NormalizeSegmentString(segment.Remove(segment.Length - 1, 1), out normalized))
					normalizedSegment = normalized + "#";
			}
			else
			{
				string normalized;
				if (segmenter.NormalizeSegmentString(segment, out normalized))
					normalizedSegment = normalized;
			}
			return normalizedSegment != null;
		}

		public IEnumerable<Tuple<string, string>> Mappings
		{
			get { return _mappings; }
		}

		public bool IsMapped(ShapeNode leftNode1, Ngram<Segment> target1, ShapeNode rightNode1, ShapeNode leftNode2, Ngram<Segment> target2, ShapeNode rightNode2)
		{
			foreach (string strRep1 in GetStrReps(leftNode1, target1, rightNode1))
			{
				foreach (string strRep2 in GetStrReps(rightNode1, target2, rightNode2))
				{
					HashSet<string> segments;
					if (_mappingLookup.TryGetValue(strRep1, out segments) && segments.Contains(strRep2))
						return true;
				}
			}
			return false;
		}

		private IEnumerable<string> GetStrReps(ShapeNode leftNode, Ngram<Segment> target, ShapeNode rightNode)
		{
			if (target.Count == 0)
			{
				if (leftNode != null && leftNode.Type() == CogFeatureSystem.AnchorType)
					yield return "#-";
				if (rightNode != null && rightNode.Type() == CogFeatureSystem.AnchorType)
					yield return "-#";
				yield return "-";
			}
			else
			{
				foreach (Segment seg in target)
				{
					string strRep = seg.ToString();
					if (leftNode != null && leftNode.Type() == CogFeatureSystem.AnchorType)
						yield return string.Format("#{0}", strRep);
					if (rightNode != null && rightNode.Type() == CogFeatureSystem.AnchorType)
						yield return string.Format("{0}#", strRep);
					yield return strRep;
				}
			}
		}
	}
}
