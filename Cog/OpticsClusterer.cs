using System.Collections.Generic;

namespace SIL.Cog
{
	public abstract class OpticsClusterer<T> : Clusterer<T>
	{
		private readonly Optics<T> _optics; 

		protected OpticsClusterer(Optics<T> optics)
		{
			_optics = optics;
		}

		public Optics<T> Optics
		{
			get { return _optics; }
		}

		public override IEnumerable<Cluster<T>> GenerateClusters(IEnumerable<T> dataObjects)
		{
			return GenerateClusters(_optics.ClusterOrder(dataObjects));
		}

		public abstract IEnumerable<Cluster<T>> GenerateClusters(IList<ClusterOrderEntry<T>> clusterOrder);
	}
}
