using System.Collections.Generic;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog.Export
{
	public abstract class CognateSetsExporterBase : ICognateSetsExporter
	{
		public abstract void Export(string path, CogProject project);

		protected IDictionary<Sense, IList<ISet<Variety>>> GetCognateSets(CogProject project)
		{
			var cognateSets = new Dictionary<Sense, IList<ISet<Variety>>>();
			foreach (Sense sense in project.Senses)
			{
				Dictionary<Variety, HashSet<Variety>> varietyClusters = project.Varieties.Where(v => v.Words[sense].Count > 0).ToDictionary(v => v, v => new HashSet<Variety>(v.ToEnumerable()));

				foreach (VarietyPair varietyPair in project.VarietyPairs)
				{
					WordPair pair;
					if (!varietyPair.WordPairs.TryGetValue(sense, out pair) || !pair.AreCognatePredicted)
						continue;

					HashSet<Variety> c1 = varietyClusters[varietyPair.Variety1];
					HashSet<Variety> c2 = varietyClusters[varietyPair.Variety2];

					if (c1 == c2)
						continue;

					int numCognates = 0;
					int numComparisons = 0;
					foreach (Variety v1 in c1)
					{
						foreach (Word w1 in v1.Words[sense])
						{
							foreach (Variety v2 in c2)
							{
								foreach (Word w2 in v2.Words[sense])
								{
									WordPair wp = w1.Variety.VarietyPairs[w2.Variety].WordPairs[sense];
									if (w1 == wp.GetWord(w1.Variety) && w2 == wp.GetWord(w2.Variety))
									{
										if (wp.AreCognatePredicted)
											numCognates++;
										numComparisons++;
										break;
									}
								}
							}
						}
					}

					double sim = (double) numCognates / numComparisons;
					if (sim >= 0.5)
					{
						c1.UnionWith(c2);
						foreach (Variety v in c2)
							varietyClusters[v] = c1;
					}
				}
				cognateSets[sense] = new List<ISet<Variety>>(varietyClusters.Values.Distinct());
			}
			return cognateSets;
		}
	}
}
