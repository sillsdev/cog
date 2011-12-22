using System.Collections.Generic;

namespace SIL.Cog
{
	public abstract class Clusterer<T>
	{
		public abstract IEnumerable<Cluster<T>> GenerateClusters(IEnumerable<T> dataObjects);

	}
}
