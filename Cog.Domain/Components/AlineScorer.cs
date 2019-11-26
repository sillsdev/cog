using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Extensions;
using SIL.Machine.Annotations;
using SIL.Machine.DataStructures;
using SIL.Machine.FeatureModel;
using SIL.Machine.NgramModeling;
using SIL.Machine.SequenceAlignment;
using SIL.Machine.Statistics;
using SIL.ObjectModel;

namespace SIL.Cog.Domain.Components
{
	public class AlineScorer : IPairwiseAlignmentScorer<Word, ShapeNode>
	{
		private readonly SegmentPool _segmentPool;
		private readonly SoundClass[] _contextualSoundClasses;

		public AlineScorer(SegmentPool segmentPool, IEnumerable<SymbolicFeature> relevantVowelFeatures,
			IEnumerable<SymbolicFeature> relevantConsFeatures, IDictionary<SymbolicFeature, int> featureWeights,
			IDictionary<FeatureSymbol, int> valueMetrics, IEnumerable<SoundClass> contextualSoundClasses,
			bool soundChangeScoringEnabled, bool syllablePositionCostEnabled)
		{
			_segmentPool = segmentPool;
			RelevantVowelFeatures = new IDBearerSet<SymbolicFeature>(relevantVowelFeatures).ToReadOnlySet();
			RelevantConsonantFeatures = new IDBearerSet<SymbolicFeature>(relevantConsFeatures).ToReadOnlySet();
			FeatureWeights = new Dictionary<SymbolicFeature, int>(featureWeights).ToReadOnlyDictionary();
			ValueMetrics = new Dictionary<FeatureSymbol, int>(valueMetrics).ToReadOnlyDictionary();
			_contextualSoundClasses = contextualSoundClasses.ToArray();
			SoundChangeScoringEnabled = soundChangeScoringEnabled;
			SyllablePositionCostEnabled = syllablePositionCostEnabled;
		}

		public int MaxIndelScore { get; set; } = 0;
		public int MaxSoundChangeScore { get; set; } = 800;
		public int MaxSubstitutionScore { get; set; } = 3500;
		public int MaxExpansionCompressionScore { get; set; } = 4500;
		public int IndelCost { get; set; } = 1000;
		public int VowelCost { get; set; } = 0;
		public int SyllablePositionCost { get; set; } = 500;

		public ReadOnlySet<SymbolicFeature> RelevantVowelFeatures { get; }
		public ReadOnlySet<SymbolicFeature> RelevantConsonantFeatures { get; }
		public ReadOnlyDictionary<SymbolicFeature, int> FeatureWeights { get; }
		public ReadOnlyDictionary<FeatureSymbol, int> ValueMetrics { get; }
		public bool SoundChangeScoringEnabled { get; }
		public bool SyllablePositionCostEnabled { get; }

		public int GetGapPenalty(Word sequence1, Word sequence2)
		{
			return -IndelCost;
		}

		public int GetInsertionScore(Word sequence1, ShapeNode p, Word sequence2, ShapeNode q)
		{
			return MaxIndelScore + GetSoundChangeScore(sequence1, null, p, sequence2, q, null);
		}

		public int GetDeletionScore(Word sequence1, ShapeNode p, Word sequence2, ShapeNode q)
		{
			return MaxIndelScore + GetSoundChangeScore(sequence1, p, null, sequence2, null, q);
		}

		public int GetSubstitutionScore(Word sequence1, ShapeNode p, Word sequence2, ShapeNode q)
		{
			return MaxSubstitutionScore - (Delta(p.Annotation.FeatureStruct, q.Annotation.FeatureStruct)
					+ GetVowelCost(p) + GetVowelCost(q) + GetSyllablePositionCost(p, q))
				+ GetSoundChangeScore(sequence1, p, null, sequence2, q, null);
		}

