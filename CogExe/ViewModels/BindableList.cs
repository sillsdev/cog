using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Threading;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class BindableList<T> : BulkObservableList<T>
	{
		public override event NotifyCollectionChangedEventHandler CollectionChanged;

		public BindableList()
		{
		}

		public BindableList(IEnumerable<T> items)
			: base(items)
		{
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler collectionChanged = CollectionChanged;
			if (collectionChanged != null)
			{
				using (BlockReentrancy())
				{
					foreach (NotifyCollectionChangedEventHandler nh in collectionChanged.GetInvocationList())
					{
						var dispObj = nh.Target as DispatcherObject;
						if (dispObj != null)
						{
							Dispatcher dispatcher = dispObj.Dispatcher;
							if (dispatcher != null && !dispatcher.CheckAccess())
							{
								NotifyCollectionChangedEventHandler nh1 = nh;
								dispatcher.BeginInvoke((Action) (() => nh1.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset))),
									DispatcherPriority.DataBind);
								continue;
							}
						}
						if ((e.NewItems != null && e.NewItems.Count > 1) || (e.OldItems != null && e.OldItems.Count > 1))
							nh.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
						else
							nh.Invoke(this, e);
					}
				}
			}
		}
	}
}
