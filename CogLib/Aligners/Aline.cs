using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Aligners
{
	public class Aline : AlignerBase
	{
		public const int Precision = 100;
		private const int MaxSoundChangeScore = 20 * Precision;
		private const int MaxSubstitutionScore = 35 * Precision;
		private const int MaxExpansionCompressionScore = 45 * Precision;
		private const int IndelCost = 10 * Precision;
		private const int VowelCost = 0 * Precision;

		private readonly IDBearerSet<SymbolicFeature> _relevantVowelFeatures;
		private readonly IDBearerSet<SymbolicFeature> _relevantConsFeatures;

		public Aline(SpanFactory<ShapeNode> spanFactory, IEnumerable<SymbolicFeature> relevantVowelFeatures,
			IEnumerable<SymbolicFeature> relevantConsFeatures)
			: this(spanFactory, relevantVowelFeatures, relevantConsFeatures, new AlignerSettings())
		{
		}

		public Aline(SpanFactory<ShapeNode> spanFactory, IEnumerable<SymbolicFeature> relevantVowelFeatures,
			IEnumerable<SymbolicFeature> relevantConsFeatures, AlignerSettings settings)
			: base(spanFactory, settings)
		{
			_relevantVowelFeatures = new IDBearerSet<SymbolicFeature>(relevantVowelFeatures);
			_relevantConsFeatures = new IDBearerSet<SymbolicFeature>(relevantConsFeatures);
		}

		public IEnumerable<SymbolicFeature> RelevantVowelFeatures
		{
			get { return _relevantVowelFeatures; }
		}

		public IEnumerable<SymbolicFeature> RelevantConsonantFeatures
		{
			get { return _relevantConsFeatures; }
		}

		public override int SigmaInsertion(VarietyPair varietyPair, ShapeNode q)
		{
			return -IndelCost + SoundChange(varietyPair, null, null, q, null);
		}

		public override int SigmaDeletion(VarietyPair varietyPair, ShapeNode p)
		{
			return -IndelCost + SoundChange(varietyPair, p, null, null, null);
		}

		public override int SigmaSubstitution(VarietyPair varietyPair, ShapeNode p, ShapeNode q)
		{
			return (MaxSubstitutionScore - (Delta(p.Annotation.FeatureStruct, q.Annotation.FeatureStruct) + V(p) + V(q))) + SoundChange(varietyPair, p, null, q, null);
		}

		public override int SigmaExpansion(VarietyPair varietyPair, ShapeNode p, ShapeNode q1, ShapeNode q2)
		{
			return (MaxExpansionCompressionScore - (Delta(p.Annotation.FeatureStruct, q1.Annotation.FeatureStruct) + Delta(p.Annotation.FeatureStruct, q2.Annotation.FeatureStruct) + V(p) + Math.Max(V(q1), V(q2))))
				+ SoundChange(varietyPair, p, null, q1, q2);
		}

		public override int SigmaCompression(VarietyPair varietyPair, ShapeNode p1, ShapeNode p2, ShapeNode q)
		{
			return (MaxExpansionCompressionScore - (Delta(p1.Annotation.FeatureStruct, q.Annotation.FeatureStruct) + Delta(p2.Annotation.FeatureStruct, q.Annotation.FeatureStruct) + V(q) + Math.Max(V(p1), V(p2))))
				+ SoundChange(varietyPair, p1, p2, q, null);
		}

		private int V(ShapeNode node)
		{
			return node.Annotation.Type() == CogFeatureSystem.VowelType ? VowelCost : 0;
		}

		public override int Delta(FeatureStruct fs1, FeatureStruct fs2)
		{
			IEnumerable<SymbolicFeature> features = ((FeatureSymbol) fs1.GetValue(CogFeatureSystem.Type)) == CogFeatureSystem.VowelType
				&& ((FeatureSymbol) fs2.GetValue(CogFeatureSystem.Type)) == CogFeatureSystem.VowelType
				? _relevantVowelFeatures : _relevantConsFeatures;

			return features.Aggregate(0, (val, feat) => val + (Diff(fs1, fs2, feat) * (int) feat.Weight));
		}

		public override int GetMaxScore1(VarietyPair varietyPair, ShapeNode p)
		{
			int maxScore = GetMaxScore(p);
			if (varietyPair.SoundChanges.Count > 0)
			{
				var target = new NSegment(varietyPair.Variety1.Segments[p]);
				NaturalClass leftEnv = NaturalClasses.FirstOrDefault(constraint =>
					constraint.FeatureStruct.IsUnifiable(p.GetPrev(node => node.Annotation.Type() != CogFeatureSystem.NullType).Annotation.FeatureStruct));
				NaturalClass rightEnv = NaturalClasses.FirstOrDefault(constraint =>
					constraint.FeatureStruct.IsUnifiable(p.GetNext(node => node.Annotation.Type() != CogFeatureSystem.NullType).Annotation.FeatureStruct));

				var lhs = new SoundChangeLhs(leftEnv, target, rightEnv);
				double prob = varietyPair.SoundChanges.DefaultCorrespondenceProbability;
				SoundChange soundChange;
				if (varietyPair.SoundChanges.TryGetValue(lhs, out soundChange) && soundChange.ObservedCorrespondences.Count > 0)
					prob = soundChange.ObservedCorrespondences.Max(nseg => soundChange[nseg]);
				maxScore += (int) (MaxSoundChangeScore * prob);
			}
			return maxScore;
		}

		public override int GetMaxScore2(VarietyPair varietyPair, ShapeNode q)
		{
			int maxScore = GetMaxScore(q);
			if (varietyPair.SoundChanges.Count > 0)
			{
				var corr = new NSegment(varietyPair.Variety2.Segments[q]);

				double prob = varietyPair.SoundChanges.Max(soundChange => soundChange[corr]);
				maxScore += (int) (MaxSoundChangeScore * prob);
			}
			return maxScore;
		}

		private int GetMaxScore(ShapeNode node)
		{
			return MaxSubstitutionScore - (V(node) * 2);
		}

		private int SoundChange(VarietyPair varietyPair, ShapeNode p1, ShapeNode p2, ShapeNode q1, ShapeNode q2)
		{
			if (varietyPair.SoundChanges.Count == 0)
				return 0;

			NSegment target;
			if (p1 == null && p2 == null)
			{
				target = new NSegment(Segment.Null);
			}
			else
			{
				Segment targetSegment = varietyPair.Variety1.Segments[p1];
				target = p2 == null ? new NSegment(targetSegment) : new NSegment(targetSegment, varietyPair.Variety1.Segments[p2]);
			}

			NSegment corr;
			if (q1 == null && q2 == null)
			{
				corr = new NSegment(Segment.Null);
			}
			else
			{
				Segment corrSegment = varietyPair.Variety2.Segments[q1];
				corr = q2 == null ? new NSegment(corrSegment) : new NSegment(corrSegment, varietyPair.Variety2.Segments[q2]);
			}

			NaturalClass leftEnv = NaturalClasses.FirstOrDefault(constraint =>
				constraint.FeatureStruct.IsUnifiable(p1.GetPrev(node => node.Annotation.Type() != CogFeatureSystem.NullType).Annotation.FeatureStruct));
			NaturalClass rightEnv = NaturalClasses.FirstOrDefault(constraint =>
				constraint.FeatureStruct.IsUnifiable((p2 ?? p1).GetNext(node => node.Annotation.Type() != CogFeatureSystem.NullType).Annotation.FeatureStruct));

			var lhs = new SoundChangeLhs(leftEnv, target, rightEnv);
			SoundChange soundChange;
			double prob = varietyPair.SoundChanges.TryGetValue(lhs, out soundChange) ? soundChange[corr]
				: varietyPair.SoundChanges.DefaultCorrespondenceProbability;
			return (int) (MaxSoundChangeScore * prob);
		}

		private static int Diff(FeatureStruct fs1, FeatureStruct fs2, SymbolicFeature feature)
		{
			SymbolicFeatureValue pValue;
			if (!fs1.TryGetValue(feature, out pValue))
				pValue = null;
			SymbolicFeatureValue qValue;
			if (!fs2.TryGetValue(feature, out qValue))
				qValue = null;

			if (pValue == null && qValue == null)
				return 0;

			if (pValue == null)
				return (int)qValue.Values.MinBy(symbol => symbol.Weight).Weight;

			if (qValue == null)
				return (int)pValue.Values.MinBy(symbol => symbol.Weight).Weight;

			int min = -1;
			foreach (FeatureSymbol pSymbol in pValue.Values)
			{
				foreach (FeatureSymbol qSymbol in qValue.Values)
				{
					int diff = Math.Abs((int)pSymbol.Weight - (int)qSymbol.Weight);
					if (min == -1 || diff < min)
						min = diff;
				}
			}

			return min;
		}
	}
}
