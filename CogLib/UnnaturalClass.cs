using System.Collections.Generic;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog
{
	public class UnnaturalClass : SoundClass
	{
		private readonly Segmenter _segmenter;
		private readonly List<string> _segments; 
		private readonly HashSet<string> _normalizedSegments;
		private readonly bool _ignoreModifiers;

		public UnnaturalClass(Segmenter segmenter, string name, IEnumerable<string> segments, bool ignoreModifiers)
			: base(name)
		{
			_segmenter = segmenter;
			_segments = segments.ToList();
			_ignoreModifiers = ignoreModifiers;
			_normalizedSegments = new HashSet<string>();
			foreach (string segment in _segments)
			{
				if (segment == "#")
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
			Shape shape;
			if (_segmenter.ToShape(segment, out shape) && shape.Count == 1)
			{
				normalizedSegment = shape.First.StrRep();
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

		public override bool Matches(Segment left, Ngram target, Segment right)
		{
			string strRep = target.ToString();
			if (_ignoreModifiers)
				strRep = StripModifiers(strRep);

			if (left != null && left.Type == CogFeatureSystem.AnchorType && _normalizedSegments.Contains(string.Format("#{0}", strRep)))
				return true;

			if (right != null && right.Type == CogFeatureSystem.AnchorType && _normalizedSegments.Contains(string.Format("{0}#", strRep)))
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
