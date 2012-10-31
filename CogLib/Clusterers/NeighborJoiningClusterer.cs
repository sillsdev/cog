using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog.Clusterers
{
	public class NeighborJoiningClusterer<T> : IClusterer<T>
	{
		private readonly Func<T, T, double> _getDistance;

		public NeighborJoiningClusterer(Func<T, T, double> getDistance)
		{
			_getDistance = getDistance;
		}

		public IEnumerable<Cluster<T>> GenerateClusters(IEnumerable<T> dataObjects)
		{
			int id = 0;
			var clusters = new List<Cluster<T>>(dataObjects.Select(obj => new Cluster<T>(id++.ToString(CultureInfo.InvariantCulture), obj.ToEnumerable())));
			var distances = new Dictionary<Cluster<T>, Dictionary<Cluster<T>, double>>();
			for (int i = 0; i < clusters.Count; i++)
			{
				for (int j = i + 1; j < clusters.Count; j++)
				{
					double dist = _getDistance(clusters[i].DataObjects.First(), clusters[j].DataObjects.First());
					distances.GetValue(clusters[i], () => new Dictionary<Cluster<T>, double>())[clusters[j]] = dist;
					distances.GetValue(clusters[j], () => new Dictionary<Cluster<T>, double>())[clusters[i]] = dist;
				}
			}

			while (true)
			{
				Dictionary<Cluster<T>, double> r = clusters.ToDictionary(c => c, c => clusters.Where(oc => oc != c).Sum(oc => distances[c][oc] / (clusters.Count - 2)));
				int minI = 0, minJ = 0;
				double minDist = 0, minQ = double.MaxValue;
				for (int i = 0; i < clusters.Count; i++)
				{
					for (int j = i + 1; j < clusters.Count; j++)
					{
						double dist = distances[clusters[i]][clusters[j]];
						double q = dist - r[clusters[i]] - r[clusters[j]];
						if (q < minQ)
						{
							minQ = q;
							minDist = dist;
							minI = i;
							minJ = j;
						}
					}
				}

				Cluster<T> iCluster = clusters[minI];
				Cluster<T> jCluster = clusters[minJ];

				var uCluster = new Cluster<T>(id++.ToString(CultureInfo.InvariantCulture), iCluster.DataObjects.Concat(jCluster.DataObjects));

				double length1 = (minDist / 2) + ((r[iCluster] - r[jCluster]) / 2);
				uCluster.Children.Add(iCluster, Math.Max(length1, 0));
				uCluster.Children.Add(jCluster, Math.Max(minDist - length1, 0));

				foreach (Cluster<T> kCluster in clusters.Where(c => c != iCluster && c != jCluster))
				{
					Dictionary<Cluster<T>, double> kDistances = distances[kCluster];
					double dist = (kDistances[iCluster] + kDistances[jCluster] - minDist) / 2;
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

				if (clusters.Count <= 2)
				{
					var rootCluster = new Cluster<T>(id.ToString(CultureInfo.InvariantCulture), clusters.SelectMany(c => c.DataObjects));
					foreach (Cluster<T> cluster in clusters)
						rootCluster.Children.Add(cluster, minDist);
					clusters.Clear();
					clusters.Add(rootCluster);
					break;
				}
			}

			return clusters;
		}
	}
}
