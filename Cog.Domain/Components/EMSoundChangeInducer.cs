using System;
using System.Linq;
using SIL.Machine;
using SIL.Machine.NgramModeling;
using SIL.Machine.Statistics;

namespace SIL.Cog.Domain.Components
{
	public class EMSoundChangeInducer : IProcessor<VarietyPair>
	{
		private readonly SegmentPool _segmentPool;
		private readonly CogProject _project;
		private readonly double _initialAlignmentThreshold;
		private readonly string _alignerID;
		private readonly string _cognateIdentifierID;

		public EMSoundChangeInducer(SegmentPool segmentPool, CogProject project, double initialAlignmentThreshold, string alignerID, string cognateIdentifierID)
		{
			_segmentPool = segmentPool;
			_project = project;
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

		public void Process(VarietyPair varietyPair)
		{
			for (int i = 0; i < 15; i++)
			{
				ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>> expectedCounts = E(varietyPair);
				if (M(varietyPair, expectedCounts))
					break;
			}
		}

		private ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>> E(VarietyPair pair)
		{
			if (pair.SoundChangeProbabilityDistribution != null)
			{
				IProcessor<VarietyPair> cognateIdentifier = _project.VarietyPairProcessors[_cognateIdentifierID];
				cognateIdentifier.Process(pair);
			}

			var expectedCounts = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
			IWordAligner aligner = _project.WordAligners[_alignerID];
			foreach (WordPair wordPair in pair.WordPairs)
			{
				if (pair.SoundChangeProbabilityDistribution == null || wordPair.AreCognatePredicted)
				{
					IWordAlignerResult alignerResult = aligner.Compute(wordPair);
					Alignment<Word, ShapeNode> alignment = alignerResult.GetAlignments().First();
					if (pair.SoundChangeProbabilityDistribution != null || alignment.NormalizedScore >= _initialAlignmentThreshold)
					{
						for (int column = 0; column < alignment.ColumnCount; column++)
						{
							SoundContext lhs = alignment.ToSoundContext(_segmentPool, 0, column, wordPair.Word1, aligner.ContextualSoundClasses);
							Ngram<Segment> corr = alignment[1, column].ToNgram(_segmentPool);
							expectedCounts[lhs].Increment(corr);
						}
					}
				}
			}
			return expectedCounts;
		}

		private bool M(VarietyPair pair, ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>> expectedCounts)
		{
			IWordAligner aligner = _project.WordAligners[_alignerID];
			int segmentCount = pair.Variety2.SegmentFrequencyDistributions[SyllablePosition.Anywhere].ObservedSamples.Count;
			int possCorrCount = aligner.ExpansionCompressionEnabled ? (segmentCount * segmentCount) + segmentCount + 1 : segmentCount + 1;
			var cpd = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(expectedCounts, fd => new WittenBellProbabilityDistribution<Ngram<Segment>>(fd, possCorrCount));

			bool converged = true;
			if (pair.SoundChangeProbabilityDistribution == null || pair.SoundChangeProbabilityDistribution.Conditions.Count != cpd.Conditions.Count)
			{
				converged = false;
			}
			else
			{
				foreach (SoundContext lhs in cpd.Conditions)
				{
					IProbabilityDistribution<Ngram<Segment>> probDist = cpd[lhs];
					IProbabilityDistribution<Ngram<Segment>> oldProbDist;
					if (!pair.SoundChangeProbabilityDistribution.TryGetProbabilityDistribution(lhs, out oldProbDist) || probDist.Samples.Count != oldProbDist.Samples.Count)
					{
						converged = false;
						break;
					}

					foreach (Ngram<Segment> correspondence in probDist.Samples)
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
