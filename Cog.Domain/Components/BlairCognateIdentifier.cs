using System.Globalization;
using System.Linq;
using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Domain.Components
{
	public class BlairCognateIdentifier : ICognateIdentifier
	{
		private readonly SegmentPool _segmentPool;
		private readonly bool _ignoreRegularInsertionDeletion;
		private readonly bool _regularConsEqual;
		private readonly ISegmentMappings _ignoredMappings;
		private readonly ISegmentMappings _similarSegments;

		public BlairCognateIdentifier(SegmentPool segmentPool, bool ignoreRegularInsertionDeletion, bool regularConsEqual,
			ISegmentMappings ignoredMappings, ISegmentMappings similarSegments)
		{
			_segmentPool = segmentPool;
			_ignoreRegularInsertionDeletion = ignoreRegularInsertionDeletion;
			_regularConsEqual = regularConsEqual;
			_ignoredMappings = ignoredMappings;
			_similarSegments = similarSegments;
		}

		public bool IgnoreRegularInsertionDeletion
		{
			get { return _ignoreRegularInsertionDeletion; }
		}

		public bool RegularConsonantEqual
		{
			get { return _regularConsEqual; }
		}

		public ISegmentMappings IgnoredMappings
		{
			get { return _ignoredMappings; }
		}

		public ISegmentMappings SimilarSegments
		{
			get { return _similarSegments; }
		}

		public void UpdateCognicity(WordPair wordPair, IWordAlignerResult alignerResult)
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

				bool regular = wordPair.VarietyPair.SoundChangeFrequencyDistribution[alignment.ToSoundContext(_segmentPool, 0, column, alignerResult.WordAligner.ContextualSoundClasses)][v] >= 3;

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
					else if (_ignoreRegularInsertionDeletion && regular)
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
						if (regular)
							cat = 1;
						else if (_similarSegments.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode))
							cat = 2;
					}
					else
					{
						if (_similarSegments.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode))
							cat = regular ? 1 : 2;
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
			wordPair.AreCognatePredicted = type1Score >= 0.5 && type1And2Score >= 0.75;
			wordPair.CognicityScore = (type1Score * 0.75) + (type1And2Score * 0.25);
		}
	}
}
