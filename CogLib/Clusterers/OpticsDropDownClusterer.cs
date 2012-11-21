using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SIL.Cog.Clusterers
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
			return GetSubclusters(processed, clusterOrder, 0, clusterOrder.Count, ref id);
		}

		private Cluster<T> CreateCluster(HashSet<int> processed, IList<ClusterOrderEntry<T>> clusterOrder, int startIndex, int endIndex, ref int id)
		{
			string idStr = id.ToString(CultureInfo.InvariantCulture);
			id++;
			var subclusterDataObjects = new HashSet<T>();
			var subclusters = new List<Cluster<T>>();
			foreach (Cluster<T> subcluster in GetSubclusters(processed, clusterOrder, startIndex, endIndex, ref id))
			{
				subclusterDataObjects.UnionWith(subcluster.AllDataObjects);
				subclusters.Add(subcluster);
			}

			for (int i = startIndex; i < endIndex; i++)
				processed.Add(i);

			var cluster = new Cluster<T>(idStr, clusterOrder.Skip(startIndex).Take(endIndex - startIndex).Select(oe => oe.DataObject).Except(subclusterDataObjects));
			cluster.Children.AddRange(subclusters);

			return cluster;
		}

		private IEnumerable<Cluster<T>> GetSubclusters(HashSet<int> processed, IList<ClusterOrderEntry<T>> clusterOrder, int startIndex, int endIndex, ref int id)
		{
			var subclusters = new List<Cluster<T>>();
			int parentCount = endIndex - startIndex;
			Tuple<ClusterOrderEntry<T>, int>[] reachOrder = clusterOrder.Skip(startIndex + 1).Take(endIndex - startIndex - 1).Select((oe, index) => Tuple.Create(oe, startIndex + index + 1))
				.OrderByDescending(oe => oe.Item1.Reachability).ThenBy(oe => oe.Item2).ToArray();
			for (int i = 0; i < reachOrder.Length; i++)
			{
				Tuple<ClusterOrderEntry<T>, int> entry = reachOrder[i];
				if (processed.Contains(entry.Item2))
					continue;

				if (entry.Item2 != clusterOrder.Count - 1 && entry.Item2 == startIndex + 1)
				{
					// is this an inflexion point?
					if ((clusterOrder[startIndex].Reachability / entry.Item2) < ((entry.Item2 / clusterOrder[entry.Item2 + 1].Reachability) * 0.75))
					{
						startIndex = entry.Item2;
						int j = i + 1;
						for (; j < reachOrder.Length && Math.Abs(reachOrder[j].Item2 - entry.Item2) < 0.00001; j++)
						{
							if (reachOrder[j].Item2 != startIndex + 1 && IsValidCluster(parentCount, startIndex, reachOrder[j].Item2))
							{
								subclusters.Add(CreateCluster(processed, clusterOrder, startIndex, reachOrder[j].Item2, ref id));
								startIndex = reachOrder[j].Item2;
							}
						}
						if (IsValidCluster(parentCount, startIndex, endIndex))
						{
							subclusters.Add(CreateCluster(processed, clusterOrder, startIndex, endIndex, ref id));
							break;
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
					if (endIndex != clusterOrder.Count
						&& (clusterOrder[entry.Item2 - 1].Reachability / entry.Item2) < ((entry.Item2 / clusterOrder[endIndex].Reachability) * 0.75)
						&& IsValidCluster(parentCount, startIndex, entry.Item2))
					{
						subclusters.Add(CreateCluster(processed, clusterOrder, startIndex, entry.Item2, ref id));
						break;
					}
					endIndex = entry.Item2;
				}
				else
				{
					if (IsValidCluster(parentCount, startIndex, entry.Item2))
						subclusters.Add(CreateCluster(processed, clusterOrder, startIndex, entry.Item2, ref id));
					startIndex = entry.Item2;
					int j = i + 1;
					for (; j < reachOrder.Length && Math.Abs(reachOrder[j].Item2 - entry.Item2) < 0.00001; j++)
					{
						if (reachOrder[j].Item2 != startIndex + 1 && IsValidCluster(parentCount, startIndex, reachOrder[j].Item2))
						{
							subclusters.Add(CreateCluster(processed, clusterOrder, startIndex, reachOrder[j].Item2, ref id));
							startIndex = reachOrder[j].Item2;
						}
					}
					if (IsValidCluster(parentCount, startIndex, endIndex))
					{
						subclusters.Add(CreateCluster(processed, clusterOrder, startIndex, endIndex, ref id));
						break;
					}
				}
			}

			return subclusters;
		}

		private bool IsValidCluster(int parentCount, int startIndex, int endIndex)
		{
			int clusterSize = endIndex - startIndex;
			return clusterSize >= Optics.MinPoints && parentCount - clusterSize >= Optics.MinPoints;
		}
	}
}
