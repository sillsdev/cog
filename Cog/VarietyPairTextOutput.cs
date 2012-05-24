using System.IO;
using System.Linq;
using System.Text;

namespace SIL.Cog
{
	public class VarietyPairTextOutput : IProcessor<VarietyPair>
	{
		private readonly string _path;
		private readonly EditDistance _editDistance;

		public VarietyPairTextOutput(string path, EditDistance editDistance)
		{
			_path = path;
			_editDistance = editDistance;
		}

		public void Process(VarietyPair varietyPair)
		{
			using (var writer = new StreamWriter(Path.Combine(_path, string.Format("{0}+{1}.txt", varietyPair.Variety1, varietyPair.Variety2))))
			{
				writer.WriteLine("Lexical Similarity: {0:0.0####}", varietyPair.LexicalSimilarityScore);
				writer.WriteLine("Phonetic Similarity: {0:0.0####}", varietyPair.PhoneticSimilarityScore);
				writer.WriteLine();

				var sb = new StringBuilder();
				foreach (SoundChange change in varietyPair.SoundChanges)
				{
					sb.AppendLine(change.ToString());
					sb.AppendLine("Segment\tProb");
					foreach (var correspondence in change.ObservedCorrespondences.Select(corr => new {Phone = corr, Probability = change[corr]}).OrderByDescending(corr => corr.Probability))
					{
						sb.AppendFormat("{0}\t{1:0.0####}", correspondence.Phone, correspondence.Probability);
						sb.AppendLine();
					}
					sb.AppendLine();
				}
				writer.WriteLine(sb);

				foreach (WordPair pair in varietyPair.WordPairs.OrderByDescending(wp => wp.PhoneticSimilarityScore))
				{
					EditDistanceMatrix matrix = _editDistance.Compute(pair);
					Alignment alignment = matrix.GetAlignments().First();
					writer.WriteLine(pair.Word1.Sense.Gloss);
					if (!string.IsNullOrEmpty(pair.Word1.Sense.Category))
						writer.WriteLine(pair.Word1.Sense.Category);
					writer.Write(alignment.ToString(pair.AlignmentNotes));
					writer.WriteLine("Score: {0:0.0####}", pair.PhoneticSimilarityScore);
					if (pair.AreCognatesPredicted)
						writer.WriteLine("***Cognate***");
					writer.WriteLine();
				}
			}
		}
	}
}
