using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.Cog.Domain;
using SIL.Machine.Clusterers;

namespace SIL.Cog.Applications.Export
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

					var clusterer = new FlatUpgmaClusterer<Word>((w1, w2) =>
						{
							WordPair wp;
							if (w1.Variety != w2.Variety && w1.Variety.VarietyPairs[w2.Variety].WordPairs.TryGetValue(sense, out wp)
								&& wp.GetWord(w1.Variety) == w1 && wp.GetWord(w2.Variety) == w2)
							{
								return wp.AreCognatePredicted ? 0.0 : 1.0;
							}
							return 1.0;
						}, 0.5);
					senseClusters[sense] = clusterer.GenerateClusters(project.Varieties.SelectMany(v => v.Words[sense])).ToList();

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
