using GalaSoft.MvvmLight;

namespace SIL.Cog.Application.ViewModels
{
	public abstract class TaskAreaViewModelBase : ViewModelBase
	{
		private readonly string _displayName;

		protected TaskAreaViewModelBase()
		{
		}

		protected TaskAreaViewModelBase(string displayName)
		{
			_displayName = displayName;
		}

		public string DisplayName
		{
			get { return _displayName; }
		}
	}
}
