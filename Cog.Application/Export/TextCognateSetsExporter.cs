using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.Cog.Domain;
using SIL.Machine.Clusterers;

namespace SIL.Cog.Application.Export
{
	public class TextCognateSetsExporter : ICognateSetsExporter
	{
		public void Export(Stream stream, CogProject project)
		{
			using (var writer = new StreamWriter(new NonClosingStreamWrapper(stream)))
			{
				var meaningClusters = new Dictionary<Meaning, List<Cluster<Word>>>();
				foreach (Meaning meaning in project.Meanings)
				{
					writer.Write("\t");
					writer.Write(meaning.Gloss);
					meaningClusters[meaning] = project.GenerateCognateSets(meaning).ToList();

				}
				writer.WriteLine();
				foreach (Meaning meaning in project.Meanings)
				{
					writer.Write("\t");
					writer.Write(meaning.Category);
				}
				writer.WriteLine();

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
								writer.Write(',');
							int i = meaningClusters[meaning].FindIndex(set => set.DataObjects.Contains(word)) + 1;
							writer.Write(i);
							first = false;
						}
					}
					writer.WriteLine();
				}
			}
		}
	}
}