		public int GetExpansionScore(Word sequence1, ShapeNode p, Word sequence2, ShapeNode q1, ShapeNode q2)
		{
			return MaxExpansionCompressionScore
				- (Delta(p.Annotation.FeatureStruct, q1.Annotation.FeatureStruct)
					+ Delta(p.Annotation.FeatureStruct, q2.Annotation.FeatureStruct) + GetVowelCost(p)
					+ Math.Max(GetVowelCost(q1), GetVowelCost(q2))
					+ Math.Max(GetSyllablePositionCost(p, q1), GetSyllablePositionCost(p, q2)))
				+ GetSoundChangeScore(sequence1, p, null, sequence2, q1, q2);
		}

		public int GetCompressionScore(Word sequence1, ShapeNode p1, ShapeNode p2, Word sequence2, ShapeNode q)
		{
			return MaxExpansionCompressionScore
				- (Delta(p1.Annotation.FeatureStruct, q.Annotation.FeatureStruct)
					+ Delta(p2.Annotation.FeatureStruct, q.Annotation.FeatureStruct) + GetVowelCost(q)
					+ Math.Max(GetVowelCost(p1), GetVowelCost(p2))
					+ Math.Max(GetSyllablePositionCost(p1, q), GetSyllablePositionCost(p2, q)))
				+ GetSoundChangeScore(sequence1, p1, p2, sequence2, q, null);
		}

		public int GetTranspositionScore(Word sequence1, ShapeNode p1, ShapeNode p2, Word sequence2, ShapeNode q1,
			ShapeNode q2)
		{
			return 0;
		}

		public int GetMaxScore1(Word sequence1, ShapeNode p, Word sequence2)
		{
			return GetMaxScore(p) + GetMaxSoundChangeScore(sequence1, p, sequence2);
		}

		public int GetMaxScore2(Word sequence1, Word sequence2, ShapeNode q)
		{
			return GetMaxScore(q) + GetMaxSoundChangeScore(sequence2, q, sequence1);
		}

		private int GetSyllablePositionCost(ShapeNode p1, ShapeNode q1)
		{
			if (!SyllablePositionCostEnabled)
				return 0;

			SymbolicFeatureValue pos1, pos2;
			if (p1.Annotation.FeatureStruct.TryGetValue(CogFeatureSystem.SyllablePosition, out pos1)
				&& q1.Annotation.FeatureStruct.TryGetValue(CogFeatureSystem.SyllablePosition, out pos2))
			{
				return (FeatureSymbol)pos1 == (FeatureSymbol)pos2 ? 0 : SyllablePositionCost;
			}
			return 0;
		}

		private int GetMaxSoundChangeScore(Word word, ShapeNode node, Word otherWord)
		{
			if (!SoundChangeScoringEnabled)
				return 0;

			if (word.Variety == otherWord.Variety)
				return 0;

			VarietyPair varietyPair = word.Variety.VarietyPairs[otherWord.Variety];
			if (varietyPair.CognateSoundCorrespondenceProbabilityDistribution == null)
				return 0;

			double prob;
			if (varietyPair.Variety1 == word.Variety)
			{
				SoundContext lhs = node.ToSoundContext(_segmentPool, _contextualSoundClasses);
				prob = varietyPair.DefaultSoundCorrespondenceProbability;
				IProbabilityDistribution<Ngram<Segment>> probDist;
				if (varietyPair.CognateSoundCorrespondenceProbabilityDistribution.TryGetProbabilityDistribution(lhs,
					out probDist) && probDist.Samples.Count > 0)
				{
					prob = probDist.Samples.Max(nseg => probDist[nseg]);
				}
			}
			else
			{
				Ngram<Segment> corr = _segmentPool.GetExisting(node);
				prob = varietyPair.CognateSoundCorrespondenceProbabilityDistribution.Conditions.Count == 0 ? 0
					: varietyPair.CognateSoundCorrespondenceProbabilityDistribution.Conditions
						.Max(lhs => varietyPair.CognateSoundCorrespondenceProbabilityDistribution[lhs][corr]);
			}
			return (int)(MaxSoundChangeScore * prob);
		}

		private int GetMaxScore(ShapeNode node)
		{
			return MaxSubstitutionScore - (GetVowelCost(node) * 2);
		}

