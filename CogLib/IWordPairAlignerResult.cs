using System.Collections.Generic;
using SIL.Machine;

namespace SIL.Cog
{
	public interface IWordPairAlignerResult
	{
		int BestRawScore { get; }

		IEnumerable<Alignment<ShapeNode>> GetAlignments();
		IEnumerable<Alignment<ShapeNode>> GetAlignments(double scoreMargin);
	}
}
