using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using GalaSoft.MvvmLight;

namespace SIL.Cog.Applications.ViewModels
{
	public abstract class ChangeTrackingViewModelBase : ViewModelBase, IChangeTracking
	{
		private bool _isChanged;

		public virtual void AcceptChanges()
		{
			IsChanged = false;
		}

		public bool IsChanged
		{
			get { return _isChanged; }
			protected set { Set(() => IsChanged, ref _isChanged, value); }
		}

		protected bool SetChanged<T>(Expression<Func<T>> propertyExpression, ref T field, T newValue)
		{
			if (Set(propertyExpression, ref field, newValue))
			{
				IsChanged = true;
				return true;
			}
			return false;
		}

		protected void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "IsChanged":
					if (((IChangeTracking) sender).IsChanged)
						IsChanged = true;
					break;
			}
		}

		protected void ChildrenAcceptChanges(IEnumerable<IChangeTracking> children)
		{
			if (children == null)
				return;

			foreach (IChangeTracking child in children)
				child.AcceptChanges();
		}
	}
}
