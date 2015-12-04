using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;
using SIL.Machine.SequenceAlignment;
using SIL.Machine.Statistics;

namespace SIL.Cog.Application.Export
{
	public class TextVarietyPairExporter : IVarietyPairExporter
	{
		public void Export(Stream stream, IWordAligner aligner, VarietyPair varietyPair)
		{
			using (var writer = new StreamWriter(new NonClosingStreamWrapper(stream)))
			{
				writer.WriteLine("Similarity");
				writer.WriteLine("----------");
				writer.WriteLine("Lexical: {0:p}", varietyPair.LexicalSimilarityScore);
				writer.WriteLine("Phonetic: {0:p}", varietyPair.PhoneticSimilarityScore);
				writer.WriteLine();

				writer.WriteLine("Likely cognates");
				writer.WriteLine("--------------");
				WriteWordPairs(writer, aligner, varietyPair.WordPairs.Where(wp => wp.Cognacy));
				writer.WriteLine();

				writer.WriteLine("Likely non-cognates");
				writer.WriteLine("-------------------");
				WriteWordPairs(writer, aligner, varietyPair.WordPairs.Where(wp => !wp.Cognacy));
				writer.WriteLine();

				writer.WriteLine("Sound correspondences");
				writer.WriteLine("---------------------");
				bool first = true;
				foreach (SoundContext lhs in varietyPair.SoundChangeProbabilityDistribution.Conditions)
				{
					if (!first)
						writer.WriteLine();
					IProbabilityDistribution<Ngram<Segment>> probDist = varietyPair.SoundChangeProbabilityDistribution[lhs];
					FrequencyDistribution<Ngram<Segment>> freqDist = varietyPair.SoundChangeFrequencyDistribution[lhs];
					writer.WriteLine(lhs.ToString());
					foreach (var correspondence in freqDist.ObservedSamples.Select(corr => new {Segment = corr, Probability = probDist[corr], Frequency = freqDist[corr]}).OrderByDescending(corr => corr.Probability))
						writer.WriteLine("{0}: {1:p}, {2}", correspondence.Segment, correspondence.Probability, correspondence.Frequency);
					first = false;
				}
			}
		}

		private static void WriteWordPairs(StreamWriter writer, IWordAligner aligner, IEnumerable<WordPair> wordPairs)
		{
			bool first = true;
			foreach (WordPair pair in wordPairs.OrderByDescending(wp => wp.PhoneticSimilarityScore))
			{
				if (!first)
					writer.WriteLine();
				IWordAlignerResult results = aligner.Compute(pair);
				Alignment<Word, ShapeNode> alignment = results.GetAlignments().First();
				writer.Write(pair.Word1.Meaning.Gloss);
				if (!string.IsNullOrEmpty(pair.Word1.Meaning.Category))
					writer.Write(" ({0})", pair.Word1.Meaning.Category);
				writer.WriteLine();
				writer.Write(alignment.ToString(pair.AlignmentNotes));
				writer.WriteLine("Similarity: {0:p}", pair.PhoneticSimilarityScore);
				first = false;
			}
		}
	}
}
