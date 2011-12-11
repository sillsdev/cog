namespace SIL.Cog
{
	public class Correspondence
	{
		private readonly string _phoneme;

		public Correspondence(string phoneme)
		{
			_phoneme = phoneme;
		}

		public string Phoneme
		{
			get { return _phoneme; }
		}

		public double Count { get; set; }

		public double Probability { get; set; }
	}
}
