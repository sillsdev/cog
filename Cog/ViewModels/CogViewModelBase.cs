using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public abstract class CogViewModelBase : ViewModelBase
	{
		private readonly string _displayName;

		protected CogViewModelBase(string displayName)
		{
			_displayName = displayName;
		}

		public string DisplayName
		{
			get { return _displayName; }
		}

		public override string ToString()
		{
			return _displayName;
		}
	}
}
