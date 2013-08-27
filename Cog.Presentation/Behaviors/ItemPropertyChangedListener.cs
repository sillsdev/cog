using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight.Threading;

namespace SIL.Cog.Presentation.Behaviors
{
	internal class ItemPropertyChangedListener : IDisposable
	{
		private static readonly Dictionary<object, ItemPropertyChangedListener> Listeners = new Dictionary<object, ItemPropertyChangedListener>(); 

		public static void Subscribe(object key, INotifyCollectionChanged items, string propertyPath, Action<object> changedCallback)
		{
			Unsubscribe(key);
			Listeners[key] = new ItemPropertyChangedListener(key, items, propertyPath, changedCallback);
		}

		public static void Unsubscribe(object key)
		{
			ItemPropertyChangedListener listener;
			if (Listeners.TryGetValue(key, out listener))
			{
				listener.Dispose();
				Listeners.Remove(key);
			}
		}

		private readonly object _key;
		private readonly INotifyCollectionChanged _itemsCollection;
		private readonly string _propertyPath;
		private readonly string[] _properties; 
		private readonly Action<object> _changedCallback;
		private readonly HashSet<INotifyPropertyChanged> _propChangeObjects; 

		private ItemPropertyChangedListener(object key, INotifyCollectionChanged itemsCollection, string propertyPath, Action<object> changedCallback)
		{
			_key = key;
			_itemsCollection = itemsCollection;
			_changedCallback = changedCallback;
			_propertyPath = propertyPath;

			_properties = _propertyPath.SplitPropertyPath().ToArray();

			_propChangeObjects = new HashSet<INotifyPropertyChanged>();
			AddItems((IEnumerable) _itemsCollection);
			_itemsCollection.CollectionChanged += ItemsChanged;
		}

		private void ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddItems(e.NewItems);
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveItems(e.OldItems);
					break;
				case NotifyCollectionChangedAction.Replace:
					RemoveItems(e.OldItems);
					AddItems(e.NewItems);
					break;
				case NotifyCollectionChangedAction.Reset:
					foreach (INotifyPropertyChanged item in _propChangeObjects)
						item.PropertyChanged -= item_PropertyChanged;
					_propChangeObjects.Clear();
					AddItems((IEnumerable) sender);
					break;
			}
			_changedCallback(_key);
		}

		private void AddItems(IEnumerable newItems)
		{
			foreach (object item in newItems)
			{
				INotifyPropertyChanged propChangeObj = GetPropertyChangeObject(item);
				if (propChangeObj != null)
				{
					propChangeObj.PropertyChanged += item_PropertyChanged;
					_propChangeObjects.Add(propChangeObj);
				}
			}
		}

		private void RemoveItems(IEnumerable oldItems)
		{
			foreach (object item in oldItems)
			{
				INotifyPropertyChanged propChangeObj = GetPropertyChangeObject(item);
				if (propChangeObj != null)
				{
					propChangeObj.PropertyChanged -= item_PropertyChanged;
					_propChangeObjects.Remove(propChangeObj);
				}
			}
		}

		private INotifyPropertyChanged GetPropertyChangeObject(object item)
		{
			object[] objects = item.GetPropertyValues(_propertyPath).ToArray();
			return (objects.Length > 1 ? objects[objects.Length - 2] : item) as INotifyPropertyChanged;
		}

		private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_properties.Length == 0 || (e.PropertyName == "Item[]" && _properties[_properties.Length - 1].StartsWith("Item["))
			    || e.PropertyName == _properties[_properties.Length - 1])
			{
				DispatcherHelper.CheckBeginInvokeOnUI(() => _changedCallback(_key));
			}
		}

		public void Dispose()
		{
			_itemsCollection.CollectionChanged -= ItemsChanged;
			foreach (INotifyPropertyChanged item in _propChangeObjects)
				item.PropertyChanged -= item_PropertyChanged;
			_propChangeObjects.Clear();
		}
	}
}
