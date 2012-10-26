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

			while (clusters.Count > 2)
			{
				Dictionary<Cluster<T>, double> r = clusters.ToDictionary(c => c, c => clusters.Where(oc => oc != c).Sum(oc => distances[c][oc]));
				int minI = 0, minJ = 0;
				double minDist = 0, minQ = double.MaxValue;
				for (int i = 0; i < clusters.Count; i++)
				{
					for (int j = i + 1; j < clusters.Count; j++)
					{
						double dist = distances[clusters[i]][clusters[j]];
						double q = dist - ((r[clusters[i]] + r[clusters[j]]) / (clusters.Count - 2));
						if (q < minQ)
						{
							minQ = q;
							minDist = dist;
							minI = i;
							minJ = j;
						}
					}
				}

				Cluster<T> cluster1 = clusters[minI];
				Cluster<T> cluster2 = clusters[minJ];

				clusters.RemoveAt(minJ);
				clusters.RemoveAt(minI);

				var newCluster = new Cluster<T>(id++.ToString(CultureInfo.InvariantCulture), cluster1.DataObjects.Concat(cluster2.DataObjects));
				if (cluster1.DataObjects.Count > 1)
					newCluster.Children.Add(cluster1);
				if (cluster2.DataObjects.Count > 1)
					newCluster.Children.Add(cluster2);

				foreach (Cluster<T> c in clusters)
				{
					Dictionary<Cluster<T>, double> cDistances = distances[c];
					double dist = (cDistances[cluster1] + cDistances[cluster2] - minDist) / 2;
					distances.GetValue(newCluster, () => new Dictionary<Cluster<T>, double>())[c] = dist;
					distances.GetValue(c, () => new Dictionary<Cluster<T>, double>())[newCluster] = dist;
					cDistances.Remove(cluster1);
					cDistances.Remove(cluster2);
				}
				clusters.Add(newCluster);
				distances.Remove(cluster1);
				distances.Remove(cluster2);
			}

			//var rootCluster = new Cluster<T>(id.ToString(CultureInfo.InvariantCulture), clusters.SelectMany(c => c.DataObjects));
			//rootCluster.Children.AddRange(clusters.Where(c => c.DataObjects.Count > 1));

			return clusters;
		}
	}
}
