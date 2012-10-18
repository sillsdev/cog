using System.Collections.Generic;
using System.ComponentModel;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public abstract class CogViewModelBase : ViewModelBase, IChangeTracking
	{
		private string _displayName;
		private bool _isChanged;

		protected CogViewModelBase(string displayName)
		{
			_displayName = displayName;
		}

		protected CogViewModelBase()
		{
		}

		public string DisplayName
		{
			get { return _displayName; }
			set { Set(() => DisplayName, ref _displayName, value); }
		}

		public override string ToString()
		{
			return _displayName;
		}

		public virtual void AcceptChanges()
		{
			IsChanged = false;
		}

		public bool IsChanged
		{
			get { return _isChanged; }
			protected set { Set(() => IsChanged, ref _isChanged, value); }
		}

		protected void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "IsChanged":
					if (((CogViewModelBase) sender).IsChanged)
						IsChanged = true;
					break;
			}
		}

		protected void ChildrenAcceptChanges(IEnumerable<CogViewModelBase> children)
		{
			if (children == null)
				return;

			foreach (CogViewModelBase child in children)
				child.AcceptChanges();
		}
	}
}
