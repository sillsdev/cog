using SIL.Cog.Domain;

namespace SIL.Cog.Application.ViewModels
{
	public class SenseViewModel : WrapperViewModel
	{
		private readonly Sense _sense;

		public SenseViewModel(Sense sense)
			: base(sense)
		{
			_sense = sense;
		}

		public string Gloss
		{
			get { return _sense.Gloss; }
		}

		public string Category
		{
			get { return _sense.Category; }
		}

		internal Sense DomainSense
		{
			get { return _sense; }
		}
	}
}
