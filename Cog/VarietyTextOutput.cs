using System.IO;
using System.Linq;

namespace SIL.Cog
{
	public class VarietyTextOutput : IProcessor<Variety>
	{
		private readonly string _path;

		public VarietyTextOutput(string path)
		{
			_path = path;
		}

		public void Process(Variety variety)
		{
			using (var writer = new StreamWriter(Path.Combine(_path, string.Format("{0}.txt", variety.ID))))
			{
				writer.WriteLine("Affix\tCategory\tScore");
				foreach (Affix affix in variety.Affixes.OrderByDescending(a => a.Score))
					writer.WriteLine("{0}\t{1}\t{2:0.0####}", affix, string.IsNullOrEmpty(affix.Category) ? "?" : affix.Category, affix.Score);
				writer.WriteLine();

				foreach (Word word in variety.Words)
				{
					writer.WriteLine(word.Gloss);
					writer.WriteLine(word.Category);
					writer.WriteLine(word.ToString());
					writer.WriteLine();
				}
			}
		}
	}
}
