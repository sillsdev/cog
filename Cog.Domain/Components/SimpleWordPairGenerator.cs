using System.Diagnostics;
using System.Linq;
using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;
using SIL.Machine.SequenceAlignment;
using SIL.Machine.Statistics;

namespace SIL.Cog.Domain.Components
{
	public class SimpleWordPairGenerator : IProcessor<VarietyPair>
	{
		private readonly CogProject _project;
		private readonly string _alignerID;
		private readonly SegmentPool _segmentPool;
		private readonly ThresholdCognateIdentifier _thresholdCognateIdentifier;

		public SimpleWordPairGenerator(SegmentPool segmentPool, CogProject project, double initialAlignmentThreshold, string alignerID)
		{
			_segmentPool = segmentPool;
			_project = project;
			_alignerID = alignerID;
			_thresholdCognateIdentifier = new ThresholdCognateIdentifier(initialAlignmentThreshold);
		}

		public string AlignerID
		{
			get { return _alignerID; }
		}

		public double InitialAlignmentThreshold
		{
			get { return _thresholdCognateIdentifier.Threshold; }
		}

		public void Process(VarietyPair varietyPair)
		{
			IWordAligner aligner = _project.WordAligners[_alignerID];
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
					WordPair bestWordPair = null;
					IWordAlignerResult bestAlignerResult = null;
					foreach (Word w1 in words1)
					{
						foreach (Word w2 in words2)
						{
							IWordAlignerResult alignerResult = aligner.Compute(w1, w2);
							if (bestAlignerResult == null || alignerResult.BestRawScore > bestAlignerResult.BestRawScore)
							{
								bestWordPair = new WordPair(w1, w2);
								bestAlignerResult = alignerResult;
							}
						}
					}

					Debug.Assert(bestWordPair != null);
					varietyPair.WordPairs.Add(bestWordPair);
					_project.CognacyDecisions.UpdateActualCognacy(bestWordPair);
					_thresholdCognateIdentifier.UpdatePredictedCognacy(bestWordPair, bestAlignerResult);
					Alignment<Word, ShapeNode> alignment = bestAlignerResult.GetAlignments().First();
					if (bestWordPair.Cognacy)
					{
						UpdateCognateCorrespondenceCounts(aligner, cognateCorrCounts, alignment);
						cognateCount++;
					}
					bestWordPair.PhoneticSimilarityScore = alignment.NormalizedScore;
				}
			}

			varietyPair.CognateCount = cognateCount;
			varietyPair.CognateSoundCorrespondenceFrequencyDistribution = cognateCorrCounts;
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
