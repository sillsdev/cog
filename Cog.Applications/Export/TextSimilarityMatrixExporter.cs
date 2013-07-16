using System;
using System.IO;
using System.Linq;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Clusterers;
using SIL.Collections;

namespace SIL.Cog.Applications.Export
{
	public class TextSimilarityMatrixExporter : ISimilarityMatrixExporter
	{
		public void Export(string path, CogProject project, SimilarityMetric similarityMetric)
		{
				var optics = new Optics<Variety>(variety => variety.VarietyPairs.Select(pair =>
					{
						double score = 0;
						switch (similarityMetric)
						{
							case SimilarityMetric.Lexical:
								score = pair.LexicalSimilarityScore;
								break;
							case SimilarityMetric.Phonetic:
								score = pair.PhoneticSimilarityScore;
								break;
						}
						return Tuple.Create(pair.GetOtherVariety(variety), 1.0 - score);
					}).Concat(Tuple.Create(variety, 0.0)), 2);
				Variety[] varietyArray = optics.ClusterOrder(project.Varieties).Select(oe => oe.DataObject).ToArray();
				using (var writer = new StreamWriter(path))
				{
					foreach (Variety variety in varietyArray)
					{
						writer.Write("\t");
						writer.Write(variety.Name);
					}
					writer.WriteLine();
					for (int i = 0; i < varietyArray.Length; i++)
					{
						writer.Write(varietyArray[i].Name);
						for (int j = 0; j < varietyArray.Length; j++)
						{
							writer.Write("\t");
							if (i != j)
							{
								VarietyPair varietyPair = varietyArray[i].VarietyPairs[varietyArray[j]];
								double score = similarityMetric == SimilarityMetric.Lexical ? varietyPair.LexicalSimilarityScore : varietyPair.PhoneticSimilarityScore;
								writer.Write("{0:0.00}", score);
							}
						}
						writer.WriteLine();
					}
				}
		}
	}
}
