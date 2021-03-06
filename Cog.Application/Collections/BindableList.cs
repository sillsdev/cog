using System.Collections.Generic;
using System.Collections.Specialized;
using SIL.ObjectModel;

namespace SIL.Cog.Application.Collections
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
			if (Updating)
				return;

			NotifyCollectionChangedEventHandler collectionChanged = CollectionChanged;
			if (collectionChanged != null)
			{
				using (BlockReentrancy())
					BindableCollectionUtils.InvokeCollectionChanged(collectionChanged, this, e);
			}
		}
	}
}
