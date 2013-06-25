using System.Collections.Generic;
using SIL.Machine;

namespace SIL.Cog
{
	public interface IWordPairAlignerResult
	{
		int BestRawScore { get; }

		IEnumerable<Alignment<Word, ShapeNode>> GetAlignments();
		IEnumerable<Alignment<Word, ShapeNode>> GetAlignments(double scoreMargin);
	}
}
