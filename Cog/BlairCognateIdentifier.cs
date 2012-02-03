using System;
using System.Collections.Generic;
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
						if (possibleLink.Item1.Type == CogFeatureSystem.NullType || possibleLink.Item2.Type == CogFeatureSystem.NullType)
							continue;

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
				int type1Count = 0;
				int type1And2Count = 0;
				int totalCount = 0;
				foreach (Tuple<Annotation<ShapeNode>, Annotation<ShapeNode>> link in wordPair.Item2.AlignedAnnotations)
				{
					if (link.Item1.Type == CogFeatureSystem.NullType || link.Item2.Type == CogFeatureSystem.NullType)
						continue;

					Phoneme u1 = varietyPair.Variety1.GetPhoneme(link.Item1.Span.Start);
					Phoneme u2 = link.Item1.Span.Length == 1 ? null : varietyPair.Variety1.GetPhoneme(link.Item1.Span.End);
					Phoneme v1 = varietyPair.Variety2.GetPhoneme(link.Item2.Span.Start);
					Phoneme v2 = link.Item2.Span.Length == 1 ? null : varietyPair.Variety2.GetPhoneme(link.Item2.Span.End);
					string uStr = link.Item1.StrRep();
					string vStr = link.Item2.StrRep();
					if (uStr == vStr)
					{
						type1Count++;
						type1And2Count++;
					}
					else if (u1.Type == CogFeatureSystem.VowelType && v1.Type == CogFeatureSystem.VowelType)
					{
						int cat = GetVowelCategory(varietyPair, u1, v1, v2);
						if (u2 != null && cat != 1)
							cat = Math.Min(cat, GetVowelCategory(varietyPair, u2, v1, v2));

						if (cat <= 2)
						{
							if (cat == 1)
								type1Count++;
							type1And2Count++;
						}
					}
					else if (u1.Type == CogFeatureSystem.ConsonantType && v1.Type == CogFeatureSystem.ConsonantType)
					{
						if (AreConsonantsSimilar(varietyPair, u1, v1, v2) || (u2 != null && AreConsonantsSimilar(varietyPair, u2, v1, v2)))
						{
							if (correspondences.Contains(Tuple.Create(uStr, vStr)))
								type1Count++;
							type1And2Count++;
						}
					}
					totalCount++;
				}

				double type1Score = (double) type1Count / totalCount;
				double type1And2Score = (double) type1And2Count / totalCount;
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

		private int GetVowelCategory(VarietyPair varietyPair, Phoneme u, Phoneme v1, Phoneme v2)
		{
			IReadOnlySet<Phoneme> cat1 = varietyPair.GetSimilarPhonemes(u);
			if (cat1.Contains(v1) || (v2 != null && cat1.Contains(v2)))
				return 1;

			foreach (Phoneme ph in cat1)
			{
				IReadOnlySet<Phoneme> cat2 = varietyPair.GetSimilarPhonemes(ph);
				if (cat2.Contains(v1) || (v2 != null && cat2.Contains(v2)))
					return 2;
			}

			return 3;
		}

		private bool AreConsonantsSimilar(VarietyPair varietyPair, Phoneme u, Phoneme v1, Phoneme v2)
		{
			IReadOnlySet<Phoneme> phonemes = varietyPair.GetSimilarPhonemes(u);
			return phonemes.Contains(v1) || phonemes.Contains(v2);
		}
	}
}
