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
			var config = new AlineConfig(spanFactory, ".\\xml\\ipa-aline.xml");
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
			HashSet<string> lang1Segments = GetUniqueSegments(index.GetLanguageWords(lang1));
			HashSet<string> lang2Segments = GetUniqueSegments(index.GetLanguageWords(lang2));

			var pairs = new HashSet<SegmentPair>();
			foreach (string u in lang1Segments)
			{
				foreach (string v in lang2Segments)
				{
					int aCoocur = 0;
					int bCoocur = 0;
					int cCoocur = 0;
					int dCoocur = 0;
					foreach (string gloss in index.Glosses)
					{
						Word lang1Word, lang2Word;
						if (index.TryGetWord(gloss, lang1, out lang1Word) && index.TryGetWord(gloss, lang2, out lang2Word))
						{
							int uFreq = lang1Word.Shape.Count(seg => seg.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep).Contains(u));
							int vFreq = lang2Word.Shape.Count(seg => seg.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep).Contains(v));
							int notUFreq = lang1Word.Shape.Count(seg => !seg.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep).Contains(u));
							int notVFreq = lang2Word.Shape.Count(seg => !seg.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep).Contains(v));

							aCoocur += Math.Min(uFreq, vFreq);
							bCoocur += Math.Min(notUFreq, vFreq);
							cCoocur += Math.Min(uFreq, notVFreq);
							dCoocur += Math.Min(notUFreq, notVFreq);
						}
					}

					pairs.Add(new SegmentPair(u, v, aCoocur) { Score = aCoocur == 0 ? double.NegativeInfinity : Stats.LogLikelihoodRatio(aCoocur, bCoocur, cCoocur, dCoocur) });
				}
			}

			double threshold = 0.0;
			bool converged = false;
			var correspondences = new Dictionary<Tuple<string, string>, SegmentPair>();
			while (!converged)
			{
				correspondences.Clear();
				foreach (SegmentPair pair in pairs.OrderByDescending(pair => pair.Score))
				{
					if (pair.Score >= threshold)
						correspondences[Tuple.Create(pair.U, pair.V)] = pair;
					pair.LinkCount = 0;
				}

				int totalLinkCount = 0;
				foreach (string gloss in index.Glosses)
				{
					Word lang1Word, lang2Word;
					if (index.TryGetWord(gloss, lang1, out lang1Word) && index.TryGetWord(gloss, lang2, out lang2Word))
					{
						var aline = new Aline(config, lang1Word.Shape, lang2Word.Shape);
						Alignment alignment = aline.GetAlignment();
						Annotation<ShapeNode> ann1 = alignment.Shape1.Annotations.GetNodes(CogFeatureSystem.StemType).SingleOrDefault();
						Annotation<ShapeNode> ann2 = alignment.Shape2.Annotations.GetNodes(CogFeatureSystem.StemType).SingleOrDefault();
						if (ann1 != null && ann2 != null)
						{
							foreach (Tuple<ShapeNode, ShapeNode> possibleLink in alignment.Shape1.GetNodes(ann1.Span).Zip(alignment.Shape2.GetNodes(ann2.Span)))
							{
								string u = possibleLink.Item1.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep).Values.Single();
								string v = possibleLink.Item2.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep).Values.Single();
								SegmentPair pair;
								if (correspondences.TryGetValue(Tuple.Create(u, v), out pair))
								{
									pair.LinkCount++;
									totalLinkCount++;
								}
							}
						}
					}
				}

				converged = true;
				foreach (SegmentPair pair in pairs)
				{
					double newProb = (double)pair.LinkCount / totalLinkCount;
					if (Math.Abs(newProb - pair.CorrespondenceProbability) > 0.0001)
						converged = false;
					pair.CorrespondenceProbability = newProb;
					pair.Score = Math.Log(pair.CorrespondenceProbability);
				}
				threshold = Math.Log(3.0 / totalLinkCount);
			}

			var sb = new StringBuilder();
			sb.AppendFormat("{0}\t{1}\tProb\tLink Count", lang1, lang2);
			sb.AppendLine();
			foreach (SegmentPair pair in correspondences.Values.OrderByDescending(pair => pair.CorrespondenceProbability))
			{
				if (pair.U != pair.V)
				{
					sb.AppendFormat("{0}\t{1}\t{2}\t{3}", pair.U, pair.V, pair.CorrespondenceProbability, pair.LinkCount);
					sb.AppendLine();
					config.AddSegmentPair(pair);
				}
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
					Alignment alignment = aline.GetAlignment();
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

		private static HashSet<string> GetUniqueSegments(IEnumerable<Word> words)
		{
			return new HashSet<string>(from word in words
									   from node in word.Shape
									   where node.Annotation.Type.IsOneOf(CogFeatureSystem.ConsonantType, CogFeatureSystem.VowelType)
									   select node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep).Values.Single());
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