		public int Delta(FeatureStruct fs1, FeatureStruct fs2)
		{
			IEnumerable<SymbolicFeature> features =
				((FeatureSymbol)fs1.GetValue(CogFeatureSystem.Type)) == CogFeatureSystem.VowelType
					&& ((FeatureSymbol)fs2.GetValue(CogFeatureSystem.Type)) == CogFeatureSystem.VowelType
				? RelevantVowelFeatures : RelevantConsonantFeatures;

			return features.Aggregate(0, (val, feat) => val + (Diff(fs1, fs2, feat) * FeatureWeights[feat]));
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

			FeatureSymbol[] values1 = pValue == null ? feature.PossibleSymbols.ToArray() : pValue.Values.ToArray();
			FeatureSymbol[] values2 = qValue == null ? feature.PossibleSymbols.ToArray() : qValue.Values.ToArray();
			if (values2.Length > values1.Length)
			{
				FeatureSymbol[] temp = values1;
				values1 = values2;
				values2 = temp;
			}
			return (int)Math.Round(values1.Average(s1 => values2
				.Min(s2 => Math.Abs(ValueMetrics[s1] - ValueMetrics[s2]))));
		}

		private int GetVowelCost(ShapeNode node)
		{
			return node.Annotation.Type() == CogFeatureSystem.VowelType ? VowelCost : 0;
		}

		private int GetSoundChangeScore(Word sequence1, ShapeNode p1, ShapeNode p2, Word sequence2, ShapeNode q1,
			ShapeNode q2)
		{
			if (!SoundChangeScoringEnabled)
				return 0;

			if (sequence1.Variety == sequence2.Variety)
				return 0;

			VarietyPair varietyPair = sequence1.Variety.VarietyPairs[sequence2.Variety];

			if (varietyPair.CognateSoundCorrespondenceProbabilityDistribution == null)
				return 0;

			if (sequence1.Variety == varietyPair.Variety2)
			{
				ShapeNode tempNode = p1;
				p1 = q1;
				q1 = tempNode;

				tempNode = p2;
				p2 = q2;
				q2 = tempNode;
			}

			Ngram<Segment> target;
			if (p1 == null)
			{
				target = new Ngram<Segment>();
			}
			else
			{
				Segment targetSegment = _segmentPool.GetExisting(p1);
				target = p2 == null ? targetSegment : new Ngram<Segment>(targetSegment, _segmentPool.GetExisting(p2));
			}

			Ngram<Segment> corr;
			if (q1 == null)
			{
				corr = new Ngram<Segment>();
			}
			else
			{
				Segment corrSegment = _segmentPool.GetExisting(q1);
				corr = q2 == null ? corrSegment : new Ngram<Segment>(corrSegment, _segmentPool.GetExisting(q2));
			}

			ShapeNode leftNode = p1 == null ? p2 : p1.GetPrev(NodeFilter);
			if (leftNode == null
				|| !_contextualSoundClasses.TryGetMatchingSoundClass(_segmentPool, leftNode, out SoundClass leftEnv))
			{
				leftEnv = null;
			}
			ShapeNode pRight = p2 ?? p1;
			ShapeNode rightNode = pRight == null ? null : pRight.GetNext(NodeFilter);
			if (rightNode == null
				|| !_contextualSoundClasses.TryGetMatchingSoundClass(_segmentPool, rightNode, out SoundClass rightEnv))
			{
				rightEnv = null;
			}

			var lhs = new SoundContext(leftEnv, target, rightEnv);
			double prob = varietyPair.CognateSoundCorrespondenceProbabilityDistribution
				.TryGetProbabilityDistribution(lhs, out IProbabilityDistribution<Ngram<Segment>> probDist)
					? probDist[corr]
					: varietyPair.DefaultSoundCorrespondenceProbability;
			return (int)(MaxSoundChangeScore * prob);
		}

		private static bool NodeFilter(ShapeNode node)
		{
			return node.Type().IsOneOf(CogFeatureSystem.ConsonantType, CogFeatureSystem.VowelType,
				CogFeatureSystem.AnchorType);
		}
	}
}
