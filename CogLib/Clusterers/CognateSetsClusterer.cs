using System.Collections.Generic;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog.Clusterers
{
	public class CognateSetsClusterer : IFlatClusterer<Variety>
	{
		private readonly Sense _sense;
		private readonly double _threshold;

		public CognateSetsClusterer(Sense sense, double threshold)
		{
			_sense = sense;
			_threshold = threshold;
		}

		public IEnumerable<Cluster<Variety>> GenerateClusters(IEnumerable<Variety> dataObjects)
		{
			Variety[] varieties = dataObjects.ToArray();
			Dictionary<Variety, Cluster<Variety>> varietyClusters = varieties.Where(v => v.Words[_sense].Count > 0).ToDictionary(v => v, v => new Cluster<Variety>(v.ToEnumerable()));

			IEnumerable<VarietyPair> varietyPairs = from i in Enumerable.Range(0, varieties.Length)
			                                        from j in Enumerable.Range(i + 1, varieties.Length - (i + 1))
			                                        let vp = varieties[i].VarietyPairs[varieties[j]]
			                                        where vp.WordPairs.Contains(_sense) && vp.WordPairs[_sense].AreCognatePredicted
			                                        orderby vp.PhoneticSimilarityScore descending 
			                                        select vp;

			foreach (VarietyPair varietyPair in varietyPairs)
			{
				Cluster<Variety> c1 = varietyClusters[varietyPair.Variety1];
				Cluster<Variety> c2 = varietyClusters[varietyPair.Variety2];

				if (c1 == c2)
					continue;

				int numCognates = 0;
				int numComparisons = 0;
				foreach (Variety v1 in c1.DataObjects)
				{
					foreach (Word w1 in v1.Words[_sense])
					{
						foreach (Variety v2 in c2.DataObjects)
						{
							foreach (Word w2 in v2.Words[_sense])
							{
								WordPair wp = w1.Variety.VarietyPairs[w2.Variety].WordPairs[_sense];
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
				if (sim >= _threshold)
				{
					var newCluster = new Cluster<Variety>(c1.DataObjects.Union(c2.DataObjects));
					foreach (Variety v in c1.DataObjects.Concat(c2.DataObjects))
						varietyClusters[v] = newCluster;
				}
			}
			return varietyClusters.Values.Distinct();
		}
	}
}
