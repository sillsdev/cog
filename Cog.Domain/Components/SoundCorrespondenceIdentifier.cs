using System.Collections.Generic;
using System.Linq;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;
using SIL.Machine.NgramModeling;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Domain.Components
{
	public class SoundCorrespondenceIdentifier : IProcessor<VarietyPair>
	{
		private readonly SegmentPool _segmentPool;
		private readonly CogProject _project;
		private readonly string _alignerID;

		public SoundCorrespondenceIdentifier(SegmentPool segmentPool, CogProject project, string alignerID)
		{
			_segmentPool = segmentPool;
			_project = project;
			_alignerID = alignerID;
		}

		public void Process(VarietyPair data)
		{
			IWordAligner aligner = _project.WordAligners[_alignerID];

			var correspondenceColls = new Dictionary<FeatureSymbol, SoundCorrespondenceCollection>
				{
					{CogFeatureSystem.Onset, new SoundCorrespondenceCollection()},
					{CogFeatureSystem.Nucleus, new SoundCorrespondenceCollection()},
					{CogFeatureSystem.Coda, new SoundCorrespondenceCollection()}
				};

			foreach (WordPair wordPair in data.WordPairs.Where(wp => wp.Cognacy))
			{
				Alignment<Word, ShapeNode> alignment = aligner.Compute(wordPair).GetAlignments().First();
				for (int i = 0; i < alignment.ColumnCount; i++)
				{
					AlignmentCell<ShapeNode> cell1 = alignment[0, i];
					AlignmentCell<ShapeNode> cell2 = alignment[1, i];

					if (!cell1.IsNull && !cell2.IsNull && cell1.Count == 1 && cell2.Count == 1)
					{
						SymbolicFeatureValue pos1, pos2;
						if (cell1.First.Annotation.FeatureStruct.TryGetValue(CogFeatureSystem.SyllablePosition, out pos1)
							&& cell2.First.Annotation.FeatureStruct.TryGetValue(CogFeatureSystem.SyllablePosition, out pos2)
							&& (FeatureSymbol) pos1 == (FeatureSymbol) pos2)
						{
							Ngram<Segment> ngram1 = cell1.ToNgram(_segmentPool);
							Ngram<Segment> ngram2 = cell2.ToNgram(_segmentPool);
							Segment seg1 = ngram1.First;
							Segment seg2 = ngram2.First;
							if (!seg1.Equals(seg2))
							{
								SoundCorrespondenceCollection correspondences = correspondenceColls[(FeatureSymbol) pos1];
								SoundCorrespondence corr;
								if (!correspondences.TryGetValue(seg1, seg2, out corr))
								{
									corr = new SoundCorrespondence(seg1, seg2);
									correspondences.Add(corr);
								}
								corr.Frequency++;
								corr.WordPairs.Add(wordPair);
							}
						}

					}
				}
			}

			foreach (KeyValuePair<FeatureSymbol, SoundCorrespondenceCollection> kvp in correspondenceColls)
				data.SoundCorrespondenceCollections[kvp.Key].ReplaceAll(kvp.Value);
		}
	}
}
