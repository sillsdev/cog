using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public class FeatureViewModel : ViewModelBase
	{
		private readonly SymbolicFeature _feature;
		private readonly ReadOnlyCollection<FeatureValueViewModel> _values;
		private FeatureValueViewModel _currentValue;

		public FeatureViewModel(SymbolicFeature feature)
			: this(feature, null)
		{
		}

		public FeatureViewModel(SymbolicFeature feature, FeatureSymbol symbol)
		{
			_feature = feature;
			_values = new ReadOnlyCollection<FeatureValueViewModel>(feature.PossibleSymbols.Select(s => new FeatureValueViewModel(s)).ToArray());
			if (symbol != null)
				_currentValue = _values.Single(fv => fv.ModelSymbol == symbol);
			else if (_values.Count > 0)
				_currentValue = _values[0];
		}

		public string Name
		{
			get { return _feature.Description; }
		}

		public SymbolicFeature ModelFeature
		{
			get { return _feature; }
		}

		public ReadOnlyCollection<FeatureValueViewModel> Values
		{
			get { return _values; }
		}

		public FeatureValueViewModel CurrentValue
		{
			get { return _currentValue; }
			set { Set(() => CurrentValue, ref _currentValue, value); }
		}
	}
}
