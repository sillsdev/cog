using GalaSoft.MvvmLight;

namespace SIL.Cog.Application.ViewModels
{
	public enum StemmingMethod
	{
		Automatic,
		Hybrid,
		Manual
	}

	public class RunStemmerViewModel : ViewModelBase
	{
		private StemmingMethod _method;
		private readonly bool _isHybridAvailable;

		public RunStemmerViewModel(bool isHybridAvailable)
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
