using SIL.Collections;

namespace SIL.Cog.Domain
{
	public interface IProbabilityDistribution<TSample>
	{
		IReadOnlyCollection<TSample> Samples { get; }
		double this[TSample sample] { get; }
	}
}
