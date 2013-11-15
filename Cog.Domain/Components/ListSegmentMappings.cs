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
		private class Context
		{
			private readonly Environment _leftEnv;
			private readonly Environment _rightEnv;

			public Context(Environment leftEnv, Environment rightEnv)
			{
				_leftEnv = leftEnv;
				_rightEnv = rightEnv;
			}

			public Environment LeftEnvironment
			{
				get { return _leftEnv; }
			}

			public Environment RightEnvironment
			{
				get { return _rightEnv; }
			}
		}

		private enum Environment
		{
			WordBoundary,
			Vowel,
			Consonant,
			None
		}

		private readonly List<Tuple<string, string>> _mappings;
		private readonly bool _implicitComplexSegments;
		private readonly Segmenter _segmenter;

		private readonly Dictionary<string, Dictionary<string, List<Tuple<Context, Context>>>> _mappingLookup;

		public ListSegmentMappings(Segmenter segmenter, IEnumerable<Tuple<string, string>> mappings, bool implicitComplexSegments)
		{
			_segmenter = segmenter;
			_mappings = mappings.ToList();
			_implicitComplexSegments = implicitComplexSegments;

			_mappingLookup = new Dictionary<string, Dictionary<string, List<Tuple<Context, Context>>>>();
			foreach (Tuple<string, string> mapping in _mappings)
			{
				Context ctxt1, ctxt2;
				string str1, str2;
				if (Normalize(mapping.Item1, out str1, out ctxt1) && Normalize(mapping.Item2, out str2, out ctxt2))
				{
					Dictionary<string, List<Tuple<Context, Context>>> segments = _mappingLookup.GetValue(str1, () => new Dictionary<string, List<Tuple<Context, Context>>>());
					List<Tuple<Context, Context>> contexts = segments.GetValue(str2, () => new List<Tuple<Context, Context>>());
					contexts.Add(Tuple.Create(ctxt1, ctxt2));
					segments = _mappingLookup.GetValue(str2, () => new Dictionary<string, List<Tuple<Context, Context>>>());
					contexts = segments.GetValue(str1, () => new List<Tuple<Context, Context>>());
					contexts.Add(Tuple.Create(ctxt2, ctxt1));
				}
			}
		}

		private bool Normalize(string segment, out string normalizedSegment, out Context ctxt)
		{
			normalizedSegment = null;
			if (string.IsNullOrEmpty(segment) || segment.IsOneOf("#", "C", "V"))
			{
				ctxt = null;
				return false;
			}

			string strRep = StripContext(segment, out ctxt);
			if (strRep.IsOneOf("-", "_"))
			{
				normalizedSegment = "-";
				return true;
			}
			string normalized;
			if (_segmenter.NormalizeSegmentString(strRep, out normalized))
			{
				normalizedSegment = normalized;
				return true;
			}

			ctxt = null;
			return false;
		}

		private string StripContext(string strRep, out Context ctxt)
		{
			Environment leftEnv = GetEnvironment(strRep[0]);
			if (leftEnv != Environment.None)
				strRep = strRep.Remove(0, 1);
			Environment rightEnv = GetEnvironment(strRep[strRep.Length - 1]);
			if (rightEnv != Environment.None)
				strRep = strRep.Remove(strRep.Length - 1, 1);
			ctxt = new Context(leftEnv, rightEnv);
			return strRep;
		}

		private Environment GetEnvironment(char c)
		{
			switch (c)
			{
				case '#':
					return Environment.WordBoundary;
				case 'C':
					return Environment.Consonant;
				case 'V':
					return Environment.Vowel;
				default:
					return Environment.None;
			}
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
			foreach (string strRep1 in GetStrReps(target1))
			{
				foreach (string strRep2 in GetStrReps(target2))
				{
					Dictionary<string, List<Tuple<Context, Context>>> segments;
					List<Tuple<Context, Context>> contexts;
					if (_mappingLookup.TryGetValue(strRep1, out segments) && segments.TryGetValue(strRep2, out contexts))
						return contexts.Any(ctxt => CheckContext(ctxt.Item1, leftNode1, rightNode1) && CheckContext(ctxt.Item2, leftNode2, rightNode2));
				}
			}
			return false;
		}

		private IEnumerable<string> GetStrReps(Ngram<Segment> target)
		{
			if (target.Count == 0)
			{
				yield return "-";
			}
			else
			{
				foreach (Segment seg in target)
				{
					yield return seg.StrRep;
					if (_implicitComplexSegments && seg.IsComplex)
					{
						Shape shape = _segmenter.Segment(seg.StrRep);
						foreach (ShapeNode node in shape)
							yield return node.StrRep();
					}
				}	
			}
		}

		private bool CheckContext(Context ctxt, ShapeNode leftNode, ShapeNode rightNode)
		{
			return CheckEnvironment(ctxt.LeftEnvironment, leftNode) && CheckEnvironment(ctxt.RightEnvironment, rightNode);
		}

		private bool CheckEnvironment(Environment env, ShapeNode node)
		{
			switch (env)
			{
				case Environment.WordBoundary:
					return node.Type() == CogFeatureSystem.AnchorType;
				case Environment.Consonant:
					return node.Type() == CogFeatureSystem.ConsonantType;
				case Environment.Vowel:
					return node.Type() == CogFeatureSystem.VowelType;
				default:
					return true;
			}
		}
	}
}
