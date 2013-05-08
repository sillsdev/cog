using System.IO;
using System.Linq;

namespace SIL.Cog.Components
{
	public class VarietyTextOutput : IProcessor<Variety>
	{
		private readonly string _fileName;

		public VarietyTextOutput(string fileName)
		{
			_fileName = fileName;
		}

		public string FileName
		{
			get { return _fileName; }
		}

		public void Process(Variety variety)
		{
			using (var writer = new StreamWriter(Path.Combine(_fileName, string.Format("{0}.txt", variety))))
			{
				writer.WriteLine("Seg\tProb");
				foreach (Segment seg in variety.Segments.OrderByDescending(s => variety.SegmentProbabilityDistribution[s]))
					writer.WriteLine("{0}\t{1:0.0####}", seg.StrRep, variety.SegmentProbabilityDistribution[seg]);
				writer.WriteLine();

				if (variety.Affixes.Count > 0)
				{
					writer.WriteLine("Affix\tCategory\tScore");
					foreach (Affix affix in variety.Affixes.OrderByDescending(a => a.Score))
						writer.WriteLine("{0}\t{1}\t{2:0.0####}", affix, string.IsNullOrEmpty(affix.Category) ? "?" : affix.Category, affix.Score);
					writer.WriteLine();
				}

				foreach (Word word in variety.Words)
				{
					writer.WriteLine(word.Sense.Gloss);
					if (!string.IsNullOrEmpty(word.Sense.Category))
						writer.WriteLine(word.Sense.Category);
					writer.WriteLine(word.ToString());
					writer.WriteLine();
				}
			}
		}
	}
}
