namespace SIL.Cog.ViewModels
{
	public class VarietyViewModel : WrapperViewModel
	{
		private readonly Variety _variety;

		public VarietyViewModel(Variety variety)
			: base(variety, variety.Name)
		{
			_variety = variety;
		}

		internal Variety ModelVariety
		{
			get { return _variety; }
		}

		public string Name
		{
			get { return _variety.Name; }
			set
			{
				_variety.Name = value;
				DisplayName = value;
			}
		}
	}
}
