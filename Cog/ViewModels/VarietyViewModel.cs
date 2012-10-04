namespace SIL.Cog.ViewModels
{
	public class VarietyViewModel : WrapperViewModelBase
	{
		private readonly Variety _variety;

		public VarietyViewModel(Variety variety)
		{
			_variety = variety;
		}

		public Variety ModelVariety
		{
			get { return _variety; }
		}

		public string Name
		{
			get { return _variety.Name; }
			set { _variety.Name = value; }
		}
	}
}
