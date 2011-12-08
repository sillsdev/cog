using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SIL.Machine;

namespace SIL.Cog
{
	class Program
	{
		public static int Main(string[] args)
		{
			var spanFactory = new SpanFactory<ShapeNode>((x, y) => x.CompareTo(y), (start, end) => start.GetNodes(end).Count(), true);
			var config = new AlineConfig(spanFactory, "data\\ipa-aline.xml");
			config.Load();

			WordIndex index;
			if (!CreateWordIndex(args[0], config.Segmenter, out index))
			{
				Console.WriteLine("The specified file contains no data.");
				return -1;
			}

			string lang1 = args[1];
			string lang2 = args[2];

			var writer = new StreamWriter(args[3]);
			var pairLinkCounts = new Dictionary<Tuple<string, string>, int>();
			var lang1LinkCounts = new Dictionary<string, int>();
			var lang2LinkCounts = new Dictionary<string, int>();
			foreach (string gloss in index.Glosses)
			{
				Word lang1Word, lang2Word;
				if (index.TryGetWord(gloss, lang1, out lang1Word) && index.TryGetWord(gloss, lang2, out lang2Word))
				{
					var aline = new Aline(config, lang1Word.Shape, lang2Word.Shape);
					Alignment alignment = aline.GetAlignments().First();
					Annotation<ShapeNode> ann1 = alignment.Shape1.Annotations.GetNodes(CogFeatureSystem.StemType).SingleOrDefault();
					Annotation<ShapeNode> ann2 = alignment.Shape2.Annotations.GetNodes(CogFeatureSystem.StemType).SingleOrDefault();
					if (ann1 != null && ann2 != null)
					{
						foreach (Tuple<ShapeNode, ShapeNode> possibleLink in alignment.Shape1.GetNodes(ann1.Span).Zip(alignment.Shape2.GetNodes(ann2.Span)))
						{
							var u = (string) possibleLink.Item1.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
							var v = (string) possibleLink.Item2.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
							if (possibleLink.Item1.Annotation.Type != CogFeatureSystem.NullType && possibleLink.Item2.Annotation.Type != CogFeatureSystem.NullType)
							{
								Tuple<string, string> key = Tuple.Create(u, v);
								if (!pairLinkCounts.ContainsKey(key))
									pairLinkCounts[key] = 0;
								pairLinkCounts[key]++;
							}

							if (possibleLink.Item1.Annotation.Type != CogFeatureSystem.NullType)
							{
								if (!lang1LinkCounts.ContainsKey(u))
									lang1LinkCounts[u] = 0;
								lang1LinkCounts[u]++;
							}

							if (possibleLink.Item2.Annotation.Type != CogFeatureSystem.NullType)
							{
								if (!lang2LinkCounts.ContainsKey(v))
									lang2LinkCounts[v] = 0;
								lang2LinkCounts[v]++;
							}
						}
					}
				}
			}

			foreach (KeyValuePair<Tuple<string, string>, int> pairLinkCount in pairLinkCounts)
			{
				if (pairLinkCount.Key.Item1 != pairLinkCount.Key.Item2 && pairLinkCount.Value > 1)
				{
					double lang1Prob = (double) pairLinkCount.Value/lang1LinkCounts[pairLinkCount.Key.Item1];
					double lang2Prob = (double) pairLinkCount.Value/lang2LinkCounts[pairLinkCount.Key.Item2];
					double prob = lang1Prob * lang2Prob;
					var pair = new SegmentPair(pairLinkCount.Key.Item1, pairLinkCount.Key.Item2, pairLinkCount.Value, prob);
					config.AddSegmentCorrespondence(pair);
				}
			}

			var sb = new StringBuilder();
			sb.AppendFormat("{0}\t{1}\tProb\tLink Count", lang1, lang2);
			sb.AppendLine();
			foreach (SegmentPair correspondence in config.SegmentCorrespondences.Where(corr => corr.CorrespondenceProbability > 0.02).OrderByDescending(corr => corr.CorrespondenceProbability))
			{
				sb.AppendFormat("{0}\t{1}\t{2:0.0####}\t{3}", correspondence.U, correspondence.V, correspondence.CorrespondenceProbability, correspondence.LinkCount);
				sb.AppendLine();
			}
			writer.WriteLine(sb);

			double totalScore = 0.0;
			int totalWordCount = 0;
			var cognates = new List<Tuple<Alignment, string>>();
			foreach (string gloss in index.Glosses)
			{
				Word lang1Word, lang2Word;
				if (index.TryGetWord(gloss, lang1, out lang1Word) && index.TryGetWord(gloss, lang2, out lang2Word))
				{
					var aline = new Aline(config, lang1Word.Shape, lang2Word.Shape);
					Alignment alignment = aline.GetAlignments().First();
					totalScore += alignment.Score;
					if (alignment.Score >= 0.75)
						cognates.Add(Tuple.Create(alignment, gloss));
					totalWordCount++;
				}
			}

			foreach (Tuple<Alignment, string> cognate in cognates.OrderByDescending(cognate => cognate.Item1.Score))
			{
				writer.WriteLine(cognate.Item2);
				writer.Write(cognate.Item1.ToString());
				writer.WriteLine("Score: {0}", cognate.Item1.Score);
				writer.WriteLine();
			}

			writer.WriteLine("Lexical Similarity: {0}", (double) cognates.Count / totalWordCount);
			writer.WriteLine("Avg. Similarity Score: {0}", totalScore / totalWordCount);

			writer.Close();

			return 0;
		}

