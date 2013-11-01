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
		private enum Boundary
		{
			WordInitial,
			WordFinal,
			None
		}

		private readonly List<Tuple<string, string>> _mappings;
		private readonly bool _implicitComplexSegments;
		private readonly Segmenter _segmenter;

		private readonly Dictionary<string, HashSet<string>> _mappingLookup;

		public ListSegmentMappings(Segmenter segmenter, IEnumerable<Tuple<string, string>> mappings, bool implicitComplexSegments)
		{
			_segmenter = segmenter;
			_mappings = mappings.ToList();
			_implicitComplexSegments = implicitComplexSegments;

			_mappingLookup = new Dictionary<string, HashSet<string>>();
			foreach (Tuple<string, string> mapping in _mappings)
			{
				string str1, str2;
				if (Normalize(mapping.Item1, out str1) && Normalize(mapping.Item2, out str2))
				{
					HashSet<string> segments = _mappingLookup.GetValue(str1, () => new HashSet<string>());
					segments.Add(str2);
					segments = _mappingLookup.GetValue(str2, () => new HashSet<string>());
					segments.Add(str1);
				}
			}
		}

		private bool Normalize(string segment, out string normalizedSegment)
		{
			normalizedSegment = null;
			if (segment == "#")
				return false;

			Boundary bdry;
			string strRep = StripBoundary(segment, out bdry);
			string normalized;
			if (_segmenter.NormalizeSegmentString(strRep, out normalized))
			{
				normalizedSegment = AddBoundary(normalized, bdry);
				return true;
			}
			return false;
		}

		private string StripBoundary(string strRep, out Boundary bdry)
		{
			if (strRep.StartsWith("#"))
			{
				bdry = Boundary.WordInitial;
				return strRep.Remove(0, 1);
			}
			if (strRep.EndsWith("#"))
			{
				bdry = Boundary.WordFinal;
				return strRep.Remove(strRep.Length - 1, 1);
			}

			bdry = Boundary.None;
			return strRep;
		}

		private string AddBoundary(string strRep, Boundary bdry)
		{
			switch (bdry)
			{
				case Boundary.WordInitial:
					return string.Format("#{0}", strRep);
				case Boundary.WordFinal:
					return string.Format("{0}#", strRep);
			}
			return strRep;
		}

		public IEnumerable<Tuple<string, string>> Mappings
		{
			get { return _mappings; }
		}

		public bool ImplicitComplexSegments
		{
			get { return _implicitComplexSegments; }
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
			IList<string> strReps;
			if (target.Count == 0)
			{
				strReps = new[] {"-", "_"};
			}
			else
			{
				strReps = new List<string>();
				foreach (Segment seg in target)
				{
					strReps.Add(seg.StrRep);
					if (_implicitComplexSegments && seg.IsComplex)
					{
						Shape shape = _segmenter.Segment(seg.StrRep);
						foreach (ShapeNode node in shape)
							strReps.Add(node.StrRep());
					}
				}	
			}

			foreach (string strRep in strReps)
			{
				if (leftNode != null && leftNode.Type() == CogFeatureSystem.AnchorType)
					yield return string.Format("#{0}", strRep);
				if (rightNode != null && rightNode.Type() == CogFeatureSystem.AnchorType)
					yield return string.Format("{0}#", strRep);
				yield return strRep;
			}
		}

	}
}
