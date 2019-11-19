using System.Collections.Specialized;
using SIL.ObjectModel;

namespace SIL.Cog.Application.Collections
{
	public class ReadOnlyBindableList<T> : ReadOnlyObservableList<T>
	{
		public override event NotifyCollectionChangedEventHandler CollectionChanged;

		public ReadOnlyBindableList(IObservableList<T> list)
			: base(list)
		{
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler collectionChanged = CollectionChanged;
			if (collectionChanged != null)
				BindableCollectionUtils.InvokeCollectionChanged(collectionChanged, this, e);
		}
	}
}
