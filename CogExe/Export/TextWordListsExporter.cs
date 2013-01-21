using System.IO;

namespace SIL.Cog.Export
{
	public class TextWordListsExporter : IWordListsExporter
	{
		public void Export(string path, CogProject project)
		{
			using (var writer = new StreamWriter(path))
			{
				foreach (Sense sense in project.Senses)
				{
					writer.Write("\t");
					writer.Write(sense.Gloss);
				}
				writer.WriteLine();
				foreach (Sense sense in project.Senses)
				{
					writer.Write("\t");
					writer.Write(sense.Category);
				}
				writer.WriteLine();

				foreach (Variety variety in project.Varieties)
				{
					writer.Write(variety.Name);
					foreach (Sense sense in project.Senses)
					{
						writer.Write("\t");
						bool first = true;
						foreach (Word word in variety.Words[sense])
						{
							if (!first)
								writer.Write(",");
							writer.Write(word.StrRep);
							first = false;
						}
					}
					writer.WriteLine();
				}
			}
		}
	}
}
