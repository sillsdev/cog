using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public abstract class MasterViewModel : CogViewModel
	{
		private ViewModelBase _currentView;
		private readonly ReadOnlyCollection<ViewModelBase> _views; 

		protected MasterViewModel(string displayName, params ViewModelBase[] views)
			: base(displayName)
		{
			_views = new ReadOnlyCollection<ViewModelBase>(views);
		}

		public ReadOnlyCollection<ViewModelBase> Views
		{
			get { return _views; }
		}

		public ViewModelBase CurrentView
		{
			get { return _currentView; }
			set { Set("CurrentView", ref _currentView, value); }
		}
	}
}
