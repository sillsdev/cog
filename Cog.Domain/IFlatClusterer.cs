using System.Collections.Generic;

namespace SIL.Cog.Domain
{
	public interface IFlatClusterer<T>
	{
		IEnumerable<Cluster<T>> GenerateClusters(IEnumerable<T> dataObjects);
	}
}
