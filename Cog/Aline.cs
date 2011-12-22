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
			return MaxSubstitutionScore - (Delta(p, q) + V(p) + V(q));
		}

		public override int SigmaExpansion(WordPair wordPair, ShapeNode p, ShapeNode q1, ShapeNode q2)
		{
			return MaxExpansionCompressionScore - (Delta(p, q1) + Delta(p, q2) + V(p) + Math.Max(V(q1), V(q2)));
		}

		public override int SigmaCompression(WordPair wordPair, ShapeNode p1, ShapeNode p2, ShapeNode q)
		{
			return MaxExpansionCompressionScore - (Delta(p1, q) + Delta(p2, q) + V(q) + Math.Max(V(p1), V(p2)));
		}

		private int V(ShapeNode node)
		{
			return node.Annotation.Type == CogFeatureSystem.VowelType ? VowelCost : 0;
		}

		public override int Delta(ShapeNode p, ShapeNode q)
		{
			IEnumerable<SymbolicFeature> features = p.Annotation.Type == CogFeatureSystem.VowelType && q.Annotation.Type == CogFeatureSystem.VowelType
				? _relevantVowelFeatures : _relevantConsFeatures;

			return features.Aggregate(0, (val, feat) => val + (Diff(p, q, feat) * (int) feat.Weight));
		}

		private static int Diff(ShapeNode p, ShapeNode q, SymbolicFeature feature)
		{
			SymbolicFeatureValue pValue;
			if (!p.Annotation.FeatureStruct.TryGetValue(feature, out pValue))
				pValue = null;
			SymbolicFeatureValue qValue;
			if (!q.Annotation.FeatureStruct.TryGetValue(feature, out qValue))
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
