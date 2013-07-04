using GalaSoft.MvvmLight;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public class FeatureValueViewModel : ViewModelBase
	{
		private readonly FeatureSymbol _symbol;

		public FeatureValueViewModel(FeatureSymbol symbol)
		{
			_symbol = symbol;
		}

		public string Name
		{
			get { return _symbol.Description; }
		}

		internal FeatureSymbol ModelSymbol
		{
			get { return _symbol; }
		}
	}
}
