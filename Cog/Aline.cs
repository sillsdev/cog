using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class Aline : EditDistance
	{
		public const int Precision = 100;
		private const int MaxSubstitutionScore = 35 * Precision;
		private const int MaxExpansionCompressionScore = 45 * Precision;
		private const int IndelCost = 10 * Precision;
		private const int VowelCost = 0 * Precision;

		private readonly IDBearerSet<SymbolicFeature> _relevantVowelFeatures;
		private readonly IDBearerSet<SymbolicFeature> _relevantConsFeatures;

		public Aline(SpanFactory<ShapeNode> spanFactory, IEnumerable<SymbolicFeature> relevantVowelFeatures,
			IEnumerable<SymbolicFeature> relevantConsFeatures)
			: base(spanFactory)
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

		public override int SigmaInsertion(WordPair wordPair, ShapeNode q)
		{
			return -IndelCost;
		}

		public override int SigmaDeletion(WordPair wordPair, ShapeNode p)
		{
			return -IndelCost;
		}

		public override int SigmaSubstitution(WordPair wordPair, ShapeNode p, ShapeNode q)
		{
			return MaxSubstitutionScore - (Delta(p.Annotation.FeatureStruct, q.Annotation.FeatureStruct) + V(p) + V(q));
		}

		public override int SigmaExpansion(WordPair wordPair, ShapeNode p, ShapeNode q1, ShapeNode q2)
		{
			return MaxExpansionCompressionScore - (Delta(p.Annotation.FeatureStruct, q1.Annotation.FeatureStruct) + Delta(p.Annotation.FeatureStruct, q2.Annotation.FeatureStruct) + V(p) + Math.Max(V(q1), V(q2)));
		}

		public override int SigmaCompression(WordPair wordPair, ShapeNode p1, ShapeNode p2, ShapeNode q)
		{
			return MaxExpansionCompressionScore - (Delta(p1.Annotation.FeatureStruct, q.Annotation.FeatureStruct) + Delta(p2.Annotation.FeatureStruct, q.Annotation.FeatureStruct) + V(q) + Math.Max(V(p1), V(p2)));
		}

		private int V(ShapeNode node)
		{
			return node.Annotation.Type == CogFeatureSystem.VowelType ? VowelCost : 0;
		}

		public override int Delta(FeatureStruct fs1, FeatureStruct fs2)
		{
			IEnumerable<SymbolicFeature> features = ((string) fs1.GetValue(AnnotationFeatureSystem.Type)) == CogFeatureSystem.VowelType
				&& ((string) fs2.GetValue(AnnotationFeatureSystem.Type)) == CogFeatureSystem.VowelType
				? _relevantVowelFeatures : _relevantConsFeatures;

			return features.Aggregate(0, (val, feat) => val + (Diff(fs1, fs2, feat) * (int) feat.Weight));
		}

		public override int GetMaxScore(WordPair wordPair, ShapeNode node)
		{
			return MaxSubstitutionScore - (V(node) * 2);
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
