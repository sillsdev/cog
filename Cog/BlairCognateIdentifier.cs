using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog
{
	public class BlairCognateIdentifier : IAnalyzer
	{
		private readonly EditDistance _editDistance;
		private readonly double _threshold;

		public BlairCognateIdentifier(EditDistance editDistance, double threshold)
		{
			_editDistance = editDistance;
			_threshold = threshold;
		}

		public void Analyze(VarietyPair varietyPair)
		{
			var correspondenceCounts = new Dictionary<Tuple<string, string>, int>();
			foreach (WordPair wordPair in varietyPair.WordPairs)
			{
				EditDistanceMatrix editDistanceMatrix = _editDistance.Compute(wordPair);
				Alignment alignment = editDistanceMatrix.GetAlignments().First();
				if (alignment.Score >= _threshold)
				{
					foreach (Tuple<ShapeNode, ShapeNode> possibleLink in alignment.AlignedNodes)
					{
						var u = (string) possibleLink.Item1.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
						var v = (string) possibleLink.Item2.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
						if (u != "-" && v != "-" && u != v)
						{
							Tuple<string, string> key = Tuple.Create(u, v);
							int count;
							if (!correspondenceCounts.TryGetValue(key, out count))
								count = 0;
							correspondenceCounts[key] = count + 1;
						}
					}
				}
			}

			var correspondences = new HashSet<Tuple<string, string>>(correspondenceCounts.Where(kvp => kvp.Value >= 3).Select(kvp => kvp.Key));
			double totalScore = 0.0;
			int totalCognateCount = 0;
			foreach (WordPair wordPair in varietyPair.WordPairs)
			{
				EditDistanceMatrix editDistanceMatrix = _editDistance.Compute(wordPair);
				Alignment alignment = editDistanceMatrix.GetAlignments().First();
				int type1Count = 0;
				int type1And2Count = 0;
				foreach (Tuple<ShapeNode, ShapeNode> link in alignment.AlignedNodes)
				{
					var u = (string) link.Item1.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
					var v = (string) link.Item2.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
					if (u == v)
					{
						type1Count++;
						type1And2Count++;
					}
					else if (link.Item1.Annotation.Type == CogFeatureSystem.VowelType && link.Item2.Annotation.Type == CogFeatureSystem.VowelType)
					{
						int delta = _editDistance.Delta(link.Item1, link.Item2);
						if (delta <= 1000)
						{
							if (delta <= 500)
								type1Count++;
							type1And2Count++;
						}
					}
					else if (link.Item1.Annotation.Type == CogFeatureSystem.ConsonantType && link.Item2.Annotation.Type == CogFeatureSystem.ConsonantType)
					{
						int delta = _editDistance.Delta(link.Item1, link.Item2);
						if (delta <= 500)
						{
							if (correspondences.Contains(Tuple.Create(u, v)))
								type1Count++;
							type1And2Count++;
						}
					}
				}

				double type1Score = (double) type1Count / alignment.AlignedNodesCount;
				double type1And2Score = (double) type1And2Count / alignment.AlignedNodesCount;
				wordPair.AreCognates = type1Score >= 0.5 && type1And2Score >= 0.75;
				wordPair.PhoneticSimilarityScore = (type1Score * 0.75) + (type1And2Score * 0.25);
				if (wordPair.AreCognates)
					totalCognateCount++;
				totalScore += wordPair.PhoneticSimilarityScore;
			}

			varietyPair.PhoneticSimilarityScore = totalScore / varietyPair.WordPairCount;
			varietyPair.LexicalSimilarityScore = (double) totalCognateCount / varietyPair.WordPairCount;
		}
	}
}
