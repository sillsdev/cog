using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog
{
	public class SimilarSegmentListIdentifier : IProcessor<VarietyPair>
	{
		private readonly Dictionary<string, HashSet<string>> _similarVowels;
		private readonly Dictionary<string, HashSet<string>> _similarConsonants;
		private readonly HashSet<string> _joiners; 

		public SimilarSegmentListIdentifier(string vowelsPath, string consPath, IEnumerable<string> joiners)
		{
			_similarVowels = new Dictionary<string, HashSet<string>>();
			ReadSimilarSegments(vowelsPath, _similarVowels);

			string[] vowels = _similarVowels.Keys.ToArray();
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

			_similarConsonants = new Dictionary<string, HashSet<string>>();
			ReadSimilarSegments(consPath, _similarConsonants);

			_joiners = new HashSet<string>(joiners);
		}

		private void ReadSimilarSegments(string path, Dictionary<string, HashSet<string>> similarSegments)
		{
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
					HashSet<string> segments = similarSegments.GetValue(seg1, () => new HashSet<string>());
					segments.Add(seg2);
					segments = similarSegments.GetValue(seg2, () => new HashSet<string>());
					segments.Add(seg1);
				}
			}
		}

		public void Process(VarietyPair varietyPair)
		{
			foreach (Segment seg1 in varietyPair.Variety1.Segments)
			{
				foreach (Segment seg2 in varietyPair.Variety2.Segments)
				{
					if (seg1.Type == seg2.Type && seg1.StrRep != seg2.StrRep)
					{
						if (seg1.Type == CogFeatureSystem.ConsonantType)
						{
							HashSet<string> segments;
							if (_similarConsonants.TryGetValue(seg1.StrRep, out segments))
							{
								if (segments.Contains(seg2.StrRep))
									varietyPair.AddSimilarSegment(seg1, seg2);
							}
						}
						else if (seg1.Type == CogFeatureSystem.VowelType)
						{
							string str1 = RemoveJoiners(seg1.StrRep);
							HashSet<string> segments;
							if (_similarVowels.TryGetValue(str1, out segments))
							{
								string str2 = RemoveJoiners(seg2.StrRep);
								if (segments.Contains(str2))
									varietyPair.AddSimilarSegment(seg1, seg2);
							}
						}
					}
				}
			}
		}

		private string RemoveJoiners(string str)
		{
			foreach (string joiner in _joiners)
				str = str.Replace(joiner, "");
			return str;
		}
	}
}
