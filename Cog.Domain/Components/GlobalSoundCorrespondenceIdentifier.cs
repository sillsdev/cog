using System;
using System.Linq;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain.Components
{
	public class GlobalSoundCorrespondenceIdentifier : ProcessorBase<VarietyPair>
	{
		private readonly string _alignerID;

		public GlobalSoundCorrespondenceIdentifier(CogProject project, string alignerID)
			: base(project)
		{
			_alignerID = alignerID;
		}

		private static bool IsStemInitial(Alignment<Word, ShapeNode> alignment, int column)
		{
			return column == 0;
		}

		private static bool IsStemMedial(Alignment<Word, ShapeNode> alignment, int column)
		{
			return column > 0 && column < alignment.ColumnCount - 1;
		}

		private static bool IsStemFinal(Alignment<Word, ShapeNode> alignment, int column)
		{
			return column == alignment.ColumnCount - 1;
		}

		private static bool IsOnset(Alignment<Word, ShapeNode> alignment, int column)
		{
			AlignmentCell<ShapeNode> cell1 = alignment[0, column];
			AlignmentCell<ShapeNode> cell2 = alignment[1, column];

			return cell1.First.Annotation.Parent.Children.First == cell1.First.Annotation
				|| cell2.First.Annotation.Parent.Children.First == cell2.First.Annotation;
		}

		private static bool IsCoda(Alignment<Word, ShapeNode> alignment, int column)
		{
			AlignmentCell<ShapeNode> cell1 = alignment[0, column];
			AlignmentCell<ShapeNode> cell2 = alignment[1, column];

			return cell1.Last.Annotation.Parent.Children.Last == cell1.Last.Annotation
				|| cell2.Last.Annotation.Parent.Children.Last == cell2.Last.Annotation;
		}

		public override void Process(VarietyPair data)
		{
			IWordAligner aligner = Project.WordAligners[_alignerID];

			var identifiers = new[]
				{
					new CorrespondenceIdentifier(CogFeatureSystem.ConsonantType, IsStemInitial),
					new CorrespondenceIdentifier(CogFeatureSystem.ConsonantType, IsStemMedial),
					new CorrespondenceIdentifier(CogFeatureSystem.ConsonantType, IsStemFinal),

					new CorrespondenceIdentifier(CogFeatureSystem.ConsonantType, IsOnset),
					new CorrespondenceIdentifier(CogFeatureSystem.ConsonantType, IsCoda),

					new CorrespondenceIdentifier(CogFeatureSystem.VowelType, (alignment, i) => true)
				};

			foreach (WordPair wordPair in data.WordPairs.Where(wp => wp.AreCognatePredicted))
			{
				Alignment<Word, ShapeNode> alignment = aligner.Compute(wordPair).GetAlignments().First();
				for (int i = 0; i < alignment.ColumnCount; i++)
				{
					foreach (CorrespondenceIdentifier identifier in identifiers)
						identifier.ProcessColumn(wordPair, alignment, i);
				}
			}

			MergeCorrespondences(identifiers[0].Correspondences, Project.StemInitialConsonantCorrespondences);
			MergeCorrespondences(identifiers[1].Correspondences, Project.StemMedialConsonantCorrespondences);
			MergeCorrespondences(identifiers[2].Correspondences, Project.StemFinalConsonantCorrespondences);
			MergeCorrespondences(identifiers[3].Correspondences, Project.OnsetConsonantCorrespondences);
			MergeCorrespondences(identifiers[4].Correspondences, Project.CodaConsonantCorrespondences);
			MergeCorrespondences(identifiers[5].Correspondences, Project.VowelCorrespondences);
		}

		private void MergeCorrespondences(GlobalSoundCorrespondenceCollection source, GlobalSoundCorrespondenceCollection target)
		{
			if (source.Count == 0)
				return;

			lock (target)
			{
				foreach (GlobalSoundCorrespondence sourceCorr in source)
				{
					GlobalSoundCorrespondence targetCorr;
					if (target.TryGetValue(sourceCorr.Segment1, sourceCorr.Segment2, out targetCorr))
					{
						targetCorr.Frequency += sourceCorr.Frequency;
						targetCorr.WordPairs.AddRange(sourceCorr.WordPairs);
					}
					else
					{
						target.Add(sourceCorr);
					}
				}
			}
		}

		private class CorrespondenceIdentifier
		{
			private readonly GlobalSoundCorrespondenceCollection _correspondences;
			private readonly FeatureSymbol _type;
			private readonly Func<Alignment<Word, ShapeNode>, int, bool> _filter;

			public CorrespondenceIdentifier(FeatureSymbol type, Func<Alignment<Word, ShapeNode>, int, bool> filter)
			{
				_correspondences = new GlobalSoundCorrespondenceCollection();
				_type = type;
				_filter = filter;
			}

			public GlobalSoundCorrespondenceCollection Correspondences
			{
				get { return _correspondences; }
			}

			public void ProcessColumn(WordPair wp, Alignment<Word, ShapeNode> alignment, int column)
			{
				AlignmentCell<ShapeNode> cell1 = alignment[0, column];
				AlignmentCell<ShapeNode> cell2 = alignment[1, column];
				if (!cell1.IsNull && cell1.First.Type() == _type && !cell2.IsNull && cell2.First.Type() == _type && _filter(alignment, column))
				{
					Ngram ngram1 = cell1.ToNgram(wp.Word1.Variety.SegmentPool);
					Ngram ngram2 = cell2.ToNgram(wp.Word2.Variety.SegmentPool);
					if (ngram1.Count == 1 && ngram2.Count == 1)
					{
						Segment seg1 = ngram1.First;
						Segment seg2 = ngram2.First;
						if (!seg1.Equals(seg2))
						{
							GlobalSoundCorrespondence corr;
							if (!_correspondences.TryGetValue(seg1, seg2, out corr))
							{
								corr = new GlobalSoundCorrespondence(seg1, seg2);
								_correspondences.Add(corr);
							}
							corr.Frequency++;
							corr.WordPairs.Add(wp);
						}
					}
				}
			}
		}
	}
}
