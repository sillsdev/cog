using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public class RelevantValueViewModel : ChangeTrackingViewModelBase
	{
		private readonly FeatureSymbol _symbol;
		private int _metric;

		public RelevantValueViewModel(FeatureSymbol symbol, int metric)
		{
			_symbol = symbol;
			_metric = metric;
		}

		public string Description
		{
			get { return _symbol.Description; }
		}

		public int Metric
		{
			get { return _metric; }
			set { SetChanged(() => Metric, ref _metric, value); }
		}

		internal FeatureSymbol ModelSymbol
		{
			get { return _symbol; }
		}
	}
}
