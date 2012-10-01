using System.Collections.Generic;

namespace SIL.Cog
{
	public interface IAlignerResult
	{
		int BestScore { get; }

		IEnumerable<Alignment> GetAlignments();
		IEnumerable<Alignment> GetAlignments(double scoreMargin);
	}
}
