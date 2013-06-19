using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Components
{
	public class Aline : WordPairAlignerBase
	{
		private const int MaxSoundChangeScore = 2000;
		private const int MaxSubstitutionScore = 3500;
		private const int MaxExpansionCompressionScore = 4500;
		private const int IndelCost = 1000;
		private const int VowelCost = 0;

		private readonly IDBearerSet<SymbolicFeature> _relevantConsFeatures;
		private readonly IDBearerSet<SymbolicFeature> _relevantVowelFeatures; 
		private readonly Dictionary<SymbolicFeature, int> _featureWeights;
		private readonly Dictionary<FeatureSymbol, int> _valueMetrics; 

		public Aline(SpanFactory<ShapeNode> spanFactory, IEnumerable<SymbolicFeature> relevantVowelFeatures, IEnumerable<SymbolicFeature> relevantConsFeatures,
			IDictionary<SymbolicFeature, int> featureWeights, IDictionary<FeatureSymbol, int> valueMetrics)
			: this(spanFactory, relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics, new WordPairAlignerSettings())
		{
		}

		public Aline(SpanFactory<ShapeNode> spanFactory, IEnumerable<SymbolicFeature> relevantVowelFeatures, IEnumerable<SymbolicFeature> relevantConsFeatures,
			IDictionary<SymbolicFeature, int> featureWeights, IDictionary<FeatureSymbol, int> valueMetrics, WordPairAlignerSettings settings)
			: base(spanFactory, settings)
		{
			_relevantVowelFeatures = new IDBearerSet<SymbolicFeature>(relevantVowelFeatures);
			_relevantConsFeatures = new IDBearerSet<SymbolicFeature>(relevantConsFeatures);
			_featureWeights = new Dictionary<SymbolicFeature, int>(featureWeights);
			_valueMetrics = new Dictionary<FeatureSymbol, int>(valueMetrics);
		}

		public IReadOnlySet<SymbolicFeature> RelevantVowelFeatures
		{
			get { return _relevantVowelFeatures.AsReadOnlySet(); }
		}

		public IReadOnlySet<SymbolicFeature> RelevantConsonantFeatures
		{
			get { return _relevantConsFeatures.AsReadOnlySet(); }
		} 

		public IReadOnlyDictionary<SymbolicFeature, int> FeatureWeights
		{
			get { return _featureWeights.AsReadOnlyDictionary(); }
		}

		public IReadOnlyDictionary<FeatureSymbol, int> ValueMetrics
		{
			get { return _valueMetrics.AsReadOnlyDictionary(); }
		}

		protected override int GetInsertionScore(VarietyPair varietyPair, ShapeNode p, ShapeNode q)
		{
			return -IndelCost + SoundChange(varietyPair, null, p, q, null);
		}

		protected override int GetDeletionScore(VarietyPair varietyPair, ShapeNode p, ShapeNode q)
		{
			return -IndelCost + SoundChange(varietyPair, p, null, null, null);
		}

		protected override int GetSubstitutionScore(VarietyPair varietyPair, ShapeNode p, ShapeNode q)
		{
			return (MaxSubstitutionScore - (Delta(p.Annotation.FeatureStruct, q.Annotation.FeatureStruct) + V(p) + V(q))) + SoundChange(varietyPair, p, null, q, null);
		}

		protected override int GetExpansionScore(VarietyPair varietyPair, ShapeNode p, ShapeNode q1, ShapeNode q2)
		{
			return (MaxExpansionCompressionScore - (Delta(p.Annotation.FeatureStruct, q1.Annotation.FeatureStruct) + Delta(p.Annotation.FeatureStruct, q2.Annotation.FeatureStruct) + V(p) + Math.Max(V(q1), V(q2))))
				+ SoundChange(varietyPair, p, null, q1, q2);
		}

		protected override int GetCompressionScore(VarietyPair varietyPair, ShapeNode p1, ShapeNode p2, ShapeNode q)
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

			return features.Aggregate(0, (val, feat) => val + (Diff(fs1, fs2, feat) * _featureWeights[feat]));
		}

		protected override int GetMaxScore1(VarietyPair varietyPair, ShapeNode p)
		{
			int maxScore = GetMaxScore(p);
			if (varietyPair.SoundChangeProbabilityDistribution != null)
			{
				SoundContext lhs = p.ToSoundContext(varietyPair.Variety1, ContextualSoundClasses);
				double prob = varietyPair.DefaultCorrespondenceProbability;
				IProbabilityDistribution<Ngram> probDist;
				if (varietyPair.SoundChangeProbabilityDistribution.TryGetProbabilityDistribution(lhs, out probDist) && probDist.Samples.Count > 0)
					prob = probDist.Samples.Max(nseg => probDist[nseg]);
				maxScore += (int) (MaxSoundChangeScore * prob);
			}
			return maxScore;
		}

		protected override int GetMaxScore2(VarietyPair varietyPair, ShapeNode q)
		{
			int maxScore = GetMaxScore(q);
			if (varietyPair.SoundChangeProbabilityDistribution != null)
			{
				Ngram corr = q.ToNgram(varietyPair.Variety2);

				double prob = varietyPair.SoundChangeProbabilityDistribution.Conditions.Max(lhs => varietyPair.SoundChangeProbabilityDistribution[lhs][corr]);
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
			if (varietyPair.SoundChangeProbabilityDistribution == null)
				return 0;

			Ngram target;
			if (p1 == null)
			{
				target = new Ngram(Segment.Null);
			}
			else
			{
				Segment targetSegment = varietyPair.Variety1.Segments[p1];
				target = p2 == null ? new Ngram(targetSegment) : new Ngram(targetSegment, varietyPair.Variety1.Segments[p2]);
			}

			Ngram corr;
			if (q1 == null && q2 == null)
			{
				corr = new Ngram(Segment.Null);
			}
			else
			{
				Segment corrSegment = varietyPair.Variety2.Segments[q1];
				corr = q2 == null ? new Ngram(corrSegment) : new Ngram(corrSegment, varietyPair.Variety2.Segments[q2]);
			}

			ShapeNode leftNode = p1 == null ? p2 : p1.GetPrev(NodeFilter);
			SoundClass leftEnv = leftNode == null ? null : leftNode.GetMatchingSoundClass(varietyPair.Variety1, ContextualSoundClasses);
			ShapeNode pRight = p2 ?? p1;
			ShapeNode rightNode = pRight == null ? null : pRight.GetNext(NodeFilter);
			SoundClass rightEnv = rightNode == null ? null : rightNode.GetMatchingSoundClass(varietyPair.Variety1, ContextualSoundClasses);

			var lhs = new SoundContext(leftEnv, target, rightEnv);
			IProbabilityDistribution<Ngram> probDist;
			double prob = varietyPair.SoundChangeProbabilityDistribution.TryGetProbabilityDistribution(lhs, out probDist) ? probDist[corr]
				: varietyPair.DefaultCorrespondenceProbability;
			return (int) (MaxSoundChangeScore * prob);
		}

		private static bool NodeFilter(ShapeNode node)
		{
			return node.Type().IsOneOf(CogFeatureSystem.ConsonantType, CogFeatureSystem.VowelType, CogFeatureSystem.AnchorType);
		}

		private int Diff(FeatureStruct fs1, FeatureStruct fs2, SymbolicFeature feature)
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
				return qValue.Values.Min(symbol => _valueMetrics[symbol]);

			if (qValue == null)
				return pValue.Values.Min(symbol => _valueMetrics[symbol]);

			int min = -1;
			foreach (FeatureSymbol pSymbol in pValue.Values)
			{
				foreach (FeatureSymbol qSymbol in qValue.Values)
				{
					int diff = Math.Abs(_valueMetrics[pSymbol] - _valueMetrics[qSymbol]);
					if (min == -1 || diff < min)
						min = diff;
				}
			}

			return min;
		}
	}
}
