using System.Linq;

namespace SIL.Cog.Domain.Components
{
	public class AffixStripper : ProcessorBase<Variety>
	{
		public AffixStripper(CogProject project)
			: base(project)
		{
		}

		public override void Process(Variety data)
		{
			foreach (Word word in data.Words.Where(w => w.Shape.Count > 0 && (w.Prefix != null || w.Suffix != null)))
			{
				word.StemIndex = 0;
				word.StemLength = word.StrRep.Length;
				Project.Segmenter.Segment(word);
			}
		}
	}
}
