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

		public IReadOnlyCollection<string> Vowels
		{
			get { return _vowels.Keys.AsReadOnlyCollection(); }
		}

		public IReadOnlyCollection<string> Consonants
		{
			get { return _consonants.Keys.AsReadOnlyCollection(); }
		}

		public IReadOnlyCollection<string> Modifiers
		{
			get { return _modifiers.Keys.AsReadOnlyCollection(); }
		}

		public IReadOnlyCollection<string> Joiners
		{
			get { return _joiners.Keys.AsReadOnlyCollection(); }
		}

		public IReadOnlyCollection<string> ToneLetters
		{
			get { return _toneLetters.AsReadOnlyCollection(); }
		}

		public IReadOnlyCollection<string> Boundaries
		{
			get { return _boundaries.AsReadOnlyCollection(); }
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
			shape = new Shape(_spanFactory, begin => new ShapeNode(_spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value));

			foreach (Match match in _regex.Matches(str.Normalize(NormalizationForm.FormD)))
			{
				if (match.Groups["vowelSeg"].Success)
				{
					Group vowelComp = match.Groups["vowelComp"];
					string strRep;
					FeatureStruct phonemeFS = BuildFeatStruct(match, vowelComp.Captures[0], "vowelBase", _vowels, out strRep);
					var sb = new StringBuilder();
					sb.Append(strRep);
					if (match.Groups["joiner"].Success)
					{
						string joinerStr = match.Groups["joiner"].Value;
						sb.Append(joinerStr);
						phonemeFS.Union(BuildFeatStruct(match, vowelComp.Captures[1], "vowelBase", _vowels, out strRep));
						sb.Append(strRep);
						phonemeFS.PriorityUnion(_joiners[joinerStr]);
					}

					phonemeFS.AddValue(CogFeatureSystem.StrRep, sb.ToString());
					phonemeFS.AddValue(CogFeatureSystem.Type, CogFeatureSystem.VowelType);
					shape.Add(phonemeFS);
				}
				else if (match.Groups["consSeg"].Success)
				{
					Group consComp = match.Groups["consComp"];
					string strRep;
					FeatureStruct phonemeFS = BuildFeatStruct(match, consComp.Captures[0], "consBase", _consonants, out strRep);
					var sb = new StringBuilder();
					sb.Append(strRep);
					if (match.Groups["joiner"].Success)
					{
						string joinerStr = match.Groups["joiner"].Value;
						sb.Append(joinerStr);
						phonemeFS.Union(BuildFeatStruct(match, consComp.Captures[1], "consBase", _consonants, out strRep));
						sb.Append(strRep);
						phonemeFS.PriorityUnion(_joiners[joinerStr]);
					}

					phonemeFS.AddValue(CogFeatureSystem.StrRep, sb.ToString());
					phonemeFS.AddValue(CogFeatureSystem.Type, CogFeatureSystem.ConsonantType);
					shape.Add(phonemeFS);
				}

				if (match.Index + match.Length == str.Length)
					return true;
			}

			shape = null;
			return false;
		}

		private FeatureStruct BuildFeatStruct(Match match, Capture capture, string baseGroupName, Dictionary<string, FeatureStruct> bases, out string strRep)
		{
			string baseStr = match.Groups[baseGroupName].Captures.Cast<Capture>().Single(cap => capture.Index == cap.Index).Value;
			FeatureStruct fs = bases[baseStr].Clone();
			var sb = new StringBuilder();
			sb.Append(baseStr);
			var modStrs = new List<string>();
			foreach (Capture modifier in match.Groups["mod"].Captures.Cast<Capture>().Where(cap => capture.Index <= cap.Index && (capture.Index + capture.Length) >= (cap.Index + cap.Length)))
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
			strRep = modStrs.OrderBy(str => str).Aggregate(sb, (s, modStr) => s.Append(modStr)).ToString();
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
			string vowelBaseStr = CreateSymbolRegexString("vowelBase", _vowels.Keys);
			string consBaseStr = CreateSymbolRegexString("consBase", _consonants.Keys);
			string modStr = CreateSymbolRegexString("mod", _modifiers.Keys);
			string joinerStr = CreateSymbolRegexString("joiner", _joiners.Keys);
			string toneStr = CreateSymbolRegexString("tone", _toneLetters);
			string bdryStr = CreateSymbolRegexString("bdry", _boundaries);

			string consCompStr = string.Format("(?'consComp'{0}{1}*)", consBaseStr, modStr);
			string voweCompStr = string.Format("(?'vowelComp'{0}{1}*)", vowelBaseStr, modStr);
			return string.Format("(?'consSeg'{0}(?:{2}{0})?)|(?'vowelSeg'{1}(?:{2}{1})?)|{3}|{4}", consCompStr, voweCompStr, joinerStr, toneStr, bdryStr);
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
				sb.Append(str);
				if (str.Length > 1)
					sb.Append(")");
				first = false;
			}
			sb.Append(")");
			return sb.ToString();
		}
	}
}
