using System.Collections.Generic;
using SIL.Machine;

namespace SIL.Cog
{
	public interface IWordAlignerResult
	{
		IEnumerable<Alignment<Word, ShapeNode>> GetAlignments();
	}
}
