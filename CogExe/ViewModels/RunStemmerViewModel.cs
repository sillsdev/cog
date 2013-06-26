namespace SIL.Cog.ViewModels
{
	public enum StemmingMethod
	{
		Automatic,
		Hybrid,
		Manual
	}

	public class RunStemmerViewModel : CogViewModelBase
	{
		private StemmingMethod _method;
		private readonly bool _isHybridAvailable;

		public RunStemmerViewModel(bool isHybridAvailable)
			: base("Run Stemmer")
		{
			_isHybridAvailable = isHybridAvailable;
		}

		public bool IsHybridAvailable
		{
			get { return _isHybridAvailable; }
		}

		public StemmingMethod Method
		{
			get { return _method; }
			set { Set(() => Method, ref _method, value); }
		}
	}
}
