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
				foreach (Sense sense in project.Senses)
				{
					writer.Write("\t");
					writer.Write(sense.Gloss);
					if (!string.IsNullOrEmpty(sense.Category))
						categoriesIncluded = true;
				}
				writer.WriteLine();

				if (categoriesIncluded)
				{
					foreach (Sense sense in project.Senses)
					{
						writer.Write("\t");
						writer.Write(sense.Category);
					}
					writer.WriteLine();
				}

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
