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

		private readonly List<Tuple<string, string>> _mappings;
		private readonly bool _implicitComplexSegments;
		private readonly Segmenter _segmenter;

		private readonly Dictionary<string, Dictionary<string, List<Tuple<Environment, Environment>>>> _mappingLookup;

		public ListSegmentMappings(Segmenter segmenter, IEnumerable<Tuple<string, string>> mappings, bool implicitComplexSegments)
		{
			_segmenter = segmenter;
			_mappings = mappings.ToList();
			_implicitComplexSegments = implicitComplexSegments;

			_mappingLookup = new Dictionary<string, Dictionary<string, List<Tuple<Environment, Environment>>>>();
			foreach (Tuple<string, string> mapping in _mappings)
			{
				Environment env1, env2;
				string str1, str2;
				if (Normalize(mapping.Item1, out str1, out env1) && Normalize(mapping.Item2, out str2, out env2))
				{
					Dictionary<string, List<Tuple<Environment, Environment>>> segments = _mappingLookup.GetValue(str1, () => new Dictionary<string, List<Tuple<Environment, Environment>>>());
					List<Tuple<Environment, Environment>> contexts = segments.GetValue(str2, () => new List<Tuple<Environment, Environment>>());
					contexts.Add(Tuple.Create(env1, env2));
					segments = _mappingLookup.GetValue(str2, () => new Dictionary<string, List<Tuple<Environment, Environment>>>());
					contexts = segments.GetValue(str1, () => new List<Tuple<Environment, Environment>>());
					contexts.Add(Tuple.Create(env2, env1));
				}
			}
		}

		private bool Normalize(string segment, out string normalizedSegment, out Environment env)
		{
			normalizedSegment = null;
			if (string.IsNullOrEmpty(segment) || segment.IsOneOf("#", "C", "V"))
			{
				env = null;
				return false;
			}

			string strRep = StripContext(segment, out env);
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

			env = null;
			return false;
		}

		private string StripContext(string strRep, out Environment env)
		{
			FeatureSymbol leftEnv = GetEnvironment(strRep[0]);
			if (leftEnv != null)
				strRep = strRep.Remove(0, 1);
			FeatureSymbol rightEnv = GetEnvironment(strRep[strRep.Length - 1]);
			if (rightEnv != null)
				strRep = strRep.Remove(strRep.Length - 1, 1);
			env = new Environment(leftEnv, rightEnv);
			return strRep;
		}

		private FeatureSymbol GetEnvironment(char c)
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
