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
				Shape shape;
				if (_segmenter.ToShape(segment, out shape) && shape.Count == 1)
				{
					string strRep = shape.First.StrRep();
					if (_ignoreModifiers)
						strRep = StripModifiers(strRep);
					_normalizedSegments.Add(strRep);
				}
			}
		}

		public IEnumerable<string> Segments
		{
			get { return _segments; }
		}

		public bool IgnoreModifiers
		{
			get { return _ignoreModifiers; }
		}

		public override bool Matches(Annotation<ShapeNode> ann)
		{
			string strRep = ann.StrRep();
			if (_ignoreModifiers)
				strRep = StripModifiers(strRep);

			if (ann.Span.Start.Prev.Type() == CogFeatureSystem.AnchorType && _normalizedSegments.Contains(string.Format("#{0}", strRep)))
				return true;

			if (ann.Span.End.Next.Type() == CogFeatureSystem.AnchorType && _normalizedSegments.Contains(string.Format("{0}#", strRep)))
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
