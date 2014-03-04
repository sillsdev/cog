using System;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace SIL.Cog.Application.Collections
{
	internal class BindableCollectionUtils
	{
		public static void InvokeCollectionChanged(NotifyCollectionChangedEventHandler collectionChanged, object sender, NotifyCollectionChangedEventArgs e)
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
						dispatcher.BeginInvoke((Action) (() => nh1.Invoke(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset))),
							DispatcherPriority.DataBind);
						continue;
					}
				}
				if ((e.NewItems != null && e.NewItems.Count > 1) || (e.OldItems != null && e.OldItems.Count > 1))
					nh.Invoke(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
				else
					nh.Invoke(sender, e);
			}
		}
	}
}
