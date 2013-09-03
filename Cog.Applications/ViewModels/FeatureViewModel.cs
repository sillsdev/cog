using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Applications.ViewModels
{
	public class FeatureViewModel : ViewModelBase
	{
		private readonly SymbolicFeature _feature;
		private readonly ReadOnlyCollection<FeatureValueViewModel> _values;
		private FeatureValueViewModel _selectedValue;

		public FeatureViewModel(SymbolicFeature feature)
			: this(feature, null)
		{
		}

		public FeatureViewModel(SymbolicFeature feature, FeatureSymbol symbol)
		{
			_feature = feature;
			_values = new ReadOnlyCollection<FeatureValueViewModel>(feature.PossibleSymbols.Select(s => new FeatureValueViewModel(s)).ToArray());
			if (symbol != null)
				_selectedValue = _values.Single(fv => fv.DomainSymbol == symbol);
			else if (_values.Count > 0)
				_selectedValue = _values[0];
		}

		public string Name
		{
			get { return _feature.Description; }
		}

		internal SymbolicFeature DomainFeature
		{
			get { return _feature; }
		}

		public ReadOnlyCollection<FeatureValueViewModel> Values
		{
			get { return _values; }
		}

		public FeatureValueViewModel SelectedValue
		{
			get { return _selectedValue; }
			set { Set(() => SelectedValue, ref _selectedValue, value); }
		}
	}
}
