using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight.Threading;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class UnorderedViewModelCollection<TCollection, TViewModel, TModel> : KeyedBulkObservableCollection<TModel, TViewModel> where TCollection : IEnumerable<TModel>, INotifyCollectionChanged
	{
		private readonly Func<TModel, TViewModel> _viewModelFactory;

		public UnorderedViewModelCollection(TCollection source, Func<TModel, TViewModel> viewModelFactory, Func<TViewModel, TModel> getModel)
			: base(source.Select(viewModelFactory), getModel)
		{
			_viewModelFactory = viewModelFactory;
			source.CollectionChanged += SourceCollectionChanged;
		}

		private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			DispatcherHelper.CheckBeginInvokeOnUI(() =>
				{
					switch (e.Action)
					{
						case NotifyCollectionChangedAction.Add:
							AddRange(e.NewItems.Cast<TModel>().Select(model => _viewModelFactory(model)));
							break;

						case NotifyCollectionChangedAction.Remove:
							if (e.OldItems.Count == 1)
							{
								Remove((TModel) e.OldItems[0]);
							}
							else
							{
								using (BulkUpdate())
								{
									foreach (TModel model in e.OldItems)
										Remove(model);
								}
							}
							break;

						case NotifyCollectionChangedAction.Reset:
							using (BulkUpdate())
							{
								Clear();
								var source = (TCollection) sender;
								AddRange(source.Select(model => _viewModelFactory(model)));
							}
							break;
					}
				});
		}


	}
}
