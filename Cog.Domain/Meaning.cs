using SIL.Collections;

namespace SIL.Cog.Domain
{
	public class Meaning : ObservableObject
	{
		private string _gloss;
		private string _category;

		public Meaning(string gloss, string category)
		{
			Gloss = gloss;
			Category = category;
		}

		public string Gloss
		{
			get { return _gloss; }
			set
			{
				if (Collection != null)
					Collection.ChangeMeaningGloss(this, value);
				Set(() => Gloss, ref _gloss, value);
			}
		}

		public string Category
		{
			get { return _category; }
			set { Set(() => Category, ref _category, value); }
		}

		internal MeaningCollection Collection { get; set; }

		public override string ToString()
		{
			return Gloss;
		}
	}
}
