using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog
{
	public class BlairCognateIdentifier : IProcessor<VarietyPair>
	{
		private readonly EditDistance _editDistance;
		private readonly double _threshold;

		public BlairCognateIdentifier(EditDistance editDistance, double threshold)
		{
			_editDistance = editDistance;
			_threshold = threshold;
		}

		public void Process(VarietyPair varietyPair)
		{
			var correspondenceCounts = new Dictionary<Tuple<string, string>, int>();
			var wordPairs = new List<Tuple<WordPair, Alignment>>();
			foreach (WordPair wordPair in varietyPair.WordPairs)
			{
				EditDistanceMatrix editDistanceMatrix = _editDistance.Compute(wordPair);
				Alignment alignment = editDistanceMatrix.GetAlignments().First();
				if (alignment.Score >= _threshold)
				{
					foreach (Tuple<Annotation<ShapeNode>, Annotation<ShapeNode>> possibleLink in alignment.AlignedAnnotations)
					{
						string uStr = possibleLink.Item1.StrRep();
						string vStr = possibleLink.Item2.StrRep();
						if (uStr != vStr)
						{
							Tuple<string, string> key = Tuple.Create(uStr, vStr);
							correspondenceCounts.UpdateValue(key, () => 0, count => count + 1);
						}
					}
				}
				wordPairs.Add(Tuple.Create(wordPair, alignment));
			}

			var correspondences = new HashSet<Tuple<string, string>>(correspondenceCounts.Where(kvp => kvp.Value >= 3).Select(kvp => kvp.Key));
			double totalScore = 0.0;
			int totalCognateCount = 0;
			foreach (Tuple<WordPair, Alignment> wordPair in wordPairs)
			{
				int cat1Count = 0;
				int cat1And2Count = 0;
				int totalCount = 0;
				foreach (Tuple<Annotation<ShapeNode>, Annotation<ShapeNode>> link in wordPair.Item2.AlignedAnnotations)
				{
					var u = new NSegment(link.Item1.Type() == CogFeatureSystem.NullType ? Enumerable.Empty<Segment>()
						: wordPair.Item2.Shape1.GetNodes(link.Item1.Span).Select(node => varietyPair.Variety1.GetSegment(node)));
					var v = new NSegment(link.Item2.Type() == CogFeatureSystem.NullType ? Enumerable.Empty<Segment>()
						: wordPair.Item2.Shape2.GetNodes(link.Item2.Span).Select(node => varietyPair.Variety2.GetSegment(node)));
					string uStr = link.Item1.StrRep();
					string vStr = link.Item2.StrRep();
					int cat = 3;
					if (uStr == vStr)
					{
						cat = 1;
					}
					else if (u.Count == 0 || v.Count == 0)
					{
						if (correspondences.Contains(Tuple.Create(uStr, vStr)))
							cat = 1;
					}
					else if (u[0].Type == CogFeatureSystem.VowelType && v[0].Type == CogFeatureSystem.VowelType)
					{
						cat = AreSegmentsSimilar(varietyPair, u, v) ? 1 : 2;
					}
					else if (u[0].Type == CogFeatureSystem.ConsonantType && v[0].Type == CogFeatureSystem.ConsonantType)
					{
						if (AreSegmentsSimilar(varietyPair, u, v))
							cat = correspondences.Contains(Tuple.Create(uStr, vStr)) ? 1 : 2;
					}

					if (cat < 3)
					{
						cat1And2Count++;
						if (cat == 1)
							cat1Count++;
					}
					wordPair.Item1.AlignmentNotes.Add(cat.ToString(CultureInfo.InvariantCulture));
					totalCount++;
				}

				double type1Score = (double) cat1Count / totalCount;
				double type1And2Score = (double) cat1And2Count / totalCount;
				wordPair.Item1.AreCognatesPredicted = type1Score >= 0.5 && type1And2Score >= 0.75;
				wordPair.Item1.PhoneticSimilarityScore = (type1Score * 0.75) + (type1And2Score * 0.25);
				if (wordPair.Item1.AreCognatesPredicted)
					totalCognateCount++;
				totalScore += wordPair.Item1.PhoneticSimilarityScore;
			}

			int wordPairCount = varietyPair.WordPairs.Count;
			varietyPair.PhoneticSimilarityScore = totalScore / wordPairCount;
			varietyPair.LexicalSimilarityScore = (double) totalCognateCount / wordPairCount;
		}

		private bool AreSegmentsSimilar(VarietyPair varietyPair, NSegment u, NSegment v)
		{
			foreach (Segment uSeg in u)
			{
				IReadOnlySet<Segment> segments = varietyPair.GetSimilarSegments(uSeg);
				foreach (Segment vSeg in v)
				{
					if (segments.Contains(vSeg))
						return true;
				}
			}

			return false;
		}
	}
}
