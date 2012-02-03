using System.Collections.Generic;
using System.Globalization;

namespace SIL.Cog
{
	public class DbscanOpticsClusterer<T> : OpticsClusterer<T>
	{
		private readonly double _epsilon;

		public DbscanOpticsClusterer(Optics<T> optics, double epsilon)
			: base(optics)
		{
			_epsilon = epsilon;
		}

		public override IEnumerable<Cluster<T>> GenerateClusters(IList<ClusterOrderEntry<T>> clusterOrder)
		{
			var clusters = new List<Cluster<T>>();
			HashSet<T> curCluster = null;
			var noise = new HashSet<T>();
			foreach (ClusterOrderEntry<T> oe in clusterOrder)
			{
				if (oe.Reachability > _epsilon)
				{
					if (oe.CoreDistance <= _epsilon)
					{
						if (curCluster != null)
							clusters.Add(new Cluster<T>(clusters.Count.ToString(CultureInfo.InvariantCulture), curCluster));
						curCluster = new HashSet<T> { oe.DataObject };
					}
					else
					{
						noise.Add(oe.DataObject);
					}
				}
				else if (curCluster != null)
				{
					curCluster.Add(oe.DataObject);
				}
				else
				{
					noise.Add(oe.DataObject);
				}
			}
			if (curCluster != null)
				clusters.Add(new Cluster<T>(clusters.Count.ToString(CultureInfo.InvariantCulture), curCluster));
			if (noise.Count > 0)
				clusters.Add(new Cluster<T>("Noise", noise, true));
			return clusters;
		}
	}
}
