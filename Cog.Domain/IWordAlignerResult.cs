using System.Collections.Generic;
using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Domain
{
	public interface IWordAlignerResult
	{
		IEnumerable<Alignment<Word, ShapeNode>> GetAlignments();
	}
}
