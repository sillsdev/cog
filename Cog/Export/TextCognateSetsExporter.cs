using System.Collections.Generic;
using System.IO;

namespace SIL.Cog.Export
{
	public class TextCognateSetsExporter : CognateSetsExporterBase
	{
		public override void Export(string path, CogProject project)
		{
			IDictionary<Sense, IList<ISet<Variety>>> cognateSets = GetCognateSets(project);
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
						IList<ISet<Variety>> clusters = cognateSets[sense];
						for (int i = 0; i < clusters.Count; i++)
						{
							if (clusters[i].Contains(variety))
							{
								writer.Write(i + 1);
								break;
							}
						}
					}
					writer.WriteLine();
				}
			}
		}
	}
}
