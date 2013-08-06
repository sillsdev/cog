using System.Collections.Generic;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog.Domain.Components
{
	public class DolgopolskyCognateIdentifier : IProcessor<VarietyPair>
	{
		private readonly SegmentPool _segmentPool;
		private readonly CogProject _project;
		private readonly List<SoundClass> _soundClasses;
		private readonly string _alignerID;
		private readonly int _initialEquivalenceThreshold;

		public DolgopolskyCognateIdentifier(SegmentPool segmentPool, CogProject project, IEnumerable<SoundClass> soundClasses, int initialEquivalenceThreshold, string alignerID)
		{
			_segmentPool = segmentPool;
			_project = project;
			_soundClasses = soundClasses.ToList();
			_alignerID = alignerID;
			_initialEquivalenceThreshold = initialEquivalenceThreshold;
		}

		public IEnumerable<SoundClass> SoundClasses
		{
			get { return _soundClasses; }
		}

		public string AlignerID
		{
			get { return _alignerID; }
		}

		public int InitialEquivalenceThreshold
		{
			get { return _initialEquivalenceThreshold; }
		}

		public void Process(VarietyPair varietyPair)
		{
			double totalScore = 0.0;
			int totalCognateCount = 0;
			IWordAligner aligner = _project.WordAligners[_alignerID];
			foreach (WordPair wp in varietyPair.WordPairs)
			{
				wp.AlignmentNotes.Clear();
				IWordAlignerResult alignerResult = aligner.Compute(wp);
				Alignment<Word, ShapeNode> alignment = alignerResult.GetAlignments().First();
				wp.PhoneticSimilarityScore = alignment.NormalizedScore;
				int initialEquivalentClasses = 0;
				bool mismatchFound = false;
				for (int column = 0; column < alignment.ColumnCount; column++)
				{
					AlignmentCell<ShapeNode> cell1 = alignment[0, column];
					AlignmentCell<ShapeNode> cell2 = alignment[1, column];

					if ((cell1.Count > 0 && cell1[0].Type() == CogFeatureSystem.VowelType) || (cell2.Count > 0 && cell2[0].Type() == CogFeatureSystem.VowelType))
					{
						wp.AlignmentNotes.Add("X");
					}
					else
					{
						if (cell1.StrRep() == cell2.StrRep())
						{
							wp.AlignmentNotes.Add("1");
							if (!mismatchFound)
								initialEquivalentClasses++;
						}
						else
						{
							SoundClass sc1;
							if (!_soundClasses.TryGetMatchingSoundClass(_segmentPool, alignment, 0, column, wp.Word1, out sc1))
								sc1 = null;
							SoundClass sc2;
							if (!_soundClasses.TryGetMatchingSoundClass(_segmentPool, alignment, 1, column, wp.Word2, out sc2))
								sc2 = null;
							if (sc1 != null && sc2 != null && sc1 == sc2)
							{
								wp.AlignmentNotes.Add("1");
								if (!mismatchFound)
									initialEquivalentClasses++;
							}
							else
							{
								wp.AlignmentNotes.Add("0");
								mismatchFound = true;
							}
						}
					}
				}

				if (!mismatchFound || initialEquivalentClasses >= _initialEquivalenceThreshold)
				{
					wp.AreCognatePredicted = true;
					totalCognateCount++;
				}

				totalScore += wp.PhoneticSimilarityScore;
			}

			int wordPairCount = varietyPair.WordPairs.Count;
			varietyPair.PhoneticSimilarityScore = totalScore / wordPairCount;
			varietyPair.LexicalSimilarityScore = (double) totalCognateCount / wordPairCount;
		}
	}
}
