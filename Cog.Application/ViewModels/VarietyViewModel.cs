using SIL.Cog.Domain;

namespace SIL.Cog.Application.ViewModels
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
		}

		internal Variety DomainVariety
		{
			get { return _variety; }
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
