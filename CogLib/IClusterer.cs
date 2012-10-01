using System.Collections.Generic;

namespace SIL.Cog
{
	public interface IClusterer<T>
	{
		IEnumerable<Cluster<T>> GenerateClusters(IEnumerable<T> dataObjects);
	}
}
