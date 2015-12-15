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
		private readonly bool _ignoreRegularInsertionDeletion;
		private readonly bool _regularConsEqual;
		private readonly ISegmentMappings _ignoredMappings;
		private readonly ISegmentMappings _similarSegments;
		private readonly bool _automaticRegularCorrespondenceThreshold;
		private readonly int _defaultRegularCorrepondenceThreshold;
		private readonly RegularSoundCorrespondenceThresholdTable _regularCorrespondenceThresholdTable;

		public BlairCognateIdentifier(SegmentPool segmentPool, bool ignoreRegularInsertionDeletion, bool regularConsEqual,
			bool automaticRegularCorrespondenceThreshold, int defaultRegularCorrepondenceThreshold, ISegmentMappings ignoredMappings, ISegmentMappings similarSegments)
		{
			_segmentPool = segmentPool;
			_ignoreRegularInsertionDeletion = ignoreRegularInsertionDeletion;
			_regularConsEqual = regularConsEqual;
			_ignoredMappings = ignoredMappings;
			_similarSegments = similarSegments;
			_automaticRegularCorrespondenceThreshold = automaticRegularCorrespondenceThreshold;
			_defaultRegularCorrepondenceThreshold = defaultRegularCorrepondenceThreshold;
			if (_automaticRegularCorrespondenceThreshold)
				_regularCorrespondenceThresholdTable = new RegularSoundCorrespondenceThresholdTable();
		}

		public bool IgnoreRegularInsertionDeletion
		{
			get { return _ignoreRegularInsertionDeletion; }
		}

		public bool RegularConsonantEqual
		{
			get { return _regularConsEqual; }
		}

		public bool AutomaticRegularCorrespondenceThreshold
		{
			get { return _automaticRegularCorrespondenceThreshold; }
		}

		public int DefaultRegularCorrespondenceThreshold
		{
			get { return _defaultRegularCorrepondenceThreshold; }
		}

		public ISegmentMappings IgnoredMappings
		{
			get { return _ignoredMappings; }
		}

		public ISegmentMappings SimilarSegments
		{
			get { return _similarSegments; }
		}

		public void UpdateCognacy(WordPair wordPair, IWordAlignerResult alignerResult)
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
				else if (_ignoredMappings.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode))
				{
					cat = 0;
				}
				else if (u.Length == 0 || v.Length == 0)
				{
					if (_similarSegments.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode))
						cat = 1;
					else if (_ignoreRegularInsertionDeletion && IsRegular(wordPair, alignerResult, alignment, column, v))
						cat = 0;
				}
				else if (u[0].Type == CogFeatureSystem.VowelType && v[0].Type == CogFeatureSystem.VowelType)
				{
					cat = _similarSegments.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode) ? 1 : 2;
				}
				else if (u[0].Type == CogFeatureSystem.ConsonantType && v[0].Type == CogFeatureSystem.ConsonantType)
				{
					if (_regularConsEqual)
					{
						if (IsRegular(wordPair, alignerResult, alignment, column, v))
							cat = 1;
						else if (_similarSegments.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode))
							cat = 2;
					}
					else
					{
						if (_similarSegments.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode))
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

		private bool IsRegular(WordPair wordPair, IWordAlignerResult alignerResult, Alignment<Word, ShapeNode> alignment, int column, Ngram<Segment> v)
		{
			VarietyPair vp = wordPair.VarietyPair;
			SoundContext context = alignment.ToSoundContext(_segmentPool, 0, column, alignerResult.WordAligner.ContextualSoundClasses);
			FrequencyDistribution<Ngram<Segment>> freqDist = vp.AllSoundCorrespondenceFrequencyDistribution[context];
			int threshold;
			if (_automaticRegularCorrespondenceThreshold)
			{
				int seg2Count = vp.AllSoundCorrespondenceFrequencyDistribution.Conditions.Where(sc => sc.LeftEnvironment == context.LeftEnvironment && sc.RightEnvironment == context.RightEnvironment)
					.Sum(sc => vp.AllSoundCorrespondenceFrequencyDistribution[sc][v]);
				if (!_regularCorrespondenceThresholdTable.TryGetThreshold(vp.WordPairs.Count, freqDist.SampleOutcomeCount, seg2Count, out threshold))
					threshold = _defaultRegularCorrepondenceThreshold;
			}
			else
			{
				threshold = _defaultRegularCorrepondenceThreshold;
			}
			return freqDist[v] >= threshold;
		}
	}
}
