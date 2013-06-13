using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (TModel model in e.NewItems)
						Add(_viewModelFactory(model));
					break;

				case NotifyCollectionChangedAction.Remove:
					foreach (TModel model in e.OldItems)
						Remove(model);
					break;

				case NotifyCollectionChangedAction.Reset:
					Clear();
					break;
			}
		}


	}
}
