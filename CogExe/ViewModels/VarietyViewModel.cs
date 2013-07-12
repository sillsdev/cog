namespace SIL.Cog.ViewModels
{
	public class VarietyViewModel : WrapperViewModel
	{
		private readonly Variety _variety;

		public VarietyViewModel(Variety variety)
			: base(variety)
		{
			_variety = variety;
		}

		public string Name
		{
			get { return _variety.Name; }
			set { _variety.Name = value; }
		}

		internal Variety ModelVariety
		{
			get { return _variety; }
		}
	}
}
