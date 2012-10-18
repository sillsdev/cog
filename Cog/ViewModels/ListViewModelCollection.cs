using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight.Threading;

namespace SIL.Cog.ViewModels
{
	public class ListViewModelCollection<TCollection, TViewModel, TModel> : ObservableCollection<TViewModel> where TCollection : IList<TModel>, INotifyCollectionChanged
	{
		private readonly Func<TModel, TViewModel> _viewModelFactory;

		public ListViewModelCollection(TCollection source, Func<TModel, TViewModel> viewModelFactory)
			: base(source.Select(viewModelFactory))
		{
			_viewModelFactory = viewModelFactory;
			source.CollectionChanged += OnSourceCollectionChanged;
		}

		private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			DispatcherHelper.CheckBeginInvokeOnUI(() =>
				{
					switch (e.Action)
					{
						case NotifyCollectionChangedAction.Add:
							for (int i = 0; i < e.NewItems.Count; i++)
								Insert(e.NewStartingIndex + i, _viewModelFactory((TModel)e.NewItems[i]));
							break;

						case NotifyCollectionChangedAction.Move:
							if (e.OldItems.Count == 1)
							{
								Move(e.OldStartingIndex, e.NewStartingIndex);
							}
							else
							{
								List<TViewModel> items = this.Skip(e.OldStartingIndex).Take(e.OldItems.Count).ToList();
								for (int i = 0; i < e.OldItems.Count; i++)
									RemoveAt(e.OldStartingIndex);

								for (int i = 0; i < items.Count; i++)
									Insert(e.NewStartingIndex + i, items[i]);
							}
							break;

						case NotifyCollectionChangedAction.Remove:
							for (int i = 0; i < e.OldItems.Count; i++)
								RemoveAt(e.OldStartingIndex);
							break;

						case NotifyCollectionChangedAction.Replace:
							// remove
							for (int i = 0; i < e.OldItems.Count; i++)
								RemoveAt(e.OldStartingIndex);

							// add
							goto case NotifyCollectionChangedAction.Add;

						case NotifyCollectionChangedAction.Reset:
							Clear();
							if (e.NewItems != null)
							{
								foreach (TModel obj in e.NewItems)
									Add(_viewModelFactory(obj));
							}
							break;
					}
				});
		}
	}
}
