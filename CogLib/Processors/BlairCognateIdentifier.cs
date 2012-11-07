using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.Processors
{
	public class BlairCognateIdentifier : ProcessorBase<VarietyPair>
	{
		private readonly double _alignmentThreshold;
		private readonly bool _ignoreRegularInsertionDeletion;
		private readonly bool _regularConsEqual;
		private readonly string _alignerID;

		public BlairCognateIdentifier(CogProject project, double alignmentThreshold, bool ignoreRegularInsertionDeletion, bool regularConsEqual, string alignerID)
			: base(project)
		{
			_alignmentThreshold = alignmentThreshold;
			_ignoreRegularInsertionDeletion = ignoreRegularInsertionDeletion;
			_regularConsEqual = regularConsEqual;
			_alignerID = alignerID;
		}

		public double AlignmentThreshold
		{
			get { return _alignmentThreshold; }
		}

		public bool IgnoreRegularInsertionDeletion
		{
			get { return _ignoreRegularInsertionDeletion; }
		}

		public bool RegularConsonantEqual
		{
			get { return _regularConsEqual; }
		}

		public string AlignerID
		{
			get { return _alignerID; }
		}

		public override void Process(VarietyPair varietyPair)
		{
			var correspondenceCounts = new Dictionary<Tuple<string, string>, int>();
			var wordPairs = new List<Tuple<WordPair, Alignment>>();
			IAligner aligner = Project.Aligners[_alignerID];
			foreach (WordPair wordPair in varietyPair.WordPairs)
			{
				IAlignerResult alignerResult = aligner.Compute(wordPair);
				Alignment alignment = alignerResult.GetAlignments().First();
				if (alignment.Score >= _alignmentThreshold)
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
					var u = link.Item1.Type() == CogFeatureSystem.NullType ? new Ngram(Segment.Null)
						: new Ngram(wordPair.Item2.Shape1.GetNodes(link.Item1.Span).Select(node => varietyPair.Variety1.Segments[node]));
					var v = link.Item2.Type() == CogFeatureSystem.NullType ? new Ngram(Segment.Null)
						: new Ngram(wordPair.Item2.Shape2.GetNodes(link.Item2.Span).Select(node => varietyPair.Variety2.Segments[node]));
					string uStr = link.Item1.StrRep();
					string vStr = link.Item2.StrRep();
					int cat = 3;
					if (uStr == vStr)
					{
						cat = 1;
					}
					else if (uStr == "-" || vStr == "-")
					{
						if (AreSegmentsSimilar(varietyPair, u, v) || correspondences.Contains(Tuple.Create(uStr, vStr)))
							cat = _ignoreRegularInsertionDeletion ? 0 : 1;
					}
					else if (u[0].Type == CogFeatureSystem.VowelType && v[0].Type == CogFeatureSystem.VowelType)
					{
						cat = AreSegmentsSimilar(varietyPair, u, v) ? 1 : 2;
					}
					else if (u[0].Type == CogFeatureSystem.ConsonantType && v[0].Type == CogFeatureSystem.ConsonantType)
					{
						if (_regularConsEqual)
						{
							if (correspondences.Contains(Tuple.Create(uStr, vStr)))
								cat = 1;
							else if (AreSegmentsSimilar(varietyPair, u, v))
								cat = 2;
						}
						else
						{
							if (AreSegmentsSimilar(varietyPair, u, v))
								cat = correspondences.Contains(Tuple.Create(uStr, vStr)) ? 1 : 2;
						}
					}

					if (cat > 0 && cat < 3)
					{
						cat1And2Count++;
						if (cat == 1)
							cat1Count++;
					}
					wordPair.Item1.AlignmentNotes.Add(cat == 0 ? "-" : cat.ToString(CultureInfo.InvariantCulture));
					if (cat > 0)
						totalCount++;
				}

				double type1Score = (double) cat1Count / totalCount;
				double type1And2Score = (double) cat1And2Count / totalCount;
				wordPair.Item1.AreCognatePredicted = type1Score >= 0.5 && type1And2Score >= 0.75;
				wordPair.Item1.PhoneticSimilarityScore = wordPair.Item2.Score;
				if (wordPair.Item1.AreCognatePredicted)
					totalCognateCount++;
				totalScore += wordPair.Item1.PhoneticSimilarityScore;
			}

			int wordPairCount = varietyPair.WordPairs.Count;
			varietyPair.PhoneticSimilarityScore = totalScore / wordPairCount;
			varietyPair.LexicalSimilarityScore = (double) totalCognateCount / wordPairCount;
		}

		private bool AreSegmentsSimilar(VarietyPair varietyPair, Ngram u, Ngram v)
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
