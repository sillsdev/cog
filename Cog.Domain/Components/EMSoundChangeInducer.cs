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
		private readonly string _alignerID;
		private readonly string _cognateIdentifierID;

		public EMSoundChangeInducer(SegmentPool segmentPool, CogProject project, string alignerID, string cognateIdentifierID)
		{
			_segmentPool = segmentPool;
			_project = project;
			_alignerID = alignerID;
			_cognateIdentifierID = cognateIdentifierID;
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
				if (i > 0)
					E(varietyPair);
				if (M(varietyPair))
					break;
			}
		}

		private void E(VarietyPair pair)
		{
			ICognateIdentifier cognateIdentifier = _project.CognateIdentifiers[_cognateIdentifierID];
			var expectedCounts = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
			IWordAligner aligner = _project.WordAligners[_alignerID];
			int cognateCount = 0;
			double totalScore = 0;
			foreach (WordPair wordPair in pair.WordPairs)
			{
				IWordAlignerResult alignerResult = aligner.Compute(wordPair);
				cognateIdentifier.UpdateCognicity(wordPair, alignerResult);
				if (wordPair.AreCognatePredicted)
				{
					Alignment<Word, ShapeNode> alignment = alignerResult.GetAlignments().First();
					for (int column = 0; column < alignment.ColumnCount; column++)
					{
						SoundContext lhs = alignment.ToSoundContext(_segmentPool, 0, column, aligner.ContextualSoundClasses);
						Ngram<Segment> corr = alignment[1, column].ToNgram(_segmentPool);
						expectedCounts[lhs].Increment(corr);
					}
					cognateCount++;
				}
				wordPair.PhoneticSimilarityScore = alignerResult.GetAlignments().First().NormalizedScore;
				totalScore += wordPair.PhoneticSimilarityScore;
			}

			pair.SoundChangeFrequencyDistribution = expectedCounts;
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
			IWordAligner aligner = _project.WordAligners[_alignerID];
			int segmentCount = pair.Variety2.SegmentFrequencyDistribution.ObservedSamples.Count;
			int possCorrCount = aligner.ExpansionCompressionEnabled ? (segmentCount * segmentCount) + segmentCount + 1 : segmentCount + 1;
			var cpd = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(pair.SoundChangeFrequencyDistribution, (sc, fd) => new WittenBellProbabilityDistribution<Ngram<Segment>>(fd, possCorrCount));

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
				pair.SoundChangeProbabilityDistribution = cpd;
				pair.DefaultCorrespondenceProbability = 1.0 / possCorrCount;
			}

			return converged;
		}
	}
}
