using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Processors
{
	public class ListSimilarSegmentIdentifier : ProcessorBase<VarietyPair>
	{
		private readonly List<Tuple<string, string>> _vowelMappings;
		private readonly List<Tuple<string, string>> _consMappings;
		private readonly bool _generateDiphthongs;

		private readonly Dictionary<string, HashSet<string>> _similarVowels;
		private readonly Dictionary<string, HashSet<string>> _similarConsonants;

		public ListSimilarSegmentIdentifier(CogProject project, IEnumerable<Tuple<string, string>> vowelMappings, IEnumerable<Tuple<string, string>> consMappings, bool generateDiphthongs)
			: base(project)
		{
			_vowelMappings = new List<Tuple<string, string>>(vowelMappings);
			_consMappings = new List<Tuple<string, string>>(consMappings);
			_generateDiphthongs = generateDiphthongs;

			_similarVowels = new Dictionary<string, HashSet<string>>();
			GenerateSimilarSegments(_vowelMappings, _similarVowels);

			if (_generateDiphthongs)
			{
				string[] vowels = _similarVowels.Keys.Where(v => v != "-").ToArray();
				foreach (string vowel1 in vowels)
				{
					foreach (string vowel2 in vowels)
					{
						if (vowel1 == vowel2)
							continue;

						string seg = vowel1 + vowel2;
						HashSet<string> segments;
						if (!_similarVowels.TryGetValue(seg, out segments))
						{
							segments = new HashSet<string>(_similarVowels[vowel1]);
							segments.UnionWith(_similarVowels[vowel2]);
							segments.Add(vowel1);
							segments.Add(vowel2);
							_similarVowels[seg] = segments;
							foreach (string otherSeg in segments)
							{
								HashSet<string> otherSegments = _similarVowels.GetValue(otherSeg, () => new HashSet<string>());
								otherSegments.Add(seg);
							}
						}
					}
				}
			}

			_similarConsonants = new Dictionary<string, HashSet<string>>();
			GenerateSimilarSegments(_consMappings, _similarConsonants);
		}

		public IEnumerable<Tuple<string, string>> VowelMappings
		{
			get { return _vowelMappings; }
		}

		public IEnumerable<Tuple<string, string>> ConsonantMappings
		{
			get { return _consMappings; }
		}

		public bool GenerateDiphthongs
		{
			get { return _generateDiphthongs; }
		}

		private void GenerateSimilarSegments(IEnumerable<Tuple<string, string>> mappings, Dictionary<string, HashSet<string>> similarSegments)
		{
			foreach (Tuple<string, string> mapping in mappings)
			{
				string str1, str2;
				if (GetNormalizedStrRep(mapping.Item1, out str1) && GetNormalizedStrRep(mapping.Item2, out str2))
				{
					HashSet<string> segments = similarSegments.GetValue(str1, () => new HashSet<string>());
					segments.Add(str2);
					segments = similarSegments.GetValue(str2, () => new HashSet<string>());
					segments.Add(str1);
				}
			}
		}

		private bool GetNormalizedStrRep(string str, out string normalizedStr)
		{
			Shape shape;
			if (Project.Segmenter.ToShape(str, out shape) && shape.Count == 1)
			{
				normalizedStr = shape.First.StrRep();
				return true;
			}
			normalizedStr = null;
			return false;
		}

		public override void Process(VarietyPair varietyPair)
		{
			foreach (Segment seg1 in varietyPair.Variety1.Segments)
			{
				Dictionary<string, HashSet<string>> similarSegments = seg1.Type == CogFeatureSystem.ConsonantType ? _similarConsonants : _similarVowels;
				string str1 = seg1.StrRep;

				HashSet<string> segments;
				if (similarSegments.TryGetValue(str1, out segments))
				{
					foreach (Segment seg2 in varietyPair.Variety2.Segments)
					{
						if (seg1.Type == seg2.Type && seg1.StrRep != seg2.StrRep)
						{
							string str2 = seg2.StrRep;
							if (segments.Contains(str2))
								varietyPair.AddSimilarSegment(seg1, seg2);
						}
					}

					if (segments.Contains("-"))
						varietyPair.AddSimilarSegment(seg1, Segment.Null);
				}
			}

			AddNullSegment(varietyPair, _similarConsonants, CogFeatureSystem.ConsonantType);
			AddNullSegment(varietyPair, _similarVowels, CogFeatureSystem.VowelType);
		}

		private void AddNullSegment(VarietyPair varietyPair, Dictionary<string, HashSet<string>> similarSegments, FeatureSymbol type)
		{
			HashSet<string> segments;
			if (similarSegments.TryGetValue("-", out segments))
			{
				foreach (Segment seg2 in varietyPair.Variety2.Segments.Where(s => s.Type == type))
				{
					if (segments.Contains(seg2.StrRep))
						varietyPair.AddSimilarSegment(Segment.Null, seg2);
				}
			}
		}
	}
}
