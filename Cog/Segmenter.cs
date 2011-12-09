using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class Segmenter
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 
		private readonly Dictionary<string, FeatureStruct> _vowels;
		private readonly Dictionary<string, FeatureStruct> _consonants;
		private readonly Dictionary<string, FeatureStruct> _modifiers;
		private readonly Dictionary<string, FeatureStruct> _joiners;
		private readonly HashSet<string> _toneLetters;
		private readonly HashSet<string> _boundaries; 
		private Regex _regex;

		public Segmenter(SpanFactory<ShapeNode> spanFactory)
		{
			_spanFactory = spanFactory;
			_vowels = new Dictionary<string, FeatureStruct>();
			_consonants = new Dictionary<string, FeatureStruct>();
			_modifiers = new Dictionary<string, FeatureStruct>();
			_joiners = new Dictionary<string, FeatureStruct>();
			_toneLetters = new HashSet<string>();
			_boundaries = new HashSet<string>();
		}

		public void AddVowel(string strRep, FeatureStruct fs)
		{
			_vowels[strRep] = fs;
			_regex = null;
		}

		public void AddConsonant(string strRep, FeatureStruct fs)
		{
			_consonants[strRep] = fs;
			_regex = null;
		}

		public void AddModifier(string strRep, FeatureStruct fs)
		{
			_modifiers[strRep] = fs;
			_regex = null;
		}

		public void AddJoiner(string strRep, FeatureStruct fs)
		{
			_joiners[strRep] = fs;
			_regex = null;
		}

		public void AddToneLetter(string strRep)
		{
			_toneLetters.Add(strRep);
			_regex = null;
		}

		public void AddBoundary(string strRep)
		{
			_boundaries.Add(strRep);
			_regex = null;
		}

		public bool ToShape(string str, out Shape shape)
		{
			if (_regex == null)
				_regex = new Regex(CreateRegexString());
			shape = new Shape(_spanFactory, new ShapeNode(_spanFactory, CogFeatureSystem.AnchorType, FeatureStruct.New().Value),
				new ShapeNode(_spanFactory, CogFeatureSystem.AnchorType, FeatureStruct.New().Value));
			foreach (Match match in _regex.Matches(str))
			{
				if (match.Groups["vowelSeg"].Success)
				{
					FeatureStruct fs = _vowels[match.Groups["vowel"].Value].Clone();
					foreach (Capture modifier in match.Groups["mod"].Captures)
						fs.PriorityUnion(_modifiers[modifier.Value]);
					fs.AddValue(CogFeatureSystem.StrRep, match.Groups["vowelSeg"].Value);
					shape.Add(CogFeatureSystem.VowelType, fs);
				}
				else if (match.Groups["consSeg"].Success)
				{
					FeatureStruct fs = _consonants[match.Groups["cons"].Value].Clone();
					foreach (Capture modifier in match.Groups["mod"].Captures)
						fs.PriorityUnion(_modifiers[modifier.Value]);
					fs.AddValue(CogFeatureSystem.StrRep, match.Groups["consSeg"].Value);
					shape.Add(CogFeatureSystem.ConsonantType, fs);
				}
				else if (match.Groups["affricate"].Success)
				{
					FeatureStruct fs = null;
					foreach (Capture modifier in match.Groups["cons"].Captures)
					{
						FeatureStruct curFS = _consonants[modifier.Value];
						if (fs == null)
							fs = curFS;
						else
							fs.Merge(curFS);
					}
					fs.PriorityUnion(_joiners[match.Groups["joiner"].Value]);
					fs.AddValue(CogFeatureSystem.StrRep, match.Groups["affricate"].Value);
					shape.Add(CogFeatureSystem.ConsonantType, fs);
				}
				else if (match.Groups["tone"].Success)
				{
					//shape.Add(CogFeatureSystem.ToneType, FeatureStruct.New(CogFeatureSystem.Instance).Feature(CogFeatureSystem.StrRep).EqualTo(match.Groups["tone"].Value).Value);
				}
				else if (match.Groups["bdry"].Success)
				{
					//shape.Add(CogFeatureSystem.BoundaryType, FeatureStruct.New(CogFeatureSystem.Instance).Feature(CogFeatureSystem.StrRep).EqualTo(match.Groups["bdry"].Value).Value);
				}

				if (match.Index + match.Length == str.Length)
					return true;
			}

			shape = null;
			return false;
		}

		private string CreateRegexString()
		{
			string vowelStr = CreateSymbolRegexString("vowel", _vowels.Keys);
			string consStr = CreateSymbolRegexString("cons", _consonants.Keys);
			string modStr = CreateSymbolRegexString("mod", _modifiers.Keys);
			string joinerStr = CreateSymbolRegexString("joiner", _joiners.Keys);
			string toneStr = CreateSymbolRegexString("tone", _toneLetters);
			string bdryStr = CreateSymbolRegexString("bdry", _boundaries);
			return string.Format("(?'vowelSeg'{0}{2}*)|(?'consSeg'{1}{2}*)|(?'affricate'{1}{3}{1})|{4}|{5}", vowelStr, consStr, modStr, joinerStr, toneStr, bdryStr);
		}

		private static string CreateSymbolRegexString(string name, IEnumerable<string> strings)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("(?'{0}'", name);
			bool first = true;
			foreach (string str in strings)
			{
				if (!first)
					sb.Append("|");
				if (str.Length > 1)
					sb.Append("(?:");
				sb.Append(Regex.Escape(str));
				if (str.Length > 1)
					sb.Append(")");
				first = false;
			}
			sb.Append(")");
			return sb.ToString();
		}
	}
}
