using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.Cog.Domain;
using SIL.Machine.Clusterers;

namespace SIL.Cog.Application.Export
{
	public class NexusCognateSetsExporter : ICognateSetsExporter
	{
		public void Export(Stream stream, CogProject project)
		{
			using (var writer = new StreamWriter(new NonClosingStreamWrapper(stream)))
			{
				writer.WriteLine("#NEXUS");

				writer.WriteLine("BEGIN Taxa;");
				writer.WriteLine("\tDIMENSIONS NTax={0};", project.Varieties.Count);
				writer.Write("\tTAXLABELS");

				int maxNameLen = 0;
				foreach (Variety variety in project.Varieties)
				{
					string name = variety.Name.RemoveNexusSpecialChars();
					maxNameLen = Math.Max(maxNameLen, name.Length);
					writer.WriteLine();
					writer.Write("\t\t{0}", name);
				}

				writer.WriteLine(";");
				writer.WriteLine("END;");

				writer.WriteLine("BEGIN Characters;");
				writer.WriteLine("\tDIMENSIONS NChar={0};", project.Meanings.Count);
				writer.WriteLine("\tFORMAT Datatype=STANDARD Missing=? Symbols=\"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ\";");
				writer.Write("\tMATRIX");

				var meaningClusters = new Dictionary<Meaning, List<Cluster<Word>>>();
				foreach (Meaning meaning in project.Meanings)
					meaningClusters[meaning] = project.GenerateCognateSets(meaning).ToList();

				foreach (Variety variety in project.Varieties)
				{
					string name = variety.Name.RemoveNexusSpecialChars();
					writer.WriteLine();
					writer.Write("\t\t{0}{1} ", name, new string(' ', maxNameLen - name.Length));
					foreach (Meaning meaning in project.Meanings)
					{
						int setIndex = 0;
						int maxSetSize = -1;
						foreach (Word word in variety.Words[meaning])
						{
							int j = 1;
							foreach (Cluster<Word> set in meaningClusters[meaning])
							{
								if (set.DataObjects.Contains(word))
								{
									if (set.DataObjects.Count > maxSetSize)
									{
										maxSetSize = set.DataObjects.Count;
										setIndex = j;
									}
									break;
								}
								j++;
							}
						}


						if (setIndex > 0)
						{
							if (setIndex >= 10)
								writer.Write((char) ('A' + (setIndex - 10)));
							else
								writer.Write(setIndex);
						}
						else
						{
							writer.Write("?");
						}
					}
				}

				writer.WriteLine(";");
				writer.WriteLine("END;");
			}
		}
	}
}
