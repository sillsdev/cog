using System.IO;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Clusterers;
using SIL.Machine.Clusterers;

namespace SIL.Cog.Applications.Export
{
	public class TextCognateSetsExporter : ICognateSetsExporter
	{
		public void Export(Stream stream, CogProject project)
		{
			using (var writer = new StreamWriter(new NonClosingStreamWrapper(stream)))
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
						var clusterer = new CognateSetsClusterer(sense, 0.5);
						int i = 1;
						foreach (Cluster<Variety> set in clusterer.GenerateClusters(project.Varieties))
						{
							if (set.DataObjects.Contains(variety))
							{
								writer.Write(i);
								break;
							}
							i++;
						}
					}
					writer.WriteLine();
				}
			}
		}
	}
}
