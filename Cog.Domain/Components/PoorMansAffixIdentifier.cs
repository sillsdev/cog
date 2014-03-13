using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;
using SIL.Machine.Morphology;
using SIL.Machine.NgramModeling;

namespace SIL.Cog.Domain.Components
{
	public class PoorMansAffixIdentifier : IProcessor<Variety>
	{
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private readonly SegmentPool _segmentPool;
		private readonly PoorMansAffixIdentifier<Word, Segment> _affixIdentifier; 

		public PoorMansAffixIdentifier(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, double threshold, int maxAffixLength)
		{
			_spanFactory = spanFactory;
			_segmentPool = segmentPool;
			_affixIdentifier = new PoorMansAffixIdentifier<Word, Segment>(word => word.Stem.Children.Where(ann => ann.Type() == CogFeatureSystem.SyllableType)
			    .Select(ann => word.Shape.GetNodes(ann.Span).Where(n => n.Type().IsOneOf(CogFeatureSystem.ConsonantType, CogFeatureSystem.VowelType)).Select(n => _segmentPool.Get(n))))
				{
					MaxAffixLength = maxAffixLength,
					Threshold = threshold
				};
		}

		public double Threshold
		{
			get { return _affixIdentifier.Threshold; }
		}

		public int MaxAffixLength
		{
			get { return _affixIdentifier.MaxAffixLength; }
		}

		public void Process(Variety variety)
		{
			if (variety.Affixes.Count == 0)
			{
				Word[] noCategoryWords = variety.Words.Where(w => string.IsNullOrEmpty(w.Meaning.Category) && w.Shape.Count > 0).ToArray();
				if (noCategoryWords.Length >= 5)
					variety.Affixes.AddRange(IdentifyAffixes(noCategoryWords, null));
				foreach (IGrouping<string, Word> categoryGroup in variety.Words.Where(w => !string.IsNullOrEmpty(w.Meaning.Category) && w.Shape.Count > 0).GroupBy(w => w.Meaning.Category))
				{
					Word[] allWords = categoryGroup.Concat(noCategoryWords).ToArray();
					if (allWords.Length >= 5)
						variety.Affixes.AddRange(IdentifyAffixes(allWords, categoryGroup.Key));
				}
			}
		}

		private IEnumerable<Affix> IdentifyAffixes(Word[] words, string category)
		{
			foreach (Affix<Segment> affix in _affixIdentifier.IdentifyAffixes(words, AffixType.Prefix))
			{
				var ngram = new Ngram<Segment>(Segment.Anchor.ToEnumerable().Concat(affix.Ngram));
				yield return CreateAffix(ngram, category, affix.Score);
			}

			foreach (Affix<Segment> affix in _affixIdentifier.IdentifyAffixes(words, AffixType.Suffix))
			{
				var ngram = new Ngram<Segment>(affix.Ngram.SkipWhile(seg => seg.Type == CogFeatureSystem.ToneLetterType).Concat(Segment.Anchor));
				if (ngram.Length > 1)
					yield return CreateAffix(ngram, category, affix.Score);
			}
		}

		private Affix CreateAffix(Ngram<Segment> ngram, string category, double score)
		{
			var shape = new Shape(_spanFactory,
				begin => new ShapeNode(_spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Feature(CogFeatureSystem.StrRep).EqualTo("#").Value));
			foreach (Segment seg in ngram)
			{
				if (seg.Type != CogFeatureSystem.AnchorType)
					shape.Add(seg.FeatureStruct);
			}
			shape.Freeze();
			return new Affix(string.Concat(shape.Select(n => n.StrRep())), ngram.First.Equals(Segment.Anchor) ? AffixType.Prefix : AffixType.Suffix,
				category) {Shape = shape, Score = score};
		}
	}
}
