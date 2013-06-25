using System.Collections.Generic;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog.Components
{
	public class DolgopolskyCognateIdentifier : ProcessorBase<VarietyPair>
	{
		private readonly List<SoundClass> _soundClasses;
		private readonly string _alignerID;
		private readonly int _initialEquivalenceThreshold;

		public DolgopolskyCognateIdentifier(CogProject project, IEnumerable<SoundClass> soundClasses, int initialEquivalenceThreshold, string alignerID)
			: base(project)
		{
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

		public override void Process(VarietyPair varietyPair)
		{
			double totalScore = 0.0;
			int totalCognateCount = 0;
			IWordPairAligner aligner = Project.Aligners[_alignerID];
			foreach (WordPair wp in varietyPair.WordPairs)
			{
				IWordPairAlignerResult alignerResult = aligner.Compute(wp);
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
							SoundClass sc1 = alignment.GetMatchingSoundClass(0, column, wp.Word1, _soundClasses);
							SoundClass sc2 = alignment.GetMatchingSoundClass(1, column, wp.Word2, _soundClasses);
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
