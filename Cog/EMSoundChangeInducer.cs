using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog
{
	public class EMSoundChangeInducer : IAnalyzer
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

		public void Analyze(VarietyPair varietyPair)
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
					foreach (Tuple<ShapeNode, ShapeNode> possibleLink in alignment.AlignedNodes)
					{
						var u = (string) possibleLink.Item1.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
						var v = (string) possibleLink.Item2.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
						if (u != "-" && v != "-")
						{
							NaturalClass leftEnv = _soundChangeAline.NaturalClasses.FirstOrDefault(constraint =>
								constraint.FeatureStruct.IsUnifiable(possibleLink.Item1.GetPrev(node => node.Annotation.Type != CogFeatureSystem.NullType).Annotation.FeatureStruct));
							NaturalClass rightEnv = _soundChangeAline.NaturalClasses.FirstOrDefault(constraint =>
								constraint.FeatureStruct.IsUnifiable(possibleLink.Item1.GetNext(node => node.Annotation.Type != CogFeatureSystem.NullType).Annotation.FeatureStruct));
							SoundChange change;
							if (!pair.TryGetSoundChange(leftEnv, u, rightEnv, out change))
								change = pair.AddSoundChange(leftEnv, u, rightEnv);

							ExpectedCount expectedCount;
							if (!expectedCounts.TryGetValue(change, out expectedCount))
							{
								expectedCount = new ExpectedCount();
								expectedCounts[change] = expectedCount;
							}
							expectedCount.Increment(v);
						}
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
					foreach (string correspondence in expectedCount.Correspondences)
					{
						double prob = (expectedCount.GetCorrespondenceCount(correspondence) + (1.0 / change.CorrespondenceCount)) / (expectedCount.Count + 1.0);
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
			private readonly Dictionary<string, int> _correspondenceCounts;

			public ExpectedCount()
			{
				_correspondenceCounts = new Dictionary<string, int>();
			}

			public int Count
			{
				get { return _count; }
			}

			public IEnumerable<string> Correspondences
			{
				get { return _correspondenceCounts.Keys; }
			}

			public int GetCorrespondenceCount(string correspondence)
			{
				int count;
				if (_correspondenceCounts.TryGetValue(correspondence, out count))
					return count;
				return 0;
			}

			public void Increment(string correspondence)
			{
				int count;
				if (!_correspondenceCounts.TryGetValue(correspondence, out count))
					count = 0;
				_correspondenceCounts[correspondence] = count + 1;
				_count++;
			}
		}
	}
}
