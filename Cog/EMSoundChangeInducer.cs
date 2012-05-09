using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog
{
	public class EMSoundChangeInducer : IProcessor<VarietyPair>
	{
		private readonly EditDistance _initEditDistance;
		private readonly SoundChangeAline _soundChangeAline;
		private readonly double _threshold;

		public EMSoundChangeInducer(EditDistance initEditDistance, SoundChangeAline soundChangeAline, double threshold)
		{
			_initEditDistance = initEditDistance;
			_soundChangeAline = soundChangeAline;
			_threshold = threshold;
		}

		public void Process(VarietyPair varietyPair)
		{
			bool converged = false;
			for (int i = 0; i < 15 && !converged; i++)
			{
				Dictionary<SoundChange, ExpectedCount> expectedCounts = E(varietyPair, i != 0);
				converged = M(varietyPair, expectedCounts);
			}
		}

		private Dictionary<SoundChange, ExpectedCount> E(VarietyPair pair, bool init)
		{
			var expectedCounts = new Dictionary<SoundChange, ExpectedCount>();
			foreach (WordPair wordPair in pair.WordPairs)
			{
				EditDistanceMatrix editDistanceMatrix = init ? _initEditDistance.Compute(wordPair) : _soundChangeAline.Compute(wordPair);
				Alignment alignment = editDistanceMatrix.GetAlignments().First();
				if (alignment.Score >= _threshold)
				{
					foreach (Tuple<Annotation<ShapeNode>, Annotation<ShapeNode>> possibleLink in alignment.AlignedAnnotations)
					{
						var u = possibleLink.Item1.Type() == CogFeatureSystem.NullType ? new NSegment(Segment.Null)
							: new NSegment(alignment.Shape1.GetNodes(possibleLink.Item1.Span).Select(node => pair.Variety1.Segments[node]));
						var v = possibleLink.Item2.Type() == CogFeatureSystem.NullType ? new NSegment(Segment.Null)
							: new NSegment(alignment.Shape2.GetNodes(possibleLink.Item2.Span).Select(node => pair.Variety2.Segments[node]));

						NaturalClass leftEnv = _soundChangeAline.NaturalClasses.FirstOrDefault(constraint =>
							constraint.FeatureStruct.IsUnifiable(possibleLink.Item1.Span.Start.GetPrev(node => node.Annotation.Type() != CogFeatureSystem.NullType).Annotation.FeatureStruct));
						NaturalClass rightEnv = _soundChangeAline.NaturalClasses.FirstOrDefault(constraint =>
							constraint.FeatureStruct.IsUnifiable(possibleLink.Item1.Span.End.GetNext(node => node.Annotation.Type() != CogFeatureSystem.NullType).Annotation.FeatureStruct));
						SoundChange change = pair.GetSoundChange(leftEnv, u, rightEnv);
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
					foreach (NSegment correspondence in expectedCount.Correspondences)
					{
						double prob = (expectedCount.GetCorrespondenceCount(correspondence) + (1.0 / pair.PossibleCorrespondenceCount)) / (expectedCount.Count + 1.0);
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
			private readonly Dictionary<NSegment, int> _correspondenceCounts;

			public ExpectedCount()
			{
				_correspondenceCounts = new Dictionary<NSegment, int>();
			}

			public int Count
			{
				get { return _count; }
			}

			public IEnumerable<NSegment> Correspondences
			{
				get { return _correspondenceCounts.Keys; }
			}

			public int GetCorrespondenceCount(NSegment correspondence)
			{
				int count;
				if (_correspondenceCounts.TryGetValue(correspondence, out count))
					return count;
				return 0;
			}

			public void Increment(NSegment correspondence)
			{
				_correspondenceCounts.UpdateValue(correspondence, () => 0, count => count + 1);
				_count++;
			}
		}
	}
}
