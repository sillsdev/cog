using SIL.Collections;

namespace SIL.Cog.Domain
{
	public class Sense : ObservableObject
	{
		private string _gloss;
		private string _category;

		public Sense(string gloss, string category)
		{
			Gloss = gloss;
			Category = category;
		}

		public string Gloss
		{
			get { return _gloss; }
			set { Set(() => Gloss, ref _gloss, value); }
		}

		public string Category
		{
			get { return _category; }
			set { Set(() => Category, ref _category, value); }
		}

		public override string ToString()
		{
			return Gloss;
		}
	}
}
