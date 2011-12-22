using System;
using System.Collections.Generic;
using System.Linq;

namespace SIL.Cog
{
	public class OpticsDropDownClusterer<T> : OpticsClusterer<T>
	{
		public OpticsDropDownClusterer(Optics<T> optics)
			: base(optics)
		{
		}

		public override IEnumerable<Cluster<T>> GenerateClusters(IList<ClusterOrderEntry<T>> clusterOrder)
		{
			int id = 0;
			var processed = new HashSet<int>();
			Tuple<ClusterOrderEntry<T>, int> prev = null;
			foreach (Tuple<ClusterOrderEntry<T>, int> entry in clusterOrder.Select((oe, index) => Tuple.Create(oe, index))
				.OrderByDescending(oe => oe.Item1.Reachability).ThenBy(oe => oe.Item2))
			{
				if (processed.Contains(entry.Item2))
					continue;

				if (prev != null && Math.Abs(prev.Item2 - entry.Item2) > Optics.MinPoints)
				{
					int startIndex = Math.Min(prev.Item2, entry.Item2);
					int endIndex = Math.Max(prev.Item2, entry.Item2);
					var cluster = new Cluster<T>(id.ToString(), clusterOrder.Skip(startIndex).Take(endIndex - startIndex + 1).Select(oe => oe.DataObject));
					id++;
					PopulateCluster(processed, cluster, clusterOrder, startIndex, endIndex, ref id);
					for (int i = startIndex; i <= endIndex; i++)
						processed.Add(i);
					yield return cluster;
				}
				else
				{
					prev = entry;
				}
			}
		}

		private Cluster<T> CreateCluster(HashSet<int> processed, IList<ClusterOrderEntry<T>> clusterOrder, int startIndex, int endIndex, ref int id)
		{
			var cluster = new Cluster<T>(id.ToString(), clusterOrder.Skip(startIndex).Take(endIndex - startIndex).Select(oe => oe.DataObject));
			id++;
			PopulateCluster(processed, cluster, clusterOrder, startIndex, endIndex, ref id);
			for (int i = startIndex; i < endIndex; i++)
				processed.Add(i);
			return cluster;
		}

		private void PopulateCluster(HashSet<int> processed, Cluster<T> cluster, IList<ClusterOrderEntry<T>> clusterOrder, int startIndex, int endIndex, ref int id)
		{
			Tuple<ClusterOrderEntry<T>, int>[] reachOrder = clusterOrder.Skip(startIndex + 1).Take(endIndex - startIndex - 1).Select((oe, index) => Tuple.Create(oe, startIndex + index + 1))
				.OrderByDescending(oe => oe.Item1.Reachability).ThenBy(oe => oe.Item2).ToArray();
			for (int i = 0; i < reachOrder.Length; i++)
			{
				Tuple<ClusterOrderEntry<T>, int> entry = reachOrder[i];
				if (processed.Contains(entry.Item2))
					continue;

				if (entry.Item2 == startIndex + 1)
				{
					// is this an inflexion point?
					if ((clusterOrder[startIndex].Reachability / entry.Item2) < ((entry.Item2 / clusterOrder[entry.Item2 + 1].Reachability) * 0.75))
					{
						startIndex = entry.Item2;
						int j = i + 1;
						for (; j < reachOrder.Length && Math.Abs(reachOrder[j].Item2 - entry.Item2) < 0.00001; j++)
						{
							if (reachOrder[j].Item2 != startIndex + 1 && IsValidCluster(cluster, startIndex, reachOrder[j].Item2))
							{
								cluster.Children.Add(CreateCluster(processed, clusterOrder, startIndex, reachOrder[j].Item2, ref id));
								startIndex = reachOrder[j].Item2;
							}
						}
						if (IsValidCluster(cluster, startIndex, endIndex))
						{
							cluster.Children.Add(CreateCluster(processed, clusterOrder, startIndex, endIndex, ref id));
							return;
						}
					}
					else
					{
						startIndex = entry.Item2;
					}
				}
				else if (entry.Item2 == endIndex - 1)
				{
					// is this an inflexion point?
					if ((clusterOrder[entry.Item2 - 1].Reachability / entry.Item2) < ((entry.Item2 / clusterOrder[endIndex].Reachability) * 0.75)
						&& IsValidCluster(cluster, startIndex, entry.Item2))
					{
						cluster.Children.Add(CreateCluster(processed, clusterOrder, startIndex, entry.Item2, ref id));
						return;
					}
					endIndex = entry.Item2;
				}
				else
				{
					if (IsValidCluster(cluster, startIndex, entry.Item2))
						cluster.Children.Add(CreateCluster(processed, clusterOrder, startIndex, entry.Item2, ref id));
					startIndex = entry.Item2;
					int j = i + 1;
					for (; j < reachOrder.Length && Math.Abs(reachOrder[j].Item2 - entry.Item2) < 0.00001; j++)
					{
						if (reachOrder[j].Item2 != startIndex + 1 && IsValidCluster(cluster, startIndex, reachOrder[j].Item2))
						{
							cluster.Children.Add(CreateCluster(processed, clusterOrder, startIndex, reachOrder[j].Item2, ref id));
							startIndex = reachOrder[j].Item2;
						}
					}
					if (IsValidCluster(cluster, startIndex, endIndex))
					{
						cluster.Children.Add(CreateCluster(processed, clusterOrder, startIndex, endIndex, ref id));
						return;
					}
				}
			}
		}

		private bool IsValidCluster(Cluster<T> parent, int startIndex, int endIndex)
		{
			int clusterSize = endIndex - startIndex;
			return clusterSize >= Optics.MinPoints && parent.DataObjectCount - clusterSize >= Optics.MinPoints;
		}
	}
}
