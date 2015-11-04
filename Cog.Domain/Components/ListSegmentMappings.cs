using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;
using SIL.Machine.NgramModeling;

namespace SIL.Cog.Domain.Components
{
	public class ListSegmentMappings : ISegmentMappings
	{
		private class Environment
		{
			private readonly FeatureSymbol _leftEnv;
			private readonly FeatureSymbol _rightEnv;

			public Environment(FeatureSymbol leftEnv, FeatureSymbol rightEnv)
			{
				_leftEnv = leftEnv;
				_rightEnv = rightEnv;
			}

			public FeatureSymbol LeftEnvironment
			{
				get { return _leftEnv; }
			}

			public FeatureSymbol RightEnvironment
			{
				get { return _rightEnv; }
			}
		}

		private readonly List<UnorderedTuple<string, string>> _mappings;
		private readonly bool _implicitComplexSegments;
		private readonly Segmenter _segmenter;

		private readonly Dictionary<string, Dictionary<string, List<Tuple<Environment, Environment>>>> _mappingLookup;

		public ListSegmentMappings(Segmenter segmenter, IEnumerable<UnorderedTuple<string, string>> mappings, bool implicitComplexSegments)
		{
			_segmenter = segmenter;
			_mappings = mappings.ToList();
			_implicitComplexSegments = implicitComplexSegments;

			_mappingLookup = new Dictionary<string, Dictionary<string, List<Tuple<Environment, Environment>>>>();
			foreach (UnorderedTuple<string, string> mapping in _mappings)
			{
				FeatureSymbol leftEnv1, rightEnv1, leftEnv2, rightEnv2;
				string str1, str2;
				if (Normalize(_segmenter, mapping.Item1, out str1, out leftEnv1, out rightEnv1) && Normalize(_segmenter, mapping.Item2, out str2, out leftEnv2, out rightEnv2))
				{
					var env1 = new Environment(leftEnv1, rightEnv1);
					var env2 = new Environment(leftEnv2, rightEnv2);
					Dictionary<string, List<Tuple<Environment, Environment>>> segments = _mappingLookup.GetValue(str1, () => new Dictionary<string, List<Tuple<Environment, Environment>>>());
					List<Tuple<Environment, Environment>> contexts = segments.GetValue(str2, () => new List<Tuple<Environment, Environment>>());
					contexts.Add(Tuple.Create(env1, env2));
					segments = _mappingLookup.GetValue(str2, () => new Dictionary<string, List<Tuple<Environment, Environment>>>());
					contexts = segments.GetValue(str1, () => new List<Tuple<Environment, Environment>>());
					contexts.Add(Tuple.Create(env2, env1));
				}
			}
		}

		public static bool IsValid(Segmenter segmenter, string segment)
		{
			string normalizedSegment;
			FeatureSymbol leftEnv, rightEnv;
			return Normalize(segmenter, segment, out normalizedSegment, out leftEnv, out rightEnv);
		}

		public static bool Normalize(Segmenter segmenter, string segment, out string normalizedSegment, out FeatureSymbol leftEnv, out FeatureSymbol rightEnv)
		{
			normalizedSegment = null;
			if (string.IsNullOrEmpty(segment) || segment.IsOneOf("#", "C", "V"))
			{
				leftEnv = null;
				rightEnv = null;
				return false;
			}

			string strRep = StripContext(segment, out leftEnv, out rightEnv);
			if (strRep.IsOneOf("-", "_"))
			{
				normalizedSegment = "-";
				return true;
			}
			string normalized;
			if (segmenter.NormalizeSegmentString(strRep, out normalized))
			{
				normalizedSegment = normalized;
				return true;
			}

			leftEnv = null;
			rightEnv = null;
			return false;
		}

		private static string StripContext(string strRep, out FeatureSymbol leftEnv, out FeatureSymbol rightEnv)
		{
			leftEnv = GetEnvironment(strRep[0]);
			if (leftEnv != null)
				strRep = strRep.Remove(0, 1);
			rightEnv = GetEnvironment(strRep[strRep.Length - 1]);
			if (rightEnv != null)
				strRep = strRep.Remove(strRep.Length - 1, 1);
			return strRep;
		}

		private static FeatureSymbol GetEnvironment(char c)
		{
			switch (c)
			{
				case '#':
					return CogFeatureSystem.AnchorType;
				case 'C':
					return CogFeatureSystem.ConsonantType;
				case 'V':
					return CogFeatureSystem.VowelType;
				default:
					return null;
			}
		}

		public IEnumerable<UnorderedTuple<string, string>> Mappings
		{
			get { return _mappings; }
		}

		public bool ImplicitComplexSegments
		{
			get { return _implicitComplexSegments; }
		}

		public bool IsMapped(ShapeNode leftNode1, Ngram<Segment> target1, ShapeNode rightNode1, ShapeNode leftNode2, Ngram<Segment> target2, ShapeNode rightNode2)
		{
			if (_mappings.Count == 0)
				return false;

			foreach (string strRep1 in GetStrReps(target1))
			{
				foreach (string strRep2 in GetStrReps(target2))
				{
					if (strRep1 == strRep2)
						return true;

					Dictionary<string, List<Tuple<Environment, Environment>>> segments;
					List<Tuple<Environment, Environment>> contexts;
					if (_mappingLookup.TryGetValue(strRep1, out segments) && segments.TryGetValue(strRep2, out contexts))
						return contexts.Any(ctxt => CheckEnvironment(ctxt.Item1, leftNode1, rightNode1) && CheckEnvironment(ctxt.Item2, leftNode2, rightNode2));
				}
			}
			return false;
		}

		private IEnumerable<string> GetStrReps(Ngram<Segment> target)
		{
			if (target.Length == 0)
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

		private bool CheckEnvironment(Environment env, ShapeNode leftNode, ShapeNode rightNode)
		{
			return (env.LeftEnvironment == null || env.LeftEnvironment == leftNode.Type()) && (env.RightEnvironment == null || env.RightEnvironment == rightNode.Type());
		}
	}
}
