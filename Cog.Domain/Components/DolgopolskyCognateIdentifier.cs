using System.Collections.Generic;
using System.Linq;
using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Domain.Components
{
	public class DolgopolskyCognateIdentifier : ICognateIdentifier
	{
		private readonly SegmentPool _segmentPool;
		private readonly List<SoundClass> _soundClasses;
		private readonly int _initialEquivalenceThreshold;

		public DolgopolskyCognateIdentifier(SegmentPool segmentPool, IEnumerable<SoundClass> soundClasses, int initialEquivalenceThreshold)
		{
			_segmentPool = segmentPool;
			_soundClasses = soundClasses.ToList();
			_initialEquivalenceThreshold = initialEquivalenceThreshold;
		}

		public IEnumerable<SoundClass> SoundClasses
		{
			get { return _soundClasses; }
		}

		public int InitialEquivalenceThreshold
		{
			get { return _initialEquivalenceThreshold; }
		}

		public void UpdatePredictedCognacy(WordPair wordPair, IWordAlignerResult alignerResult)
		{
			wordPair.AlignmentNotes.Clear();
			Alignment<Word, ShapeNode> alignment = alignerResult.GetAlignments().First();
			int initialEquivalentClasses = 0;
			bool mismatchFound = false;
			for (int column = 0; column < alignment.ColumnCount; column++)
			{
				AlignmentCell<ShapeNode> cell1 = alignment[0, column];
				AlignmentCell<ShapeNode> cell2 = alignment[1, column];

				if ((cell1.Count > 0 && cell1[0].Type() == CogFeatureSystem.VowelType) || (cell2.Count > 0 && cell2[0].Type() == CogFeatureSystem.VowelType))
				{
					wordPair.AlignmentNotes.Add("X");
				}
				else
				{
					if (cell1.StrRep() == cell2.StrRep())
					{
						wordPair.AlignmentNotes.Add("1");
						if (!mismatchFound)
							initialEquivalentClasses++;
					}
					else
					{
						SoundClass sc1;
						if (!_soundClasses.TryGetMatchingSoundClass(_segmentPool, alignment, 0, column, out sc1))
							sc1 = null;
						SoundClass sc2;
						if (!_soundClasses.TryGetMatchingSoundClass(_segmentPool, alignment, 1, column, out sc2))
							sc2 = null;
						if (sc1 != null && sc2 != null && sc1 == sc2)
						{
							wordPair.AlignmentNotes.Add("1");
							if (!mismatchFound)
								initialEquivalentClasses++;
						}
						else
						{
							wordPair.AlignmentNotes.Add("0");
							mismatchFound = true;
						}
					}
				}
			}

			wordPair.PredictedCognacy = !mismatchFound || initialEquivalentClasses >= _initialEquivalenceThreshold;
			wordPair.PredictedCognacyScore = (double) initialEquivalentClasses / alignment.ColumnCount;
		}
	}
}
