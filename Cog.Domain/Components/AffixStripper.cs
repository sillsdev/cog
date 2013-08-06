using System.Linq;

namespace SIL.Cog.Domain.Components
{
	public class AffixStripper : IProcessor<Variety>
	{
		private readonly Segmenter _segmenter;

		public AffixStripper(Segmenter segmenter)
		{
			_segmenter = segmenter;
		}

		public void Process(Variety data)
		{
			foreach (Word word in data.Words.Where(w => w.Shape.Count > 0 && (w.Prefix != null || w.Suffix != null)))
			{
				word.StemIndex = 0;
				word.StemLength = word.StrRep.Length;
				_segmenter.Segment(word);
			}
		}
	}
}
