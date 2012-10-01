using SIL.Collections;

namespace SIL.Cog
{
	public class Sense : NotifyPropertyChangedBase
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
			set
			{
				_gloss = value;
				OnPropertyChanged("Gloss");
			}
		}

		public string Category
		{
			get { return _category; }
			set
			{
				_category = value;
				OnPropertyChanged("Category");
			}
		}

		public override string ToString()
		{
			return Gloss;
		}
	}
}
