using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog.Clusterers
{
	public class UpgmaClusterer<T> : IClusterer<T>
	{
		private readonly Func<T, T, double> _getDistance;

		public UpgmaClusterer(Func<T, T, double> getDistance)
		{
			_getDistance = getDistance;
		}

		public IEnumerable<Cluster<T>> GenerateClusters(IEnumerable<T> dataObjects)
		{
			var clusters = new List<Cluster<T>>(dataObjects.Select(obj => new Cluster<T>(obj.ToEnumerable()) {Description = obj.ToString()}));
			var distances = new Dictionary<UnorderedTuple<Cluster<T>, Cluster<T>>, double>();
			var heights = new Dictionary<Cluster<T>, double>();
			for (int i = 0; i < clusters.Count; i++)
			{
				for (int j = i + 1; j < clusters.Count; j++)
					distances[UnorderedTuple.Create(clusters[i], clusters[j])] = _getDistance(clusters[i].DataObjects.First(), clusters[j].DataObjects.First());
				heights[clusters[i]] = 0;
			}

			while (clusters.Count >= 2)
			{
				int minI = 0, minJ = 0;
				double minDist = double.MaxValue;
				for (int i = 0; i < clusters.Count; i++)
				{
					for (int j = i + 1; j < clusters.Count; j++)
					{
						double dist = distances[UnorderedTuple.Create(clusters[i], clusters[j])];
						if (dist < minDist)
						{
							minDist = dist;
							minI = i;
							minJ = j;
						}
					}
				}

				Cluster<T> iCluster = clusters[minI];
				Cluster<T> jCluster = clusters[minJ];
				distances.Remove(UnorderedTuple.Create(iCluster, jCluster));

				var uCluster = new Cluster<T>();
				double height = minDist / 2;
				heights[uCluster] = height;
				uCluster.Children.Add(iCluster, height - heights[iCluster]);
				uCluster.Children.Add(jCluster, height - heights[jCluster]);

				int iCount = iCluster.AllDataObjects.Count();
				int jCount = jCluster.AllDataObjects.Count();
				double iWeight = (double) iCount / (iCount + jCount);
				double jWeight = (double) jCount / (iCount + jCount);
				foreach (Cluster<T> kCluster in clusters.Where(c => c != iCluster && c != jCluster))
				{
					UnorderedTuple<Cluster<T>, Cluster<T>> kiKey = UnorderedTuple.Create(kCluster, iCluster);
					UnorderedTuple<Cluster<T>, Cluster<T>> kjKey = UnorderedTuple.Create(kCluster, jCluster);
					distances[UnorderedTuple.Create(uCluster, kCluster)] = (iWeight * distances[kiKey]) + (jWeight * distances[kjKey]);
					distances.Remove(kiKey);
					distances.Remove(kjKey);
				}
				clusters.RemoveAt(minJ);
				clusters.RemoveAt(minI);
				clusters.Add(uCluster);
			}

			return clusters;
		}
	}
}
