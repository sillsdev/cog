namespace SIL.Cog
{
	public class Sense
	{
		private readonly string _gloss;
		private readonly string _category;

		public Sense(string gloss, string category)
		{
			_gloss = gloss;
			_category = category;
		}

		public string Gloss
		{
			get { return _gloss; }
		}

		public string Category
		{
			get { return _category; }
		}

		public override string ToString()
		{
			return _gloss;
		}
	}
}
