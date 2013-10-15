using System;
using System.IO;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Clusterers;
using SIL.Machine.Clusterers;

namespace SIL.Cog.Applications.Export
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
				writer.WriteLine("\tTAXLABELS");
				int maxNameLen = 0;
				for (int i = 0; i < project.Varieties.Count; i++)
				{
					string name = RemoveSpecialChars(project.Varieties[i].Name);
					maxNameLen = Math.Max(maxNameLen, name.Length);
					writer.Write("\t\t{0}", name);
					if (i == project.Varieties.Count - 1)
						writer.Write(";");
					writer.WriteLine();
				}
				writer.WriteLine("END;");

				writer.WriteLine("BEGIN Characters;");
				writer.WriteLine("\tDIMENSIONS NChar={0};", project.Senses.Count);
				writer.WriteLine("\tFORMAT Datatype=STANDARD Missing=? Symbols=\"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ\";");
				writer.WriteLine("\tMATRIX");
				for (int i = 0; i < project.Varieties.Count; i++)
				{
					Variety variety = project.Varieties[i];
					string name = RemoveSpecialChars(variety.Name);
					writer.Write("\t\t{0}{1} ", name, new string(' ', maxNameLen - name.Length));
					foreach (Sense sense in project.Senses)
					{
						var clusterer = new CognateSetsClusterer(sense, 0.5);
						bool found = false;
						int j = 1;
						foreach (Cluster<Variety> set in clusterer.GenerateClusters(project.Varieties))
						{
							if (set.DataObjects.Contains(variety))
							{
								if (j >= 10)
								{
									writer.Write((char) ('A' + (j - 10)));
								}
								else
								{
									writer.Write(j);
								}
								found = true;
								break;
							}
							j++;
						}

						if (!found)
							writer.Write("?");
					}
					if (i == project.Varieties.Count - 1)
						writer.Write(";");
					writer.WriteLine();
				}

				writer.Write("END;");
			}
		}

		private static readonly char[] SpecialChars = new[] {'(', ')', '[', ']', '{', '}', '/', '\\', ',', ';', ':', '=', '*', '\'', '"', '<', '>', '^', '`'}; 
		private static string RemoveSpecialChars(string str)
		{
			return string.Concat(str.Replace(" ", "_").Split(SpecialChars, StringSplitOptions.RemoveEmptyEntries));
		}
	}
}
