using SIL.Collections;

namespace SIL.Cog
{
	public interface IProbabilityDistribution<TSample>
	{
		IReadOnlyCollection<TSample> Samples { get; }
		double this[TSample sample] { get; }
	}
}
