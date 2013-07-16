using System.Collections.Generic;
using SIL.Machine;

namespace SIL.Cog.Domain
{
	public interface IWordAlignerResult
	{
		IEnumerable<Alignment<Word, ShapeNode>> GetAlignments();
	}
}
