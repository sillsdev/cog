using GalaSoft.MvvmLight;

namespace SIL.Cog.Applications.ViewModels
{
	public abstract class ContainerChildViewModelBase : ViewModelBase
	{
		private string _displayName;
		private bool _isCurrent;

		protected ContainerChildViewModelBase(string displayName)
		{
			_displayName = displayName;
		}

		public string DisplayName
		{
			get { return _displayName; }
			set { Set(() => DisplayName, ref _displayName, value); }
		}

		public bool IsCurrent
		{
			get { return _isCurrent; }
			set
			{
				if (Set(() => IsCurrent, ref _isCurrent, value))
					OnIsCurrentChanged();
			}
		}

		protected virtual void OnIsCurrentChanged()
		{
		}
	}
}
