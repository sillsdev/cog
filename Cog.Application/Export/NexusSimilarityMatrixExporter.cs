using System;
using System.ComponentModel;
using System.IO;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Export
{
	public class NexusSimilarityMatrixExporter : ISimilarityMatrixExporter
	{
		public void Export(Stream stream, CogProject project, SimilarityMetric similarityMetric)
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

				writer.WriteLine("BEGIN Distances;");
				writer.WriteLine("\tDIMENSIONS NTax={0};", project.Varieties.Count);
				writer.WriteLine("\tFORMAT Triangle=LOWER Diagonal Labels Missing=?;");
				writer.Write("\tMATRIX");

				for (int i = 0; i < project.Varieties.Count; i++)
				{
					Variety variety1 = project.Varieties[i];
					string name = variety1.Name.RemoveNexusSpecialChars();
					writer.WriteLine();
					writer.Write("\t\t{0}{1} ", name, new string(' ', maxNameLen - name.Length));
					for (int j = 0; j <= i; j++)
					{
						if (i == j)
						{
							writer.Write("0.00");
						}
						else
						{
							Variety variety2 = project.Varieties[j];
							VarietyPair vp = variety1.VarietyPairs[variety2];
							double sim;
							switch (similarityMetric)
							{
								case SimilarityMetric.Lexical:
									sim = vp.LexicalSimilarityScore;
									break;
								case SimilarityMetric.Phonetic:
									sim = vp.PhoneticSimilarityScore;
									break;
								default:
									throw new InvalidEnumArgumentException();
							}

							writer.Write("{0:0.00} ", 1.0 - sim);
						}
					}
				}

				writer.WriteLine(";");
				writer.WriteLine("END;");
			}
		}
	}
}
