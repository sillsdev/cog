using System.Collections.Generic;

namespace SIL.Cog
{
	public interface IFlatClusterer<T>
	{
		IEnumerable<Cluster<T>> GenerateClusters(IEnumerable<T> dataObjects);
	}
}
