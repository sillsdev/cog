using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog.Processors
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
			IAligner aligner = Project.Aligners[_alignerID];
			foreach (WordPair wp in varietyPair.WordPairs)
			{
				IAlignerResult alignerResult = aligner.Compute(wp);
				Alignment alignment = alignerResult.GetAlignments().First();
				wp.PhoneticSimilarityScore = alignment.Score;
				int initialEquivalentClasses = 0;
				bool mismatchFound = false;
				foreach (Tuple<Annotation<ShapeNode>, Annotation<ShapeNode>> aann in alignment.AlignedAnnotations)
				{
					if (aann.Item1.Type() == CogFeatureSystem.VowelType || aann.Item2.Type() == CogFeatureSystem.VowelType)
					{
						wp.AlignmentNotes.Add("X");
					}
					else
					{
						if (aann.Item1.StrRep() == aann.Item2.StrRep())
						{
							wp.AlignmentNotes.Add("1");
							if (!mismatchFound)
								initialEquivalentClasses++;
						}
						else
						{
							SoundClass sc1 = GetSoundClass(aann.Item1);
							SoundClass sc2 = GetSoundClass(aann.Item2);
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

		private SoundClass GetSoundClass(Annotation<ShapeNode> ann)
		{
			return _soundClasses.FirstOrDefault(sc => sc.Matches(ann));
		}
	}
}
