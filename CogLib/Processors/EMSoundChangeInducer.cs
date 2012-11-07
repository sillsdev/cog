using System;
using System.Collections.Generic;
using System.Linq;
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
				Dictionary<SoundChange, ExpectedCount> expectedCounts = E(varietyPair);
				converged = M(varietyPair, expectedCounts);
			}
		}

		private Dictionary<SoundChange, ExpectedCount> E(VarietyPair pair)
		{
			var expectedCounts = new Dictionary<SoundChange, ExpectedCount>();
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
						SoundChange change;
						if (!pair.SoundChanges.TryGetValue(lhs, out change))
							change = pair.SoundChanges.Add(lhs);
						ExpectedCount expectedCount = expectedCounts.GetValue(change, () => new ExpectedCount());
						expectedCount.Increment(v);
					}
				}
			}
			return expectedCounts;
		}

		private bool M(VarietyPair pair, Dictionary<SoundChange, ExpectedCount> expectedCounts)
		{
			bool converged = true;
			foreach (SoundChange change in pair.SoundChanges)
			{
				ExpectedCount expectedCount;
				if (expectedCounts.TryGetValue(change, out expectedCount))
				{
					foreach (Ngram correspondence in expectedCount.Correspondences)
					{
						double prob = (expectedCount.GetCorrespondenceCount(correspondence) + (1.0 / pair.SoundChanges.PossibleCorrespondenceCount)) / (expectedCount.Count + 1.0);
						if (Math.Abs(prob - change[correspondence]) > 0.0001)
							converged = false;
						change[correspondence] = prob;
					}
				}
				else
				{
					change.Reset();
				}
			}
			return converged;
		}

		private class ExpectedCount
		{
			private int _count;
			private readonly Dictionary<Ngram, int> _correspondenceCounts;

			public ExpectedCount()
			{
				_correspondenceCounts = new Dictionary<Ngram, int>();
			}

			public int Count
			{
				get { return _count; }
			}

			public IEnumerable<Ngram> Correspondences
			{
				get { return _correspondenceCounts.Keys; }
			}

			public int GetCorrespondenceCount(Ngram correspondence)
			{
				int count;
				if (_correspondenceCounts.TryGetValue(correspondence, out count))
					return count;
				return 0;
			}

			public void Increment(Ngram correspondence)
			{
				_correspondenceCounts.UpdateValue(correspondence, () => 0, count => count + 1);
				_count++;
			}
		}
	}
}
