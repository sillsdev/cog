using System;
using System.Linq;
using SIL.Cog.Statistics;
using SIL.Machine;

namespace SIL.Cog.Components
{
	public class EMSoundChangeInducer : ProcessorBase<VarietyPair>
	{
		private readonly double _initialAlignmentThreshold;
		private readonly string _alignerID;
		private readonly string _cognateIdentifierID;

		public EMSoundChangeInducer(CogProject project, double initialAlignmentThreshold, string alignerID, string cognateIdentifierID)
			: base(project)
		{
			_initialAlignmentThreshold = initialAlignmentThreshold;
			_alignerID = alignerID;
			_cognateIdentifierID = cognateIdentifierID;
		}

		public double InitialAlignmentThreshold
		{
			get { return _initialAlignmentThreshold; }
		}

		public string AlignerID
		{
			get { return _alignerID; }
		}

		public string CognateIdentifierID
		{
			get { return _cognateIdentifierID; }
		}

		public override void Process(VarietyPair varietyPair)
		{
			for (int i = 0; i < 15; i++)
			{
				ConditionalFrequencyDistribution<SoundContext, Ngram> expectedCounts = E(varietyPair);
				if (M(varietyPair, expectedCounts))
					break;
			}
		}

		private ConditionalFrequencyDistribution<SoundContext, Ngram> E(VarietyPair pair)
		{
			if (pair.SoundChangeProbabilityDistribution != null)
			{
				IProcessor<VarietyPair> cognateIdentifier = Project.VarietyPairProcessors[_cognateIdentifierID];
				cognateIdentifier.Process(pair);
			}

			var expectedCounts = new ConditionalFrequencyDistribution<SoundContext, Ngram>();
			IWordAligner aligner = Project.WordAligners[_alignerID];
			foreach (WordPair wordPair in pair.WordPairs)
			{
				IWordAlignerResult alignerResult = aligner.Compute(wordPair);
				Alignment<Word, ShapeNode> alignment = alignerResult.GetAlignments().First();
				if ((pair.SoundChangeProbabilityDistribution == null && alignment.NormalizedScore >= _initialAlignmentThreshold) || (pair.SoundChangeProbabilityDistribution != null && wordPair.AreCognatePredicted))
				{
					for (int column = 0; column < alignment.ColumnCount; column++)
					{
						SoundContext lhs = alignment.ToSoundContext(0, column, wordPair.Word1, aligner.ContextualSoundClasses);
						Ngram corr = alignment[1, column].ToNgram(pair.Variety2);
						expectedCounts[lhs].Increment(corr);
					}
				}
			}
			return expectedCounts;
		}

		private bool M(VarietyPair pair, ConditionalFrequencyDistribution<SoundContext, Ngram> expectedCounts)
		{
			IWordAligner aligner = Project.WordAligners[_alignerID];
			int segmentCount = pair.Variety2.Segments.Count;
			int possCorrCount = aligner.ExpansionCompressionEnabled ? (segmentCount * segmentCount) + segmentCount + 1 : segmentCount + 1;
			var cpd = new ConditionalProbabilityDistribution<SoundContext, Ngram>(expectedCounts, fd => new WittenBellProbabilityDistribution<Ngram>(fd, possCorrCount));

			bool converged = true;
			if (pair.SoundChangeProbabilityDistribution == null || pair.SoundChangeProbabilityDistribution.Conditions.Count != cpd.Conditions.Count)
			{
				converged = false;
			}
			else
			{
				foreach (SoundContext lhs in cpd.Conditions)
				{
					IProbabilityDistribution<Ngram> probDist = cpd[lhs];
					IProbabilityDistribution<Ngram> oldProbDist;
					if (!pair.SoundChangeProbabilityDistribution.TryGetProbabilityDistribution(lhs, out oldProbDist) || probDist.Samples.Count != oldProbDist.Samples.Count)
					{
						converged = false;
						break;
					}

					foreach (Ngram correspondence in probDist.Samples)
					{
						if (Math.Abs(probDist[correspondence] - oldProbDist[correspondence]) > 0.0001)
						{
							converged = false;
							break;
						}
					}

					if (!converged)
						break;
				}
			}

			if (!converged)
			{
				pair.SoundChangeFrequencyDistribution = expectedCounts;
				pair.SoundChangeProbabilityDistribution = cpd;
				pair.DefaultCorrespondenceProbability = 1.0 / possCorrCount;
			}

			return converged;
		}
	}
}
