using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog
{
	public class SoundChangeInducer
	{
		private readonly AlineConfig _alineConfig;

		public SoundChangeInducer(AlineConfig alineConfig)
		{
			_alineConfig = alineConfig;
		}

		public void InduceSoundChanges(IEnumerable<Word> words1, IEnumerable<Word> words2)
		{
			var x = new List<Word>(words1);
			var y = new List<Word>(words2);

			List<string> phonemes1 = x.SelectMany(word => word.Shape,
				(word, node) => (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep)).Concat("-").Distinct().ToList();
			List<string> phonemes2 = y.SelectMany(word => word.Shape,
				(word, node) => (string)node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep)).Concat("-").Distinct().ToList();


			bool converged = false;
			for (int i = 0; i < 15 && !converged; i++)
			{
				E(x, y);
				converged = M();
			}
		}

		private void E(IEnumerable<Word> x, IEnumerable<Word> y)
		{
			foreach (SoundChange pair in _alineConfig.SegmentCorrespondences)
				pair.LinkCount = 0;

			foreach (Tuple<Word, Word> words in x.Zip(y))
			{
				var aline = new Aline(_alineConfig, words.Item1.Shape, words.Item2.Shape);
				Alignment alignment = aline.GetAlignments().First();
				Annotation<ShapeNode> ann1 = alignment.Shape1.Annotations.GetNodes(CogFeatureSystem.StemType).SingleOrDefault();
				Annotation<ShapeNode> ann2 = alignment.Shape2.Annotations.GetNodes(CogFeatureSystem.StemType).SingleOrDefault();
				if (ann1 != null && ann2 != null)
				{
					foreach (Tuple<ShapeNode, ShapeNode> possibleLink in alignment.Shape1.GetNodes(ann1.Span).Zip(alignment.Shape2.GetNodes(ann2.Span)))
					{
						var u = (string) possibleLink.Item1.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
						var v = (string) possibleLink.Item2.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
						SoundChange pair;
						if (!_alineConfig.TryGetSegmentCorrespondence(u, v, out pair))
						{
							pair = new SoundChange(u, v, 0.1);
							_alineConfig.AddSegmentCorrespondence(pair);
						}
						pair.LinkCount++;
					}
				}
			}
		}

		private bool M()
		{
			Dictionary<string, Tuple<int, double>> contextCounts = _alineConfig.SegmentCorrespondences.GroupBy(pair => pair.U)
				.ToDictionary(group => group.Key, group => Tuple.Create(group.Count(), group.Aggregate(0.0, (count, pair) => count + pair.LinkCount)));

			bool converged = true;
			foreach (SoundChange pair in _alineConfig.SegmentCorrespondences)
			{
				Tuple<int, double> stats = contextCounts[pair.U];
				double prob = (pair.LinkCount + (pair.U == pair.V ? 6.0 : 1.1 / stats.Item1)) / (stats.Item2 + 7.1);
				if (Math.Abs(prob - pair.CorrespondenceProbability) > 0.001)
					converged = false;
				pair.CorrespondenceProbability = prob;
			}
			return converged;
		}
	}
}
