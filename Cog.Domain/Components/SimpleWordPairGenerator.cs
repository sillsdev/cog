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
		private readonly double _initialAlignmentThreshold;
		private readonly SegmentPool _segmentPool;

		public SimpleWordPairGenerator(SegmentPool segmentPool, CogProject project, double initialAlignmentThreshold, string alignerID)
		{
			_segmentPool = segmentPool;
			_project = project;
			_alignerID = alignerID;
			_initialAlignmentThreshold = initialAlignmentThreshold;
		}

		public string AlignerID
		{
			get { return _alignerID; }
		}

		public double InitialAlignmentThreshold
		{
			get { return _initialAlignmentThreshold; }
		}

		public void Process(VarietyPair varietyPair)
		{
			IWordAligner aligner = _project.WordAligners[_alignerID];
			varietyPair.WordPairs.Clear();
			var counts = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
			foreach (Meaning meaning in varietyPair.Variety1.Words.Meanings)
			{
				Word[] words1 = varietyPair.Variety1.Words[meaning].Where(w => w.Shape.Count > 0).ToArray();
				Word[] words2 = varietyPair.Variety2.Words[meaning].Where(w => w.Shape.Count > 0).ToArray();
				if (words1.Length == 1 && words2.Length == 1)
				{
					Word word1 = words1.Single();
					Word word2 = words2.Single();
					WordPair wp = varietyPair.WordPairs.Add(word1, word2);
					Alignment<Word, ShapeNode> alignment = aligner.Compute(wp).GetAlignments().First();
					wp.PhoneticSimilarityScore = alignment.NormalizedScore;
					UpdateCounts(aligner, counts, alignment);
				}
				else if (words1.Length > 0 && words2.Length > 0)
				{
					WordPair bestWordPair = null;
					Alignment<Word, ShapeNode> bestAlignment = null;
					foreach (Word w1 in words1)
					{
						foreach (Word w2 in words2)
						{
							Alignment<Word, ShapeNode> alignment = aligner.Compute(w1, w2).GetAlignments().First();
							double score = alignment.NormalizedScore;
							if (bestWordPair == null || score > bestWordPair.PhoneticSimilarityScore)
							{
								bestWordPair = new WordPair(w1, w2) {PhoneticSimilarityScore = score};
								bestAlignment = alignment;
							}
						}
					}

					varietyPair.WordPairs.Add(bestWordPair);
					UpdateCounts(aligner, counts, bestAlignment);
				}
			}

			varietyPair.SoundChangeFrequencyDistribution = counts;
		}

		private void UpdateCounts(IWordAligner aligner, ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>> counts, Alignment<Word, ShapeNode> alignment)
		{
			if (alignment.NormalizedScore < _initialAlignmentThreshold)
				return;

			for (int column = 0; column < alignment.ColumnCount; column++)
			{
				SoundContext lhs = alignment.ToSoundContext(_segmentPool, 0, column, aligner.ContextualSoundClasses);
				Ngram<Segment> corr = alignment[1, column].ToNgram(_segmentPool);
				counts[lhs].Increment(corr);
			}
		}
	}
}
