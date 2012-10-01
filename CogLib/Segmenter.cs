using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class Segmenter : NotifyPropertyChangedBase
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 
		private readonly SymbolCollection _vowels;
		private readonly SymbolCollection _consonants;
		private readonly SymbolCollection _modifiers;
		private readonly SymbolCollection _joiners;
		private readonly SymbolCollection _toneLetters;
		private readonly SymbolCollection _boundaries;
		private readonly Shape _emptyShape;
		private Regex _regex;

		private int _maxConsonantLength = 1;
		private int _maxVowelLength = 1;

		public Segmenter(SpanFactory<ShapeNode> spanFactory)
		{
			_spanFactory = spanFactory;
			_vowels = new SymbolCollection();
			_vowels.CollectionChanged += SymbolCollectionChanged;
			_consonants = new SymbolCollection();
			_consonants.CollectionChanged += SymbolCollectionChanged;
			_modifiers = new SymbolCollection();
			_modifiers.CollectionChanged += SymbolCollectionChanged;
			_joiners = new SymbolCollection();
			_joiners.CollectionChanged += SymbolCollectionChanged;
			_toneLetters = new SymbolCollection();
			_toneLetters.CollectionChanged += SymbolCollectionChanged;
			_boundaries = new SymbolCollection();
			_boundaries.CollectionChanged += SymbolCollectionChanged;

			_emptyShape = new Shape(_spanFactory, begin => new ShapeNode(_spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value));
			_emptyShape.Freeze();
		}

		private void SymbolCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_regex = null;
		}

		public SymbolCollection Vowels
		{
			get { return _vowels; }
		}

		public SymbolCollection Consonants
		{
			get { return _consonants; }
		}

		public SymbolCollection Modifiers
		{
			get { return _modifiers; }
		}

		public SymbolCollection Joiners
		{
			get { return _joiners; }
		}

		public SymbolCollection ToneLetters
		{
			get { return _toneLetters; }
		}

		public SymbolCollection Boundaries
		{
			get { return _boundaries; }
		}

		public int MaxConsonantLength
		{
			get { return _maxConsonantLength; }
			set
			{
				if (value < 1)
					throw new ArgumentOutOfRangeException("value");

				_maxConsonantLength = value;
				_regex = null;
				OnPropertyChanged("MaxConsonantLength");
			}
		}

		public int MaxVowelLength
		{
			get { return _maxVowelLength; }
			set
			{
				if (value < 1)
					throw new ArgumentOutOfRangeException("value");

				_maxVowelLength = value;
				_regex = null;
				OnPropertyChanged("MaxVowelLength");
			}
		}

		public Shape EmptyShape
		{
			get { return _emptyShape; }
		}

		public bool ToShape(string str, out Shape shape)
		{
			if (string.IsNullOrEmpty(str))
			{
				shape = _emptyShape;
				return true;
			}

			if (_regex == null)
				_regex = new Regex(CreateRegexString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

			shape = new Shape(_spanFactory, begin => new ShapeNode(_spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value));

			int index = 0;
			foreach (Match match in _regex.Matches(str.Normalize(NormalizationForm.FormD)))
			{
				if (match.Index != index)
					break;

				if (match.Groups["vowelSeg"].Success)
				{
					Group vowelComp = match.Groups["vowelComp"];
					string strRep;
					FeatureStruct phonemeFS = BuildFeatStruct(match, vowelComp.Captures[0], "vowelBase", _vowels, out strRep);
					var sb = new StringBuilder();
					sb.Append(strRep);
					if (match.Groups["joiner"].Success)
					{
						Group joinerGroup = match.Groups["joiner"];
						for (int i = 0; i < joinerGroup.Captures.Count; i++)
						{
							string joinerStr = joinerGroup.Captures[i].Value;
							//sb.Append(joinerStr);
							phonemeFS.Union(BuildFeatStruct(match, vowelComp.Captures[i + 1], "vowelBase", _vowels, out strRep));
							sb.Append(strRep);
							phonemeFS.PriorityUnion(_joiners[joinerStr].FeatureStruct);
						}
					}
					else if (vowelComp.Captures.Count > 1)
					{
						for (int i = 1; i < vowelComp.Captures.Count; i++)
						{
							phonemeFS.Union(BuildFeatStruct(match, vowelComp.Captures[i], "vowelBase", _vowels, out strRep));
							sb.Append(strRep);
						}
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
						Group joinerGroup = match.Groups["joiner"];
						for (int i = 0; i < joinerGroup.Captures.Count; i++)
						{
							string joinerStr = joinerGroup.Captures[i].Value;
							//sb.Append(joinerStr);
							phonemeFS.Union(BuildFeatStruct(match, consComp.Captures[i + 1], "consBase", _consonants, out strRep));
							sb.Append(strRep);
							phonemeFS.PriorityUnion(_joiners[joinerStr].FeatureStruct);
						}
					}
					else if (consComp.Captures.Count > 1)
					{
						for (int i = 1; i < consComp.Captures.Count; i++)
						{
							phonemeFS.Union(BuildFeatStruct(match, consComp.Captures[i], "consBase", _consonants, out strRep));
							sb.Append(strRep);
						}
					}

					phonemeFS.AddValue(CogFeatureSystem.StrRep, sb.ToString());
					phonemeFS.AddValue(CogFeatureSystem.Type, CogFeatureSystem.ConsonantType);
					shape.Add(phonemeFS);
				}

				index = match.Index + match.Length;

				if (index == str.Length)
				{
					if (shape.Count == 0)
						break;

					shape.Freeze();
					return true;
				}
			}

			shape = null;
			return false;
		}

		private FeatureStruct BuildFeatStruct(Match match, Capture capture, string baseGroupName, SymbolCollection bases, out string strRep)
		{
			string baseStr = match.Groups[baseGroupName].Captures.Cast<Capture>().Single(cap => capture.Index == cap.Index).Value;
			FeatureStruct fs = bases[baseStr].FeatureStruct.DeepClone();
			var sb = new StringBuilder();
			sb.Append(baseStr);
			var modStrs = new List<string>();
			foreach (Capture modifier in match.Groups["mod"].Captures.Cast<Capture>().Where(cap => capture.Index <= cap.Index && (capture.Index + capture.Length) >= (cap.Index + cap.Length)))
			{
				string modStr = modifier.Value;
				Symbol modInfo = _modifiers[modStr];
				if (!modInfo.FeatureStruct.IsEmpty)
				{
					if (modInfo.Overwrite)
						fs.PriorityUnion(modInfo.FeatureStruct);
					else
						fs.Add(modInfo.FeatureStruct);

					if (modStr.Length == 1 && IsStackingDiacritic(modStr[0]))
						sb.Append(modStr);
					else
						modStrs.Add(modStr);
				}
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
			string vowelBaseStr = string.Format("(?'vowelBase'{0})", CreateSymbolRegexString(_vowels));
			string consBaseStr = string.Format("(?'consBase'{0})", CreateSymbolRegexString(_consonants));
			string modStr = string.Format("(?'mod'{0})", CreateSymbolRegexString(_modifiers));
			string ncJoinerStr = CreateSymbolRegexString(_joiners);
			string joinerStr = string.Format("(?'joiner'{0})", ncJoinerStr);
			string toneStr = string.Format("(?'tone'{0})", CreateSymbolRegexString(_toneLetters));
			string bdryStr = string.Format("(?'bdry'{0})", CreateSymbolRegexString(_boundaries));

			string consCompStr = string.Format("(?'consComp'{0}{1}*)", consBaseStr, modStr);
			string vowelCompStr = string.Format("(?'vowelComp'{0}{1}*)", vowelBaseStr, modStr);

			return string.Format("(?'consSeg'{0}(?({2})(?:{3}{0})*|{0}{{0,{6}}}))|(?'vowelSeg'{1}(?({2})(?:{3}{1})*|{1}{{0,{7}}}))|{4}|{5}",
				consCompStr, vowelCompStr, ncJoinerStr, joinerStr, toneStr, bdryStr, _maxConsonantLength, _maxVowelLength);
		}

		private static string CreateSymbolRegexString(IEnumerable<Symbol> symbols)
		{
			var sb = new StringBuilder();
			bool first = true;
			foreach (Symbol symbol in symbols)
			{
				if (!first)
					sb.Append("|");
				if (symbol.StrRep.Length > 1)
					sb.Append("(?:");
				sb.Append(Regex.Escape(symbol.StrRep));
				if (symbol.StrRep.Length > 1)
					sb.Append(")");
				first = false;
			}
			return sb.ToString();
		}
	}
}
