using System.Globalization;
using System.Linq;
using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;
using SIL.Machine.SequenceAlignment;
using SIL.Machine.Statistics;

namespace SIL.Cog.Domain.Components
{
	public class BlairCognateIdentifier : ICognateIdentifier
	{
		private readonly SegmentPool _segmentPool;
		private readonly RegularSoundCorrespondenceThresholdTable _regularCorrespondenceThresholdTable;

		public BlairCognateIdentifier(SegmentPool segmentPool, bool ignoreRegularInsertionDeletion, bool regularConsEqual,
			bool automaticRegularCorrespondenceThreshold, int defaultRegularCorrepondenceThreshold, ISegmentMappings ignoredMappings,
			ISegmentMappings similarSegments)
		{
			_segmentPool = segmentPool;
			IgnoreRegularInsertionDeletion = ignoreRegularInsertionDeletion;
			RegularConsonantEqual = regularConsEqual;
			IgnoredMappings = ignoredMappings;
			SimilarSegments = similarSegments;
			AutomaticRegularCorrespondenceThreshold = automaticRegularCorrespondenceThreshold;
			DefaultRegularCorrespondenceThreshold = defaultRegularCorrepondenceThreshold;
			if (AutomaticRegularCorrespondenceThreshold)
				_regularCorrespondenceThresholdTable = new RegularSoundCorrespondenceThresholdTable();
		}

		public bool IgnoreRegularInsertionDeletion { get; }
		public bool RegularConsonantEqual { get; }
		public bool AutomaticRegularCorrespondenceThreshold { get; }
		public int DefaultRegularCorrespondenceThreshold { get; }
		public ISegmentMappings IgnoredMappings { get; }
		public ISegmentMappings SimilarSegments { get; }

		public void UpdatePredictedCognacy(WordPair wordPair, IWordAlignerResult alignerResult)
		{
			wordPair.AlignmentNotes.Clear();
			int cat1Count = 0;
			int cat1And2Count = 0;
			int totalCount = 0;
			Alignment<Word, ShapeNode> alignment = alignerResult.GetAlignments().First();
			for (int column = 0; column < alignment.ColumnCount; column++)
			{
				ShapeNode uLeftNode = alignment.GetLeftNode(0, column);
				Ngram<Segment> u = alignment[0, column].ToNgram(_segmentPool);
				ShapeNode uRightNode = alignment.GetRightNode(0, column);
				ShapeNode vLeftNode = alignment.GetLeftNode(1, column);
				Ngram<Segment> v = alignment[1, column].ToNgram(_segmentPool);
				ShapeNode vRightNode = alignment.GetRightNode(1, column);

				int cat = 3;
				if (u.Equals(v))
				{
					cat = 1;
				}
				else if (IgnoredMappings.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode))
				{
					cat = 0;
				}
				else if (u.Length == 0 || v.Length == 0)
				{
					if (SimilarSegments.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode))
						cat = 1;
					else if (IgnoreRegularInsertionDeletion && IsRegular(wordPair, alignerResult, alignment, column, v))
						cat = 0;
				}
				else if (u[0].Type == CogFeatureSystem.VowelType && v[0].Type == CogFeatureSystem.VowelType)
				{
					cat = SimilarSegments.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode) ? 1 : 2;
				}
				else if (u[0].Type == CogFeatureSystem.ConsonantType && v[0].Type == CogFeatureSystem.ConsonantType)
				{
					if (RegularConsonantEqual)
					{
						if (IsRegular(wordPair, alignerResult, alignment, column, v))
							cat = 1;
						else if (SimilarSegments.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode))
							cat = 2;
					}
					else
					{
						if (SimilarSegments.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode))
							cat = IsRegular(wordPair, alignerResult, alignment, column, v) ? 1 : 2;
					}
				}

				if (cat > 0 && cat < 3)
				{
					cat1And2Count++;
					if (cat == 1)
						cat1Count++;
				}
				wordPair.AlignmentNotes.Add(cat == 0 ? "-" : cat.ToString(CultureInfo.InvariantCulture));
				if (cat > 0)
					totalCount++;
			}

			double type1Score = (double) cat1Count / totalCount;
			double type1And2Score = (double) cat1And2Count / totalCount;
			wordPair.PredictedCognacy = type1Score >= 0.5 && type1And2Score >= 0.75;
			wordPair.PredictedCognacyScore = (type1Score * 0.75) + (type1And2Score * 0.25);
		}

		private bool IsRegular(WordPair wordPair, IWordAlignerResult alignerResult, Alignment<Word, ShapeNode> alignment, int column,
			Ngram<Segment> v)
		{
			VarietyPair vp = wordPair.VarietyPair;
			SoundContext context = alignment.ToSoundContext(_segmentPool, 0, column, alignerResult.WordAligner.ContextualSoundClasses);
			FrequencyDistribution<Ngram<Segment>> freqDist = vp.CognateSoundCorrespondenceFrequencyDistribution[context];
			int threshold;
			if (AutomaticRegularCorrespondenceThreshold)
			{
				int seg2Count = vp.CognateSoundCorrespondenceFrequencyDistribution.Conditions
					.Where(sc => sc.LeftEnvironment == context.LeftEnvironment && sc.RightEnvironment == context.RightEnvironment)
					.Sum(sc => vp.CognateSoundCorrespondenceFrequencyDistribution[sc][v]);
				if (!_regularCorrespondenceThresholdTable.TryGetThreshold(vp.CognateCount, freqDist.SampleOutcomeCount, seg2Count,
					out threshold))
				{
					threshold = DefaultRegularCorrespondenceThreshold;
				}
			}
			else
			{
				threshold = DefaultRegularCorrespondenceThreshold;
			}
			return freqDist[v] >= threshold;
		}
	}
}
