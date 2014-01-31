using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain
{
	public class Segmenter : ObservableObject
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

			_emptyShape = new Shape(_spanFactory, begin => new ShapeNode(_spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Feature(CogFeatureSystem.StrRep).EqualTo("#").Value));
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

				if (Set(() => MaxConsonantLength, ref _maxConsonantLength, value))
					_regex = null;
			}
		}

		public int MaxVowelLength
		{
			get { return _maxVowelLength; }
			set
			{
				if (value < 1)
					throw new ArgumentOutOfRangeException("value");

				if (Set(() => MaxVowelLength, ref _maxVowelLength, value))
					_regex = null;
			}
		}

		public void Segment(Affix affix)
		{
			if (string.IsNullOrEmpty(affix.StrRep))
			{
				affix.Shape = _emptyShape;
				return;
			}

			var shape = new Shape(_spanFactory, begin => new ShapeNode(_spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Feature(CogFeatureSystem.StrRep).EqualTo("#").Value));
			if (SegmentString(shape, affix.StrRep))
			{
				shape.Freeze();
				affix.Shape = shape;
			}
			else
			{
				affix.Shape = _emptyShape;
			}
		}

		public void Segment(Word word)
		{
			if (string.IsNullOrEmpty(word.StrRep))
			{
				word.Shape = _emptyShape;
				return;
			}

			var shape = new Shape(_spanFactory, begin => new ShapeNode(_spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Feature(CogFeatureSystem.StrRep).EqualTo("#").Value));

			string prefix = word.StrRep.Substring(0, word.StemIndex);
			ShapeNode start = shape.Begin;
			if (!string.IsNullOrEmpty(prefix))
			{
				if (SegmentString(shape, prefix))
				{
					shape.Annotations.Add(start.Next, shape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.PrefixType).Value);
					start = shape.Last;
				}
				else
				{
					word.Shape = _emptyShape;
					return;
				}
			}

			string stem = word.StrRep.Substring(word.StemIndex, word.StemLength);
			if (SegmentString(shape, stem))
			{
				shape.Annotations.Add(start.Next, shape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.StemType).Value);
				start = shape.Last;
			}
			else
			{
				word.Shape = _emptyShape;
				return;
			}

			string suffix = word.StrRep.Substring(word.StemIndex + word.StemLength);
			if (!string.IsNullOrEmpty(suffix))
			{
				if (SegmentString(shape, suffix))
				{
					shape.Annotations.Add(start.Next, shape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.SuffixType).Value);
				}
				else
				{
					word.Shape = _emptyShape;
					return;
				}
			}

			shape.Freeze();
			word.Shape = shape;
		}

		public Shape Segment(string str)
		{
			Shape shape;
			if (TrySegment(str, out shape))
				return shape;
			throw new ArgumentException("The input string cannot be segmented.", "str");
		}

		public bool TrySegment(string str, out Shape shape)
		{
			if (string.IsNullOrEmpty(str))
			{
				shape = _emptyShape;
				return true;
			}

			shape = new Shape(_spanFactory, begin => new ShapeNode(_spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Feature(CogFeatureSystem.StrRep).EqualTo("#").Value));

			if (SegmentString(shape, str))
			{
				shape.Freeze();
				return true;
			}

			shape = null;
			return false;
		}

		public bool CanSegment(string str)
		{
			if (_regex == null)
				_regex = new Regex(CreateRegexString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

			int index = 0;
			int nodeCount = 0;
			foreach (Match match in _regex.Matches(str))
			{
				if (match.Index != index)
					break;

				if (match.Groups["vowelSeg"].Success || match.Groups["consSeg"].Success || match.Groups["tone"].Success || match.Groups["bdry"].Success)
				{
					nodeCount++;
				}

				index = match.Index + match.Length;

				if (index == str.Length)
				{
					if (nodeCount == 0)
						break;

					return true;
				}
			}

			return false;
		}

		public bool IsValidSegment(string str)
		{
			Shape shape;
			return TrySegment(str, out shape) && shape.All(n => n.Type() == shape.First.Type());
		}

		public bool NormalizeSegmentString(string str, out string normalizedStr)
		{
			Shape shape;
			if (TrySegment(str, out shape) && shape.All(n => n.Type() == shape.First.Type()))
			{
				normalizedStr = shape.StrRep();
				return true;
			}
			normalizedStr = null;
			return false;
		}

		private bool SegmentString(Shape shape, string str)
		{
			if (_regex == null)
				_regex = new Regex(CreateRegexString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

			int index = 0;
			foreach (Match match in _regex.Matches(str))
			{
				if (match.Index != index)
					break;

				if (match.Groups["vowelSeg"].Success)
				{
					string strRep;
					FeatureStruct phonemeFS;
					if (!TryMultipleBaseCharacterSymbol(match, _vowels, out strRep, out phonemeFS))
					{
						var sb = new StringBuilder();
						Group vowelComp = match.Groups["vowelComp"];
						string partStrRep;
						phonemeFS = BuildFeatStruct(match, vowelComp.Captures[0], "vowelBase", _vowels, out partStrRep);
						sb.Append(partStrRep);
						Group joinerGroup = match.Groups["joiner"];
						if (joinerGroup.Success || vowelComp.Captures.Count > 1)
						{
							phonemeFS.AddValue(CogFeatureSystem.First, phonemeFS.DeepClone());
							phonemeFS.AddValue(CogFeatureSystem.SegmentType, CogFeatureSystem.Complex);
						}
						else
						{
							phonemeFS.AddValue(CogFeatureSystem.SegmentType, CogFeatureSystem.Simple);
						}
						if (joinerGroup.Success)
						{
							for (int i = 0; i < joinerGroup.Captures.Count; i++)
							{
								string joinerStr = joinerGroup.Captures[i].Value;
								phonemeFS.Add(BuildFeatStruct(match, vowelComp.Captures[i + 1], "vowelBase", _vowels, out partStrRep));
								sb.Append(partStrRep);
								FeatureStruct joinerFs = _joiners[joinerStr].FeatureStruct;
								if (joinerFs != null)
									phonemeFS.PriorityUnion(joinerFs);
							}
						}
						else if (vowelComp.Captures.Count > 1)
						{
							for (int i = 1; i < vowelComp.Captures.Count; i++)
							{
								phonemeFS.Add(BuildFeatStruct(match, vowelComp.Captures[i], "vowelBase", _vowels, out partStrRep));
								sb.Append(partStrRep);
							}
						}
						strRep = sb.ToString();
					}

					phonemeFS.AddValue(CogFeatureSystem.StrRep, strRep);
					phonemeFS.AddValue(CogFeatureSystem.OriginalStrRep, match.Value);
					phonemeFS.AddValue(CogFeatureSystem.Type, CogFeatureSystem.VowelType);
					shape.Add(phonemeFS);
				}
				else if (match.Groups["consSeg"].Success)
				{
					string strRep;
					FeatureStruct phonemeFS;
					if (!TryMultipleBaseCharacterSymbol(match, _consonants, out strRep, out phonemeFS))
					{
						var sb = new StringBuilder();
						Group consComp = match.Groups["consComp"];
						string compStrRep;
						phonemeFS = BuildFeatStruct(match, consComp.Captures[0], "consBase", _consonants, out compStrRep);
						sb.Append(compStrRep);
						Group joinerGroup = match.Groups["joiner"];
						if (joinerGroup.Success || consComp.Captures.Count > 1)
						{
							phonemeFS.AddValue(CogFeatureSystem.First, phonemeFS.DeepClone());
							phonemeFS.AddValue(CogFeatureSystem.SegmentType, CogFeatureSystem.Complex);
						}
						else
						{
							phonemeFS.AddValue(CogFeatureSystem.SegmentType, CogFeatureSystem.Simple);
						}
						if (joinerGroup.Success)
						{
							for (int i = 0; i < joinerGroup.Captures.Count; i++)
							{
								string joinerStr = joinerGroup.Captures[i].Value;
								phonemeFS.Add(BuildFeatStruct(match, consComp.Captures[i + 1], "consBase", _consonants, out compStrRep));
								sb.Append(compStrRep);
								FeatureStruct joinerFs = _joiners[joinerStr].FeatureStruct;
								if (joinerFs != null)
									phonemeFS.PriorityUnion(joinerFs);
							}
						}
						else if (consComp.Captures.Count > 1)
						{
							for (int i = 1; i < consComp.Captures.Count; i++)
							{
								phonemeFS.Add(BuildFeatStruct(match, consComp.Captures[i], "consBase", _consonants, out compStrRep));
								sb.Append(compStrRep);
							}
						}
						strRep = sb.ToString();
					}

					phonemeFS.AddValue(CogFeatureSystem.StrRep, strRep);
					phonemeFS.AddValue(CogFeatureSystem.OriginalStrRep, match.Value);
					phonemeFS.AddValue(CogFeatureSystem.Type, CogFeatureSystem.ConsonantType);
					shape.Add(phonemeFS);
				}
				else if (match.Groups["tone"].Success)
				{
					shape.Add(FeatureStruct.New()
						.Symbol(CogFeatureSystem.ToneLetterType)
						.Feature(CogFeatureSystem.StrRep).EqualTo(match.Value)
						.Feature(CogFeatureSystem.OriginalStrRep).EqualTo(match.Value).Value);
				}
				else if (match.Groups["bdry"].Success)
				{
					shape.Add(FeatureStruct.New()
						.Symbol(CogFeatureSystem.BoundaryType)
						.Feature(CogFeatureSystem.StrRep).EqualTo(match.Value)
						.Feature(CogFeatureSystem.OriginalStrRep).EqualTo(match.Value).Value);
				}

				index = match.Index + match.Length;

				if (index == str.Length)
				{
					if (shape.Count == 0)
						break;

					return true;
				}
			}

			return false;
		}

		private bool TryMultipleBaseCharacterSymbol(Match match, SymbolCollection bases, out string strRep, out FeatureStruct fs)
		{
			Group joinerGroup = match.Groups["joiner"];
			Group modGroup = match.Groups["mod"];
			Group consBaseGroup = match.Groups["consBase"];
			Symbol symbol;
			if (joinerGroup.Success && (!modGroup.Success || modGroup.Index >= consBaseGroup.Index + consBaseGroup.Length)
				&& bases.TryGetValue(string.Concat(consBaseGroup.Captures.Cast<Capture>().Select(cap => cap.Value)), out symbol))
			{
				var sb = new StringBuilder();
				sb.Append(symbol.StrRep);
				fs = symbol.FeatureStruct != null ? symbol.FeatureStruct.DeepClone() : new FeatureStruct();
				ApplyModifiers(match.Groups["mod"].Captures.Cast<Capture>(), sb, fs);
				strRep = sb.ToString();
				return true;
			}
			strRep = null;
			fs = null;
			return false;
		}

		private FeatureStruct BuildFeatStruct(Match match, Capture capture, string baseGroupName, SymbolCollection bases, out string strRep)
		{
			string baseStr = match.Groups[baseGroupName].Captures.Cast<Capture>().Single(cap => capture.Index == cap.Index).Value.ToLowerInvariant();
			FeatureStruct baseFs = bases[baseStr].FeatureStruct;
			FeatureStruct fs = baseFs != null ? baseFs.DeepClone() : new FeatureStruct();
			var sb = new StringBuilder();
			sb.Append(baseStr);
			ApplyModifiers(match.Groups["mod"].Captures.Cast<Capture>().Where(cap => capture.Index <= cap.Index && (capture.Index + capture.Length) >= (cap.Index + cap.Length)), sb, fs);
			strRep = sb.ToString();
			return fs;
		}

		private void ApplyModifiers(IEnumerable<Capture> modifiers, StringBuilder sb, FeatureStruct fs)
		{
			var modStrs = new List<string>();
			foreach (Capture modifier in modifiers)
			{
				string modStr = modifier.Value;
				Symbol modInfo = _modifiers[modStr];
				if (modInfo.FeatureStruct != null && !modInfo.FeatureStruct.IsEmpty)
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
			modStrs.OrderBy(str => str).Aggregate(sb, (s, modStr) => s.Append(modStr));
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
			string modStr = CreateSymbolRegexString(_modifiers);
			string joinerStr = CreateSymbolRegexString(_joiners);

			var sb = new StringBuilder();
			AppendBaseSymbolPattern(sb, "cons", _consonants, modStr, joinerStr, _maxConsonantLength);
			AppendBaseSymbolPattern(sb, "vowel", _vowels, modStr, joinerStr, _maxVowelLength);
			AppendSymbolPattern(sb, "tone", _toneLetters);
			AppendSymbolPattern(sb, "bdry", _boundaries);
			return sb.ToString();
		}

		private static void AppendBaseSymbolPattern(StringBuilder sb, string name, SymbolCollection symbols, string modStr, string joinerStr, int maxLength)
		{
			if (symbols.Count > 0)
			{
				if (sb.Length > 0)
					sb.Append("|");
				string baseStr = string.Format("(?'{0}Base'{1})", name, CreateSymbolRegexString(symbols));
				string compStr = !string.IsNullOrEmpty(modStr) ? string.Format("(?'{0}Comp'{1}(?'mod'{2})*)", name, baseStr, modStr) : string.Format("(?'{0}Comp'{1})", name, baseStr);
				sb.AppendFormat("(?'{0}Seg'{1}", name, compStr);
				if (!string.IsNullOrEmpty(joinerStr))
				{
					if (maxLength > 1)
						sb.AppendFormat("(?({0})(?:(?'joiner'{0}){1})*|{1}{{0,{2}}})", joinerStr, compStr, maxLength - 1);
					else
						sb.AppendFormat("(?:(?'joiner'{0}){1})*", joinerStr, compStr);
				}
				else if (maxLength > 1)
				{
					sb.AppendFormat("(?:{0}{{0,{1}}})", compStr, maxLength - 1);
				}
				sb.Append(")");
			}
		}

		private static void AppendSymbolPattern(StringBuilder sb, string name, SymbolCollection symbols)
		{
			if (symbols.Count > 0)
			{
				if (sb.Length > 0)
					sb.Append("|");
				sb.AppendFormat("(?'{0}'{1})", name, CreateSymbolRegexString(symbols));
			}
		}

		private static string CreateSymbolRegexString(IEnumerable<Symbol> symbols)
		{
			var sb = new StringBuilder();
			bool first = true;
			foreach (Symbol symbol in symbols.OrderByDescending(s => s.StrRep.Length))
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
