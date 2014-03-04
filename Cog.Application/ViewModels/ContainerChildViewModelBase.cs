using GalaSoft.MvvmLight;

namespace SIL.Cog.Application.ViewModels
{
	public abstract class ContainerChildViewModelBase : ViewModelBase
	{
		private string _displayName;
		private bool _isSelected;

		protected ContainerChildViewModelBase(string displayName)
		{
			_displayName = displayName;
		}

		public string DisplayName
		{
			get { return _displayName; }
			set { Set(() => DisplayName, ref _displayName, value); }
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (Set(() => IsSelected, ref _isSelected, value))
					OnIsSelectedChanged();
			}
		}

		protected virtual void OnIsSelectedChanged()
		{
		}
	}
}
