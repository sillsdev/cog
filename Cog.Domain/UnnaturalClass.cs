using System.Collections.Generic;
using System.Linq;
using SIL.Machine;
using SIL.Machine.NgramModeling;

namespace SIL.Cog.Domain
{
	public class UnnaturalClass : SoundClass
	{
		private readonly Segmenter _segmenter;
		private readonly List<string> _segments; 
		private readonly HashSet<string> _normalizedSegments;
		private readonly bool _ignoreModifiers;

		public UnnaturalClass(string name, IEnumerable<string> segments, bool ignoreModifiers, Segmenter segmenter)
			: base(name)
		{
			_segmenter = segmenter;
			_segments = segments.ToList();
			_ignoreModifiers = ignoreModifiers;
			_normalizedSegments = new HashSet<string>();
			foreach (string segment in _segments)
			{
				if (segment == "#" || segment == "-")
				{
					_normalizedSegments.Add(segment);
				}
				else if (segment.StartsWith("#"))
				{
					string normalized;
					if (Normalize(segment.Remove(0, 1), out normalized))
						_normalizedSegments.Add("#" + normalized);
				}
				else if (segment.EndsWith("#"))
				{
					string normalized;
					if (Normalize(segment.Remove(segment.Length - 1, 1), out normalized))
						_normalizedSegments.Add(normalized + "#");
				}
				else
				{
					string normalized;
					if (Normalize(segment, out normalized))
						_normalizedSegments.Add(normalized);
				}
			}
		}

		private bool Normalize(string segment, out string normalizedSegment)
		{
			if (_segmenter.NormalizeSegmentString(segment, out normalizedSegment))
			{
				if (_ignoreModifiers)
					normalizedSegment = StripModifiers(normalizedSegment);
				return true;
			}
			normalizedSegment = null;
			return false;
		}

		public IEnumerable<string> Segments
		{
			get { return _segments; }
		}

		public bool IgnoreModifiers
		{
			get { return _ignoreModifiers; }
		}

		public override bool Matches(ShapeNode leftNode, Ngram<Segment> target, ShapeNode rightNode)
		{
			string strRep = target.ToString();
			if (_ignoreModifiers)
				strRep = StripModifiers(strRep);

			if (leftNode != null && leftNode.Type() == CogFeatureSystem.AnchorType && _normalizedSegments.Contains(string.Format("#{0}", strRep)))
				return true;

			if (rightNode != null && rightNode.Type() == CogFeatureSystem.AnchorType && _normalizedSegments.Contains(string.Format("{0}#", strRep)))
				return true;

			return _normalizedSegments.Contains(strRep);
		}

		private string StripModifiers(string str)
		{
			foreach (Symbol mod in _segmenter.Modifiers)
				str = str.Replace(mod.StrRep, "");
			return str;
		}
	}
}
