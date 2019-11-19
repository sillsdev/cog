using System.Collections.Generic;
using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;
using SIL.ObjectModel;

namespace SIL.Cog.Domain
{
	public interface IWordAlignerResult
	{
		IWordAligner WordAligner { get; }

		ReadOnlyList<Word> Words { get; }

		IEnumerable<Alignment<Word, ShapeNode>> GetAlignments();

		int BestRawScore { get; }
	}
}
