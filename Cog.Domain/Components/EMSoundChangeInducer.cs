using System;
using System.Linq;
using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;
using SIL.Machine.SequenceAlignment;
using SIL.Machine.Statistics;

namespace SIL.Cog.Domain.Components
{
	public class EMSoundChangeInducer : IProcessor<VarietyPair>
	{
		private readonly SegmentPool _segmentPool;
		private readonly CogProject _project;

		public EMSoundChangeInducer(SegmentPool segmentPool, CogProject project, string alignerId, string cognateIdentifierId)
		{
			_segmentPool = segmentPool;
			_project = project;
			AlignerId = alignerId;
			CognateIdentifierId = cognateIdentifierId;
		}

		public string AlignerId { get; }
		public string CognateIdentifierId { get; }

		public void Process(VarietyPair varietyPair)
		{
			for (int i = 0; i < 15; i++)
			{
				if (i > 0)
					E(varietyPair);
				if (M(varietyPair))
					break;
			}
		}

		private void E(VarietyPair pair)
		{
			ICognateIdentifier cognateIdentifier = _project.CognateIdentifiers[CognateIdentifierId];
			var cognateCorrCounts = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
			IWordAligner aligner = _project.WordAligners[AlignerId];
			int cognateCount = 0;
			double totalScore = 0;
			foreach (WordPair wordPair in pair.WordPairs)
			{
				IWordAlignerResult alignerResult = aligner.Compute(wordPair);
				cognateIdentifier.UpdatePredictedCognacy(wordPair, alignerResult);
				Alignment<Word, ShapeNode> alignment = alignerResult.GetAlignments().First();
				if (wordPair.Cognacy)
				{
					for (int column = 0; column < alignment.ColumnCount; column++)
					{
						SoundContext lhs = alignment.ToSoundContext(_segmentPool, 0, column, aligner.ContextualSoundClasses);
						Ngram<Segment> corr = alignment[1, column].ToNgram(_segmentPool);
						cognateCorrCounts[lhs].Increment(corr);
					}
					cognateCount++;
				}
				wordPair.PhoneticSimilarityScore = alignment.NormalizedScore;
				totalScore += wordPair.PhoneticSimilarityScore;
			}

			pair.CognateCount = cognateCount;
			pair.CognateSoundCorrespondenceFrequencyDistribution = cognateCorrCounts;
			if (pair.WordPairs.Count == 0)
			{
				pair.LexicalSimilarityScore = 0;
				pair.PhoneticSimilarityScore = 0;
			}
			else
			{
				pair.LexicalSimilarityScore = (double) cognateCount / pair.WordPairs.Count;
				pair.PhoneticSimilarityScore = totalScore / pair.WordPairs.Count;
			}
		}

		private bool M(VarietyPair pair)
		{
			IWordAligner aligner = _project.WordAligners[AlignerId];
			int segmentCount = pair.Variety2.SegmentFrequencyDistribution.ObservedSamples.Count;
			int possCorrCount = aligner.ExpansionCompressionEnabled ? (segmentCount * segmentCount) + segmentCount + 1 : segmentCount + 1;
			var cpd = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(
				pair.CognateSoundCorrespondenceFrequencyDistribution,
				(sc, fd) => new WittenBellProbabilityDistribution<Ngram<Segment>>(fd, possCorrCount));

			bool converged = true;
			if (pair.CognateSoundCorrespondenceProbabilityDistribution == null
				|| pair.CognateSoundCorrespondenceProbabilityDistribution.Conditions.Count != cpd.Conditions.Count)
			{
				converged = false;
			}
			else
			{
				foreach (SoundContext lhs in cpd.Conditions)
				{
					IProbabilityDistribution<Ngram<Segment>> probDist = cpd[lhs];
					IProbabilityDistribution<Ngram<Segment>> oldProbDist;
					if (!pair.CognateSoundCorrespondenceProbabilityDistribution.TryGetProbabilityDistribution(lhs, out oldProbDist)
						|| probDist.Samples.Count != oldProbDist.Samples.Count)
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
				pair.CognateSoundCorrespondenceProbabilityDistribution = cpd;
				pair.DefaultSoundCorrespondenceProbability = 1.0 / possCorrCount;
			}

			return converged;
		}
	}
}
