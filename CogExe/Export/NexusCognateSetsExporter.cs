using System;
using System.Collections.Generic;
using System.IO;

namespace SIL.Cog.Export
{
	public class NexusCognateSetsExporter : CognateSetsExporterBase
	{
		public override void Export(string path, CogProject project)
		{
			IDictionary<Sense, IList<ISet<Variety>>> cognateSets = GetCognateSets(project);
			using (var writer = new StreamWriter(path))
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
						IList<ISet<Variety>> clusters = cognateSets[sense];
						bool found = false;
						for (int j = 0; j < clusters.Count; j++)
						{
							if (clusters[j].Contains(variety))
							{
								if (j >= 9)
								{
									writer.Write((char) ('A' + (j - 9)));
								}
								else
								{
									writer.Write(j + 1);
								}
								found = true;
								break;
							}
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
