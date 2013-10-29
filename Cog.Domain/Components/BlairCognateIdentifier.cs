using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SIL.Machine;
using SIL.Machine.NgramModeling;

namespace SIL.Cog.Domain.Components
{
	public class BlairCognateIdentifier : IProcessor<VarietyPair>
	{
		private readonly SegmentPool _segmentPool;
		private readonly CogProject _project;
		private readonly bool _ignoreRegularInsertionDeletion;
		private readonly bool _regularConsEqual;
		private readonly string _alignerID;
		private readonly ISegmentMappings _ignoredMappings;
		private readonly ISegmentMappings _similarSegments;

		public BlairCognateIdentifier(SegmentPool segmentPool, CogProject project, bool ignoreRegularInsertionDeletion, bool regularConsEqual, string alignerID,
			ISegmentMappings ignoredMappings, ISegmentMappings similarSegments)
		{
			_segmentPool = segmentPool;
			_project = project;
			_ignoreRegularInsertionDeletion = ignoreRegularInsertionDeletion;
			_regularConsEqual = regularConsEqual;
			_alignerID = alignerID;
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

		public string AlignerID
		{
			get { return _alignerID; }
		}

		public ISegmentMappings IgnoredMappings
		{
			get { return _ignoredMappings; }
		}

		public ISegmentMappings SimilarSegments
		{
			get { return _similarSegments; }
		}

		public void Process(VarietyPair varietyPair)
		{
			IWordAligner aligner = _project.WordAligners[_alignerID];
			var correspondences = new HashSet<Tuple<string, string>>(varietyPair.SoundChangeFrequencyDistribution.Conditions
				.SelectMany(cond => varietyPair.SoundChangeFrequencyDistribution[cond].ObservedSamples.Where(ngram => varietyPair.SoundChangeFrequencyDistribution[cond][ngram] >= 3),
				(lhs, ngram) => Tuple.Create(lhs.Target.ToString(), ngram.ToString())));
			double totalScore = 0.0;
			int totalCognateCount = 0;
			foreach (WordPair wordPair in varietyPair.WordPairs)
			{
				wordPair.AlignmentNotes.Clear();
				IWordAlignerResult alignerResult = aligner.Compute(wordPair);
				Alignment<Word, ShapeNode> alignment = alignerResult.GetAlignments().First();
				int cat1Count = 0;
				int cat1And2Count = 0;
				int totalCount = 0;
				for (int column = 0; column < alignment.ColumnCount; column++)
				{
					ShapeNode uLeftNode = alignment.GetLeftNode(0, column);
					Ngram<Segment> u = alignment[0, column].ToNgram(_segmentPool);
					ShapeNode uRightNode = alignment.GetRightNode(0, column);
					ShapeNode vLeftNode = alignment.GetLeftNode(1, column);
					Ngram<Segment> v = alignment[1, column].ToNgram(_segmentPool);
					ShapeNode vRightNode = alignment.GetRightNode(1, column);
					string uStr = u.ToString();
					string vStr = v.ToString();
					int cat = 3;
					if (uStr == vStr)
					{
						cat = 1;
					}
					else if (_ignoredMappings.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode))
					{
						cat = 0;
					}
					else if (u.Count == 0 || v.Count == 0)
					{
						if (_similarSegments.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode) || correspondences.Contains(Tuple.Create(uStr, vStr)))
							cat = _ignoreRegularInsertionDeletion ? 0 : 1;
					}
					else if (u[0].Type == CogFeatureSystem.VowelType && v[0].Type == CogFeatureSystem.VowelType)
					{
						cat = _similarSegments.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode) ? 1 : 2;
					}
					else if (u[0].Type == CogFeatureSystem.ConsonantType && v[0].Type == CogFeatureSystem.ConsonantType)
					{
						if (_regularConsEqual)
						{
							if (correspondences.Contains(Tuple.Create(uStr, vStr)))
								cat = 1;
							else if (_similarSegments.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode))
								cat = 2;
						}
						else
						{
							if (_similarSegments.IsMapped(uLeftNode, u, uRightNode, vLeftNode, v, vRightNode))
								cat = correspondences.Contains(Tuple.Create(uStr, vStr)) ? 1 : 2;
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
				wordPair.PhoneticSimilarityScore = alignment.NormalizedScore;
				if (wordPair.AreCognatePredicted)
					totalCognateCount++;
				totalScore += wordPair.PhoneticSimilarityScore;
			}

			int wordPairCount = varietyPair.WordPairs.Count;
			varietyPair.PhoneticSimilarityScore = totalScore / wordPairCount;
			varietyPair.LexicalSimilarityScore = (double) totalCognateCount / wordPairCount;
		}
	}
}
