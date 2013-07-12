using System;
using GalaSoft.MvvmLight;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public abstract class InitializableViewModelBase : ViewModelBase
	{
		private string _displayName;

		protected InitializableViewModelBase(string displayName)
		{
			_displayName = displayName;
		}

		public string DisplayName
		{
			get { return _displayName; }
			set { Set(() => DisplayName, ref _displayName, value); }
		}

		public abstract void Initialize(CogProject project);

		public abstract bool SwitchView(Type viewType, IReadOnlyList<object> models);
	}
}
