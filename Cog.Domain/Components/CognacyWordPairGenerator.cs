using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SIL.Extensions;
using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;
using SIL.Machine.SequenceAlignment;
using SIL.Machine.Statistics;

namespace SIL.Cog.Domain.Components
{
	public class CognacyWordPairGenerator : IProcessor<VarietyPair>
	{
		private readonly CogProject _project;
		private readonly string _alignerID;
		private readonly SegmentPool _segmentPool;
		private readonly string _cognateIdentifierID;
		private readonly ThresholdCognateIdentifier _thresholdCognateIdentifier;

		public CognacyWordPairGenerator(SegmentPool segmentPool, CogProject project, double initialAlignmentThreshold, string alignerID, string cognateIdentifierID)
		{
			_project = project;
			_alignerID = alignerID;
			_segmentPool = segmentPool;
			_cognateIdentifierID = cognateIdentifierID;
			_thresholdCognateIdentifier = new ThresholdCognateIdentifier(initialAlignmentThreshold);
		}

		public double InitialAlignmentThreshold
		{
			get { return _thresholdCognateIdentifier.Threshold; }
		}

		public string AlignerID
		{
			get { return _alignerID; }
		}

		public string CognateIdentifierID
		{
			get { return _cognateIdentifierID; }
		}

		public void Process(VarietyPair varietyPair)
		{
			IWordAligner aligner = _project.WordAligners[_alignerID];
			var ambiguousMeanings = new List<Tuple<Meaning, IWordAlignerResult, IWordAlignerResult[]>>();
			varietyPair.WordPairs.Clear();
			var cognateCorrCounts = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
			int cognateCount = 0;
			foreach (Meaning meaning in varietyPair.Variety1.Words.Meanings)
			{
				Word[] words1 = varietyPair.Variety1.Words[meaning].Where(w => w.Shape.Count > 0).ToArray();
				Word[] words2 = varietyPair.Variety2.Words[meaning].Where(w => w.Shape.Count > 0).ToArray();

				if (words1.Length == 1 && words2.Length == 1)
				{
					Word word1 = words1.Single();
					Word word2 = words2.Single();
					WordPair wp = varietyPair.WordPairs.Add(word1, word2);
					_project.CognacyDecisions.UpdateActualCognacy(wp);
					IWordAlignerResult alignerResult = aligner.Compute(wp);
					_thresholdCognateIdentifier.UpdatePredictedCognacy(wp, alignerResult);
					Alignment<Word, ShapeNode> alignment = alignerResult.GetAlignments().First();
					if (wp.Cognacy)
					{
						UpdateCognateCorrespondenceCounts(aligner, cognateCorrCounts, alignment);
						cognateCount++;
					}
					wp.PhoneticSimilarityScore = alignment.NormalizedScore;
				}
				else if (words1.Length > 0 && words2.Length > 0)
				{
					IWordAlignerResult[] alignerResults = words1.SelectMany(w1 => words2.Select(w2 => aligner.Compute(w1, w2))).ToArray();
					IWordAlignerResult maxAlignerResult = alignerResults.MaxBy(a => a.BestRawScore);
					ambiguousMeanings.Add(Tuple.Create(meaning, maxAlignerResult, alignerResults));
					WordPair wp = varietyPair.WordPairs.Add(maxAlignerResult.Words[0], maxAlignerResult.Words[1]);
					_thresholdCognateIdentifier.UpdatePredictedCognacy(wp, maxAlignerResult);
				}
			}

			ICognateIdentifier cognateIdentifier = _project.CognateIdentifiers[_cognateIdentifierID];
			for (int i = 0; i < ambiguousMeanings.Count; i++)
			{
				ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>> newCognateCorrCounts = cognateCorrCounts.Clone();
				int newCognateCount = cognateCount;
				for (int j = i + 1; j < ambiguousMeanings.Count; j++)
				{
					if (varietyPair.WordPairs[ambiguousMeanings[j].Item1].Cognacy)
					{
						UpdateCognateCorrespondenceCounts(aligner, newCognateCorrCounts, ambiguousMeanings[j].Item2.GetAlignments().First());
						newCognateCount++;
					}
				}

				IWordAlignerResult bestAlignerResult = null;
				WordPair bestWordPair = null;
				foreach (IWordAlignerResult alignerResult in ambiguousMeanings[i].Item3)
				{
					ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>> alignmentCognateCorrCounts = newCognateCorrCounts.Clone();
					int alignmentCognateCount = newCognateCount;
					Alignment<Word, ShapeNode> alignment = alignerResult.GetAlignments().First();
					varietyPair.WordPairs.Remove(ambiguousMeanings[i].Item1);
					WordPair wordPair = varietyPair.WordPairs.Add(alignerResult.Words[0], alignerResult.Words[1]);
					_thresholdCognateIdentifier.UpdatePredictedCognacy(wordPair, alignerResult);
					if (wordPair.Cognacy)
					{
						UpdateCognateCorrespondenceCounts(aligner, alignmentCognateCorrCounts, alignment);
						alignmentCognateCount++;
					}
					varietyPair.CognateCount = alignmentCognateCount;
					varietyPair.CognateSoundCorrespondenceFrequencyDistribution = alignmentCognateCorrCounts;
					cognateIdentifier.UpdatePredictedCognacy(wordPair, alignerResult);
					wordPair.PhoneticSimilarityScore = alignment.NormalizedScore;
					if (bestWordPair == null || Compare(wordPair, bestWordPair) > 0)
					{
						bestWordPair = wordPair;
						bestAlignerResult = alignerResult;
					}
				}

				Debug.Assert(bestWordPair != null);
				varietyPair.WordPairs.Remove(ambiguousMeanings[i].Item1);
				varietyPair.WordPairs.Add(bestWordPair);
				_project.CognacyDecisions.UpdateActualCognacy(bestWordPair);
				if (bestWordPair.Cognacy)
				{
					UpdateCognateCorrespondenceCounts(aligner, cognateCorrCounts, bestAlignerResult.GetAlignments().First());
					cognateCount++;
				}
			}

			varietyPair.CognateCount = cognateCount;
			varietyPair.CognateSoundCorrespondenceFrequencyDistribution = cognateCorrCounts;
		}

		private static int Compare(WordPair x, WordPair y)
		{
			if (x.PredictedCognacy != y.PredictedCognacy)
				return x.PredictedCognacy ? 1 : -1;

			int res = x.PredictedCognacyScore.CompareTo(y.PredictedCognacyScore);
			if (res != 0)
				return res;

			return x.PhoneticSimilarityScore.CompareTo(y.PhoneticSimilarityScore);
		}

		private void UpdateCognateCorrespondenceCounts(IWordAligner aligner, ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>> cognateCorrCounts,
			Alignment<Word, ShapeNode> alignment)
		{
			for (int column = 0; column < alignment.ColumnCount; column++)
			{
				SoundContext lhs = alignment.ToSoundContext(_segmentPool, 0, column, aligner.ContextualSoundClasses);
				Ngram<Segment> corr = alignment[1, column].ToNgram(_segmentPool);
				cognateCorrCounts[lhs].Increment(corr);
			}
		}
	}
}
