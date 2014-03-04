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
				var senseClusters = new Dictionary<Sense, List<Cluster<Word>>>();
				foreach (Sense sense in project.Senses)
				{
					writer.Write("\t");
					writer.Write(sense.Gloss);
					senseClusters[sense] = project.GenerateCognateSets(sense).ToList();

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
								writer.Write(',');
							int i = senseClusters[sense].FindIndex(set => set.DataObjects.Contains(word)) + 1;
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
