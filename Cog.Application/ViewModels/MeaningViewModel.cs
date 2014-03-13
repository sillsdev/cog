using SIL.Cog.Domain;

namespace SIL.Cog.Application.ViewModels
{
	public class MeaningViewModel : WrapperViewModel
	{
		private readonly Meaning _meaning;

		public MeaningViewModel(Meaning meaning)
			: base(meaning)
		{
			_meaning = meaning;
		}

		public string Gloss
		{
			get { return _meaning.Gloss; }
		}

		public string Category
		{
			get { return _meaning.Category; }
		}

		internal Meaning DomainMeaning
		{
			get { return _meaning; }
		}
	}
}
