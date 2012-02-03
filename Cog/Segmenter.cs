using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class Segmenter
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 
		private readonly Dictionary<string, FeatureStruct> _vowels;
		private readonly Dictionary<string, FeatureStruct> _consonants;
		private readonly Dictionary<string, Tuple<FeatureStruct, bool>> _modifiers;
		private readonly Dictionary<string, FeatureStruct> _joiners;
		private readonly HashSet<string> _toneLetters;
		private readonly HashSet<string> _boundaries; 
		private Regex _regex;

		public Segmenter(SpanFactory<ShapeNode> spanFactory)
		{
			_spanFactory = spanFactory;
			_vowels = new Dictionary<string, FeatureStruct>();
			_consonants = new Dictionary<string, FeatureStruct>();
			_modifiers = new Dictionary<string, Tuple<FeatureStruct, bool>>();
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

		public void AddModifier(string strRep, FeatureStruct fs, bool overwrite)
		{
			_modifiers[strRep] = Tuple.Create(fs, overwrite);
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
				_regex = new Regex(CreateRegexString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			shape = new Shape(_spanFactory, new ShapeNode(_spanFactory, CogFeatureSystem.AnchorType, FeatureStruct.New().Value),
				new ShapeNode(_spanFactory, CogFeatureSystem.AnchorType, FeatureStruct.New().Value));

			foreach (Match match in _regex.Matches(str.Normalize(NormalizationForm.FormD)))
			{
				if (match.Groups["vowelSeg"].Success)
				{
					shape.Add(CogFeatureSystem.VowelType, BuildFeatStruct(match, "vowel", _vowels));
				}
				else if (match.Groups["consSeg"].Success)
				{
					shape.Add(CogFeatureSystem.ConsonantType, BuildFeatStruct(match, "cons", _consonants));
				}
				else if (match.Groups["affricate"].Success)
				{
					Capture fricative = match.Groups["cons"].Captures[match.Groups["cons"].Captures.Count - 1];
					FeatureStruct fs = _consonants[fricative.Value].Clone();
					fs.PriorityUnion(_joiners[match.Groups["joiner"].Value]);
					fs.AddValue(CogFeatureSystem.StrRep, match.Groups["affricate"].Value);
					shape.Add(CogFeatureSystem.ConsonantType, fs);
				}
				else if (match.Groups["tone"].Success)
				{
					//shape.Add(CogFeatureSystem.ToneType, FeatureStruct.New().Feature(CogFeatureSystem.StrRep).EqualTo(match.Groups["tone"].Value).Value);
				}
				else if (match.Groups["bdry"].Success)
				{
					//shape.Add(CogFeatureSystem.BoundaryType, FeatureStruct.New().Feature(CogFeatureSystem.StrRep).EqualTo(match.Groups["bdry"].Value).Value);
				}

				if (match.Index + match.Length == str.Length)
					return true;
			}

			shape = null;
			return false;
		}

		private FeatureStruct BuildFeatStruct(Match match, string groupName, Dictionary<string, FeatureStruct> bases)
		{
			string baseStr = match.Groups[groupName].Value.ToLowerInvariant();
			FeatureStruct fs = bases[baseStr].Clone();
			var sb = new StringBuilder();
			sb.Append(baseStr);
			var modStrs = new List<string>();
			foreach (Capture modifier in match.Groups["mod"].Captures)
			{
				string modStr = modifier.Value;
				Tuple<FeatureStruct, bool> modInfo = _modifiers[modStr];
				if (modInfo.Item2)
					fs.PriorityUnion(modInfo.Item1);
				else
					fs.Union(modInfo.Item1);

				if (modStr.Length == 1 && IsStackingDiacritic(modStr[0]))
					sb.Append(modStr);
				else
					modStrs.Add(modStr);
			}
			modStrs.Sort();
			fs.AddValue(CogFeatureSystem.StrRep, modStrs.Aggregate(sb, (strRep, modStr) => strRep.Append(modStr)).ToString());
			return fs;
		}

		private static bool IsStackingDiacritic(char c)
		{
			switch (CharUnicodeInfo.GetUnicodeCategory(c))
			{
				case UnicodeCategory.NonSpacingMark:
				case UnicodeCategory.SpacingCombiningMark:
				case UnicodeCategory.EnclosingMark:
					return true;
			}

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
			return string.Format("(?'affricate'{0}{3}{0})|(?'consSeg'{0}{2}*)|(?'vowelSeg'{1}{2}*)|{4}|{5}", consStr, vowelStr, modStr, joinerStr, toneStr, bdryStr);
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
				foreach (char c in str)
					sb.AppendFormat("\\u{0}", ((int) c).ToString("X4"));
				if (str.Length > 1)
					sb.Append(")");
				first = false;
			}
			sb.Append(")");
			return sb.ToString();
		}
	}
}
