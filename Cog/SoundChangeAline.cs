using System.Collections.Generic;
using System.Linq;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class SoundChangeAline : Aline
	{
		private const int MaxSoundChangeScore = 20 * Precision;

		private readonly IDBearerSet<NaturalClass> _naturalClasses;

		public SoundChangeAline(SpanFactory<ShapeNode> spanFactory, IEnumerable<SymbolicFeature> relevantVowelFeatures,
			IEnumerable<SymbolicFeature> relevantConsFeatures, IEnumerable<NaturalClass> naturalClasses)
			: base(spanFactory, relevantVowelFeatures, relevantConsFeatures)
		{
			_naturalClasses = new IDBearerSet<NaturalClass>(naturalClasses);
		}

		public IEnumerable<NaturalClass> NaturalClasses
		{
			get { return _naturalClasses; }
		}

		public override int SigmaSubstitution(WordPair wordPair, ShapeNode p, ShapeNode q)
		{
			return base.SigmaSubstitution(wordPair, p, q) + SoundChange(wordPair, p, null, q, null);
		}

		public override int SigmaExpansion(WordPair wordPair, ShapeNode p, ShapeNode q1, ShapeNode q2)
		{
			return base.SigmaExpansion(wordPair, p, q1, q2) + SoundChange(wordPair, p, null, q1, q2);
		}

		public override int SigmaCompression(WordPair wordPair, ShapeNode p1, ShapeNode p2, ShapeNode q)
		{
			return base.SigmaCompression(wordPair, p1, p2, q) + SoundChange(wordPair, p1, p2, q, null);
		}

		public override int GetMaxScore(WordPair wordPair, ShapeNode node)
		{
			return base.GetMaxScore(wordPair, node) + (int) (MaxSoundChangeScore * 0.85);
		}

		private int SoundChange(WordPair wordPair, ShapeNode p1, ShapeNode p2, ShapeNode q1, ShapeNode q2)
		{
			Phoneme targetPhoneme = wordPair.VarietyPair.Variety1.GetPhoneme(p1);
			NPhone target = p2 == null ? new NPhone(targetPhoneme) : new NPhone(targetPhoneme, wordPair.VarietyPair.Variety1.GetPhoneme(p2));

			Phoneme corrPhoneme = wordPair.VarietyPair.Variety2.GetPhoneme(q1);
			NPhone corr = q2 == null ? new NPhone(corrPhoneme) : new NPhone(corrPhoneme, wordPair.VarietyPair.Variety2.GetPhoneme(q2));

			NaturalClass leftEnv = _naturalClasses.FirstOrDefault(constraint =>
				constraint.FeatureStruct.IsUnifiable(p1.GetPrev(node => node.Annotation.Type() != CogFeatureSystem.NullType).Annotation.FeatureStruct));
			NaturalClass rightEnv = _naturalClasses.FirstOrDefault(constraint =>
				constraint.FeatureStruct.IsUnifiable((p2 ?? p1).GetNext(node => node.Annotation.Type() != CogFeatureSystem.NullType).Annotation.FeatureStruct));
			SoundChange soundChange;
			double prob = wordPair.VarietyPair.TryGetSoundChange(leftEnv, target, rightEnv, out soundChange) ? soundChange[corr]
				: wordPair.VarietyPair.DefaultCorrespondenceProbability;
			return (int) (MaxSoundChangeScore * prob);
		}
	}
}
