namespace SIL.Cog.Domain.Components
{
	public class VarietySegmenter : IProcessor<Variety>
	{
		private readonly Segmenter _segmenter;

		public VarietySegmenter(Segmenter segmenter)
		{
			_segmenter = segmenter;
		}

		public void Process(Variety data)
		{
			foreach (Word word in data.Words)
				_segmenter.Segment(word);

			foreach (Affix affix in data.Affixes)
				_segmenter.Segment(affix);
		}
	}
}
