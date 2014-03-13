using System.IO;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Export
{
	public class TextWordListsExporter : IWordListsExporter
	{
		public void Export(Stream stream, CogProject project)
		{
			using (var writer = new StreamWriter(new NonClosingStreamWrapper(stream)))
			{
				bool categoriesIncluded = false;
				foreach (Meaning meaning in project.Meanings)
				{
					writer.Write("\t");
					writer.Write(meaning.Gloss);
					if (!string.IsNullOrEmpty(meaning.Category))
						categoriesIncluded = true;
				}
				writer.WriteLine();

				if (categoriesIncluded)
				{
					foreach (Meaning meaning in project.Meanings)
					{
						writer.Write("\t");
						writer.Write(meaning.Category);
					}
					writer.WriteLine();
				}

				foreach (Variety variety in project.Varieties)
				{
					writer.Write(variety.Name);
					foreach (Meaning meaning in project.Meanings)
					{
						writer.Write("\t");
						bool first = true;
						foreach (Word word in variety.Words[meaning])
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
