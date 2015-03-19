using SIL.Cog.Domain;

namespace SIL.Cog.Application.ViewModels
{
	public class MultipleWordAlignmentVarietyViewModel : VarietyViewModel
	{
		private readonly int _wordIndex;

		public MultipleWordAlignmentVarietyViewModel(Variety variety, int wordIndex)
			: base(variety)
		{
			_wordIndex = wordIndex;
		}

		public int WordIndex
		{
			get { return _wordIndex; }
		}
	}
}
