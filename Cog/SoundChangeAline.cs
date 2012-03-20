using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
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
			: this(spanFactory, relevantVowelFeatures, relevantConsFeatures, naturalClasses, new EditDistanceSettings())
		{
		}

		public SoundChangeAline(SpanFactory<ShapeNode> spanFactory, IEnumerable<SymbolicFeature> relevantVowelFeatures,
			IEnumerable<SymbolicFeature> relevantConsFeatures, IEnumerable<NaturalClass> naturalClasses, EditDistanceSettings settings)
			: base(spanFactory, relevantVowelFeatures, relevantConsFeatures, settings)
		{
			_naturalClasses = new IDBearerSet<NaturalClass>(naturalClasses);
		}

		public IEnumerable<NaturalClass> NaturalClasses
		{
			get { return _naturalClasses; }
		}

		public override int SigmaDeletion(VarietyPair varietyPair, ShapeNode p)
		{
			return base.SigmaDeletion(varietyPair, p) + SoundChange(varietyPair, p, null, null, null);
		}

		public override int SigmaInsertion(VarietyPair varietyPair, ShapeNode q)
		{
			return base.SigmaInsertion(varietyPair, q) + SoundChange(varietyPair, null, null, q, null);
		}

		public override int SigmaSubstitution(VarietyPair varietyPair, ShapeNode p, ShapeNode q)
		{
			return base.SigmaSubstitution(varietyPair, p, q) + SoundChange(varietyPair, p, null, q, null);
		}

		public override int SigmaExpansion(VarietyPair varietyPair, ShapeNode p, ShapeNode q1, ShapeNode q2)
		{
			return base.SigmaExpansion(varietyPair, p, q1, q2) + SoundChange(varietyPair, p, null, q1, q2);
		}

		public override int SigmaCompression(VarietyPair varietyPair, ShapeNode p1, ShapeNode p2, ShapeNode q)
		{
			return base.SigmaCompression(varietyPair, p1, p2, q) + SoundChange(varietyPair, p1, p2, q, null);
		}

		public override int GetMaxScore(VarietyPair varietyPair, ShapeNode node)
		{
			return base.GetMaxScore(varietyPair, node) + (int) (MaxSoundChangeScore * 0.85);
		}

		private int SoundChange(VarietyPair varietyPair, ShapeNode p1, ShapeNode p2, ShapeNode q1, ShapeNode q2)
		{
			NSegment target;
			if (p1 == null && p2 == null)
			{
				target = new NSegment(Segment.Null);
			}
			else
			{
				Segment targetSegment = varietyPair.Variety1.GetSegment(p1);
				target = p2 == null ? new NSegment(targetSegment) : new NSegment(targetSegment, varietyPair.Variety1.GetSegment(p2));
			}

			NSegment corr;
			if (q1 == null && q2 == null)
			{
				corr = new NSegment(Segment.Null);
			}
			else
			{
				Segment corrSegment = varietyPair.Variety2.GetSegment(q1);
				corr = q2 == null ? new NSegment(corrSegment) : new NSegment(corrSegment, varietyPair.Variety2.GetSegment(q2));
			}

			NaturalClass leftEnv = _naturalClasses.FirstOrDefault(constraint =>
				constraint.FeatureStruct.IsUnifiable(p1.GetPrev(node => node.Annotation.Type() != CogFeatureSystem.NullType).Annotation.FeatureStruct));
			NaturalClass rightEnv = _naturalClasses.FirstOrDefault(constraint =>
				constraint.FeatureStruct.IsUnifiable((p2 ?? p1).GetNext(node => node.Annotation.Type() != CogFeatureSystem.NullType).Annotation.FeatureStruct));
			SoundChange soundChange;
			double prob = varietyPair.TryGetSoundChange(leftEnv, target, rightEnv, out soundChange) ? soundChange[corr]
				: varietyPair.DefaultCorrespondenceProbability;
			return (int) (MaxSoundChangeScore * prob);
		}
	}
}
