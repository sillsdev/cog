using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public abstract class CogViewModel : ViewModelBase
	{
		private readonly string _displayName;

		protected CogViewModel(string displayName)
		{
			_displayName = displayName;
		}

		public string DisplayName
		{
			get { return _displayName; }
		}

		public abstract void Initialize(CogProject project);
	}
}
