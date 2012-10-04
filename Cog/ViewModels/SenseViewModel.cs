namespace SIL.Cog.ViewModels
{
	public class SenseViewModel : WrapperViewModelBase
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
			set { _sense.Gloss = value; }
		}

		public string Category
		{
			get { return _sense.Category; }
			set { _sense.Category = value; }
		}

		public Sense ModelSense
		{
			get { return _sense; }
		}
	}
}
