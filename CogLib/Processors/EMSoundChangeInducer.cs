using System;
using System.Linq;
using SIL.Cog.Statistics;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.Processors
{
	public class EMSoundChangeInducer : ProcessorBase<VarietyPair>
	{
		private readonly double _alignmentThreshold;
		private readonly string _alignerID;

		public EMSoundChangeInducer(CogProject project, double alignmentThreshold, string alignerID)
			: base(project)
		{
			_alignmentThreshold = alignmentThreshold;
			_alignerID = alignerID;
		}

		public double AlignmentThreshold
		{
			get { return _alignmentThreshold; }
		}

		public string AlignerID
		{
			get { return _alignerID; }
		}

		public override void Process(VarietyPair varietyPair)
		{
			bool converged = false;
			for (int i = 0; i < 15 && !converged; i++)
			{
				ConditionalFrequencyDistribution<SoundChangeLhs, Ngram> expectedCounts = E(varietyPair);
				converged = M(varietyPair, expectedCounts);
			}
		}

		private ConditionalFrequencyDistribution<SoundChangeLhs, Ngram> E(VarietyPair pair)
		{
			var expectedCounts = new ConditionalFrequencyDistribution<SoundChangeLhs, Ngram>();
			IAligner aligner = Project.Aligners[_alignerID];
			foreach (WordPair wordPair in pair.WordPairs)
			{
				IAlignerResult alignerResult = aligner.Compute(wordPair);
				Alignment alignment = alignerResult.GetAlignments().First();
				if (alignment.Score >= _alignmentThreshold)
				{
					foreach (Tuple<Annotation<ShapeNode>, Annotation<ShapeNode>> possibleLink in alignment.AlignedAnnotations)
					{
						var u = possibleLink.Item1.Type() == CogFeatureSystem.NullType ? new Ngram(Segment.Null)
							: new Ngram(alignment.Shape1.GetNodes(possibleLink.Item1.Span).Select(node => pair.Variety1.Segments[node]));
						var v = possibleLink.Item2.Type() == CogFeatureSystem.NullType ? new Ngram(Segment.Null)
							: new Ngram(alignment.Shape2.GetNodes(possibleLink.Item2.Span).Select(node => pair.Variety2.Segments[node]));

						NaturalClass leftEnv = aligner.NaturalClasses.FirstOrDefault(constraint =>
							constraint.FeatureStruct.IsUnifiable(possibleLink.Item1.Span.Start.GetPrev(node => node.Annotation.Type() != CogFeatureSystem.NullType).Annotation.FeatureStruct));
						NaturalClass rightEnv = aligner.NaturalClasses.FirstOrDefault(constraint =>
							constraint.FeatureStruct.IsUnifiable(possibleLink.Item1.Span.End.GetNext(node => node.Annotation.Type() != CogFeatureSystem.NullType).Annotation.FeatureStruct));

						var lhs = new SoundChangeLhs(leftEnv, u, rightEnv);
						expectedCounts[lhs].Increment(v);
					}
				}
			}
			return expectedCounts;
		}

		private bool M(VarietyPair pair, ConditionalFrequencyDistribution<SoundChangeLhs, Ngram> expectedCounts)
		{
			IAligner aligner = Project.Aligners[_alignerID];
			int segmentCount = pair.Variety2.Segments.Count;
			int possCorrCount = aligner.SupportsExpansionCompression ? (segmentCount * segmentCount) + segmentCount + 1 : segmentCount + 1;
			var cpd = new ConditionalProbabilityDistribution<SoundChangeLhs, Ngram>(expectedCounts, fd => new WittenBellProbabilityDistribution<Ngram>(fd, possCorrCount));

			bool converged = true;
			if (pair.SoundChanges == null || pair.SoundChanges.Conditions.Count != cpd.Conditions.Count)
			{
				converged = false;
			}
			else
			{
				foreach (SoundChangeLhs lhs in cpd.Conditions)
				{
					IProbabilityDistribution<Ngram> probDist = cpd[lhs];
					IProbabilityDistribution<Ngram> oldProbDist;
					if (!pair.SoundChanges.TryGetProbabilityDistribution(lhs, out oldProbDist) || probDist.Samples.Count != oldProbDist.Samples.Count)
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
			pair.SoundChanges = cpd;
			pair.DefaultCorrespondenceProbability = 1.0 / possCorrCount;
			return converged;
		}
	}
}
