using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog.Components
{
	public class BlairCognateIdentifier : ProcessorBase<VarietyPair>
	{
		private readonly bool _ignoreRegularInsertionDeletion;
		private readonly bool _regularConsEqual;
		private readonly string _alignerID;
		private readonly ISegmentMappings _ignoredMappings;
		private readonly ISegmentMappings _similarSegments;

		public BlairCognateIdentifier(CogProject project, bool ignoreRegularInsertionDeletion, bool regularConsEqual, string alignerID,
			ISegmentMappings ignoredMappings, ISegmentMappings similarSegments)
			: base(project)
		{
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

		public override void Process(VarietyPair varietyPair)
		{
			IWordAligner aligner = Project.WordAligners[_alignerID];
			var correspondences = new HashSet<Tuple<string, string>>(varietyPair.SoundChangeFrequencyDistribution.Conditions
				.SelectMany(cond => varietyPair.SoundChangeFrequencyDistribution[cond].ObservedSamples.Where(ngram => varietyPair.SoundChangeFrequencyDistribution[cond][ngram] >= 3),
				(lhs, ngram) => Tuple.Create(lhs.Target.ToString(), ngram.ToString())));
			double totalScore = 0.0;
			int totalCognateCount = 0;
			foreach (WordPair wordPair in varietyPair.WordPairs)
			{
				IWordAlignerResult alignerResult = aligner.Compute(wordPair);
				Alignment<Word, ShapeNode> alignment = alignerResult.GetAlignments().First();
				int cat1Count = 0;
				int cat1And2Count = 0;
				int totalCount = 0;
				for (int column = 0; column < alignment.ColumnCount; column++)
				{
					Ngram u = alignment[0, column].ToNgram(varietyPair.Variety1.SegmentPool);
					Ngram v = alignment[1, column].ToNgram(varietyPair.Variety2.SegmentPool);
					string uStr = u.ToString();
					string vStr = v.ToString();
					int cat = 3;
					if (uStr == vStr)
					{
						cat = 1;
					}
					else if (AreSegmentsMapped(_ignoredMappings, u, v))
					{
						cat = 0;
					}
					else if (u.Count == 0 || v.Count == 0)
					{
						if (AreSegmentsMapped(_similarSegments, u, v) || correspondences.Contains(Tuple.Create(uStr, vStr)))
							cat = _ignoreRegularInsertionDeletion ? 0 : 1;
					}
					else if (u[0].Type == CogFeatureSystem.VowelType && v[0].Type == CogFeatureSystem.VowelType)
					{
						cat = AreSegmentsMapped(_similarSegments, u, v) ? 1 : 2;
					}
					else if (u[0].Type == CogFeatureSystem.ConsonantType && v[0].Type == CogFeatureSystem.ConsonantType)
					{
						if (_regularConsEqual)
						{
							if (correspondences.Contains(Tuple.Create(uStr, vStr)))
								cat = 1;
							else if (AreSegmentsMapped(_similarSegments, u, v))
								cat = 2;
						}
						else
						{
							if (AreSegmentsMapped(_similarSegments, u, v))
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

		private bool AreSegmentsMapped(ISegmentMappings mappings, Ngram u, Ngram v)
		{
			IEnumerable<Segment> uSegs = u.Count == 0 ? new Segment[] {null} : (IEnumerable<Segment>) u;
			IEnumerable<Segment> vSegs = v.Count == 0 ? new Segment[] {null} : (IEnumerable<Segment>) v;
			foreach (Segment uSeg in uSegs)
			{
				foreach (Segment vSeg in vSegs)
				{
					if (mappings.IsMapped(uSeg, vSeg))
						return true;
				}
			}

			return false;
		}
	}
}
