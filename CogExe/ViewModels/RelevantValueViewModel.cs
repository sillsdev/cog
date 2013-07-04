using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public class RelevantValueViewModel : CogViewModelBase
	{
		private readonly FeatureSymbol _symbol;
		private int _metric;

		public RelevantValueViewModel(FeatureSymbol symbol, int metric)
			: base(symbol.Description)
		{
			_symbol = symbol;
			_metric = metric;
		}

		internal FeatureSymbol ModelSymbol
		{
			get { return _symbol; }
		}

		public int Metric
		{
			get { return _metric; }
			set
			{
				if (Set(() => Metric, ref _metric, value))
					IsChanged = true;
			}
		}
	}
}
