namespace SIL.Cog.Statistics
{
	public interface IProbabilityDistribution<in TSample>
	{
		double GetProbability(TSample sample);
	}
}
