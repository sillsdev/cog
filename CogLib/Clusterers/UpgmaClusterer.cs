using System;
using System.Collections.Generic;
using System.Globalization;
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
			var clusters = new List<Cluster<T>>(dataObjects.Select(obj => new Cluster<T>(obj.ToString(), obj.ToEnumerable())));
			var distances = new Dictionary<Cluster<T>, Dictionary<Cluster<T>, double>>();
			var heights = new Dictionary<Cluster<T>, double>();
			for (int i = 0; i < clusters.Count; i++)
			{
				for (int j = i + 1; j < clusters.Count; j++)
				{
					double dist = _getDistance(clusters[i].DataObjects.First(), clusters[j].DataObjects.First());
					distances.GetValue(clusters[i], () => new Dictionary<Cluster<T>, double>())[clusters[j]] = dist;
					distances.GetValue(clusters[j], () => new Dictionary<Cluster<T>, double>())[clusters[i]] = dist;
				}
				heights[clusters[i]] = 0;
			}

			int id = 0;
			while (clusters.Count >= 2)
			{
				int minI = 0, minJ = 0;
				double minDist = double.MaxValue;
				for (int i = 0; i < clusters.Count; i++)
				{
					for (int j = i + 1; j < clusters.Count; j++)
					{
						double dist = distances[clusters[i]][clusters[j]];
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

				var uCluster = new Cluster<T>(id++.ToString(CultureInfo.InvariantCulture));
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
					Dictionary<Cluster<T>, double> kDistances = distances[kCluster];
					double dist = (iWeight * kDistances[iCluster]) + (jWeight * kDistances[jCluster]);
					distances.GetValue(uCluster, () => new Dictionary<Cluster<T>, double>())[kCluster] = dist;
					distances.GetValue(kCluster, () => new Dictionary<Cluster<T>, double>())[uCluster] = dist;
					kDistances.Remove(iCluster);
					kDistances.Remove(jCluster);
				}
				clusters.RemoveAt(minJ);
				clusters.RemoveAt(minI);
				clusters.Add(uCluster);
				distances.Remove(iCluster);
				distances.Remove(jCluster);
			}

			return clusters;
		}
	}
}
