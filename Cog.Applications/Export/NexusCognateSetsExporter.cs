using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.Cog.Domain;
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

				var senseClusters = new Dictionary<Sense, List<Cluster<Word>>>();
				foreach (Sense sense in project.Senses)
					senseClusters[sense] = project.GenerateCognateSets(sense).ToList();

				for (int i = 0; i < project.Varieties.Count; i++)
				{
					Variety variety = project.Varieties[i];
					string name = RemoveSpecialChars(variety.Name);
					writer.Write("\t\t{0}{1} ", name, new string(' ', maxNameLen - name.Length));
					foreach (Sense sense in project.Senses)
					{
						int setIndex = 0;
						int maxSetSize = -1;
						foreach (Word word in variety.Words[sense])
						{
							int j = 1;
							foreach (Cluster<Word> set in senseClusters[sense])
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