		private static bool CreateWordIndex(string wordFilePath, Segmenter segmenter, out WordIndex index)
		{
			index = new WordIndex();
			using (var file = new StreamReader(wordFilePath))
			{
				string line = file.ReadLine();
				if (line == null)
				{
					index = null;
					return false;
				}
				string[] languages = line.Split('\t');
				while ((line = file.ReadLine()) != null)
				{
					string[] gloss = line.Split('\t');
					for (int i = 1; i < gloss.Length; i++)
					{
						if (!string.IsNullOrEmpty(gloss[i]))
						{
							Shape shape;
							if (segmenter.ToShape(gloss[i], out shape))
								index.Add(new Word(shape, languages[i], gloss[0]));
						}
					}
				}

				file.Close();
			}
			return true;
		}

#if COMMENTOUT
		private static void AlignSegments(Shape shape1, Shape shape2, out IList<string> alignedSegments1, out IList<string> alignedSegments2)
		{
			int i, j;
			var d = new int[segments1.Count, segments2.Count];
			for (i = 0; i < segments1.Count; i++)
				d[i, 0] = i;
			for (j = 0; j < segments2.Count; j++)
				d[0, j] = j;

			for (i = 1; i < segments1.Count; i++)
			{
				for (j = 1; j < segments2.Count; j++)
				{
					if (segments1[i] == segments2[j])
						d[i, j] = d[i - 1, j - 1];
					else
						d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + 1);
				}
			}

			alignedSegments1 = new List<string>();
			alignedSegments2 = new List<string>();
			i = segments1.Count - 1;
			j = segments2.Count - 1;
			while (i >= 0 || j >= 0)
			{
				int cur = d[i, j];
				int subst = i == 0 || j == 0 ? int.MaxValue : d[i - 1, j - 1];
				int insert = j == 0 ? int.MaxValue : d[i, j - 1];
				int del = i == 0 ? int.MaxValue : d[i - 1, j];
				if ((i == 0 && j == 0) || (subst <= insert && subst <= del && (subst == cur || subst == cur - 1)))
				{
					alignedSegments1.Insert(0, segments1[i]);
					alignedSegments2.Insert(0, segments2[j]);
					i--;
					j--;
				}
				else if (insert <= del && (insert == cur || insert == cur - 1))
				{
					alignedSegments1.Insert(0, null);
					alignedSegments2.Insert(0, segments2[j]);
					j--;
				}
				else
				{
					alignedSegments1.Insert(0, segments1[i]);
					alignedSegments2.Insert(0, null);
					i--;
				}
			}
		}
#endif
	}
}
