using System;
using System.Collections.Generic;
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
			var clusters = new List<Cluster<T>>(dataObjects.Select(obj => new Cluster<T>(obj.ToEnumerable()) {Description = obj.ToString()}));
			var distances = new Dictionary<UnorderedTuple<Cluster<T>, Cluster<T>>, double>();
			for (int i = 0; i < clusters.Count; i++)
			{
				for (int j = i + 1; j < clusters.Count; j++)
					distances[UnorderedTuple.Create(clusters[i], clusters[j])] = _getDistance(clusters[i].DataObjects.First(), clusters[j].DataObjects.First());
			}

			while (clusters.Count > 2)
			{
				Dictionary<Cluster<T>, double> r = clusters.ToDictionary(c => c, c => clusters.Where(oc => oc != c).Sum(oc => distances[UnorderedTuple.Create(c, oc)] / (clusters.Count - 2)));
				int minI = 0, minJ = 0;
				double minDist = 0, minQ = double.MaxValue;
				for (int i = 0; i < clusters.Count; i++)
				{
					for (int j = i + 1; j < clusters.Count; j++)
					{
						double dist = distances[UnorderedTuple.Create(clusters[i], clusters[j])];
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
				distances.Remove(UnorderedTuple.Create(iCluster, jCluster));

				var uCluster = new Cluster<T>();

				double iLen = (minDist / 2) + ((r[iCluster] - r[jCluster]) / 2);
				if (iLen <= 0 && !iCluster.IsLeaf)
				{
					foreach (var iChild in iCluster.Children.Select(c => new {Node = c, Len = iCluster.Children.GetLength(c)}).ToArray())
						uCluster.Children.Add(iChild.Node, iChild.Len);
				}
				else
				{
					uCluster.Children.Add(iCluster, Math.Max(iLen, 0));
				}
				double jLen = minDist - iLen;
				if (jLen <= 0 && !jCluster.IsLeaf)
				{
					foreach (var jChild in jCluster.Children.Select(c => new {Node = c, Len = jCluster.Children.GetLength(c)}).ToArray())
						uCluster.Children.Add(jChild.Node, jChild.Len);
				}
				else
				{
					uCluster.Children.Add(jCluster, Math.Max(jLen, 0));
				}

				foreach (Cluster<T> kCluster in clusters.Where(c => c != iCluster && c != jCluster))
				{
					UnorderedTuple<Cluster<T>, Cluster<T>> kiKey = UnorderedTuple.Create(kCluster, iCluster);
					UnorderedTuple<Cluster<T>, Cluster<T>> kjKey = UnorderedTuple.Create(kCluster, jCluster);
					distances[UnorderedTuple.Create(kCluster, uCluster)] = (distances[kiKey] + distances[kjKey] - minDist) / 2;
					distances.Remove(kiKey);
					distances.Remove(kjKey);
				}
				clusters.RemoveAt(minJ);
				clusters.RemoveAt(minI);
				clusters.Add(uCluster);
			}

			if (clusters.Count == 2)
			{
				clusters[1].Children.Add(clusters[0], distances[UnorderedTuple.Create(clusters[0], clusters[1])]);
				clusters.RemoveAt(0);
			}

			return clusters;
		}
	}
}
