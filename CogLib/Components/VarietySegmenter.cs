namespace SIL.Cog.Components
{
	public class VarietySegmenter : ProcessorBase<Variety>
	{
		public VarietySegmenter(CogProject project)
			: base(project)
		{
		}

		public override void Process(Variety data)
		{
			foreach (Word word in data.Words)
				Project.Segmenter.Segment(word);

			foreach (Affix affix in data.Affixes)
				Project.Segmenter.Segment(affix);
		}
	}
}
