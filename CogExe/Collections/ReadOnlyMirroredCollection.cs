using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog.Collections
{
	public class ReadOnlyMirroredCollection<TSource, TTarget> : ReadOnlyObservableList<TTarget>, IReadOnlyKeyedCollection<TSource, TTarget>, IKeyedCollection<TSource, TTarget>
	{
		private readonly Func<TSource, TTarget> _sourceToTarget;
		private readonly KeyedBulkObservableList<TSource, TTarget> _items;

		public ReadOnlyMirroredCollection(IReadOnlyObservableCollection<TSource> source, Func<TSource, TTarget> sourceToTarget, Func<TTarget, TSource> targetToSource)
			: this((IEnumerable<TSource>) source, sourceToTarget, targetToSource)
		{
			source.CollectionChanged += OnSourceCollectionChanged;
		}

		public ReadOnlyMirroredCollection(IObservableCollection<TSource> source, Func<TSource, TTarget> sourceToTarget, Func<TTarget, TSource> targetToSource)
			: this((IEnumerable<TSource>) source, sourceToTarget, targetToSource)
		{
			source.CollectionChanged += OnSourceCollectionChanged;
		}

		protected ReadOnlyMirroredCollection(IEnumerable<TSource> source, Func<TSource, TTarget> sourceToTarget, Func<TTarget, TSource> targetToSource)
			: base(new KeyedBulkObservableList<TSource, TTarget>(source.Select(sourceToTarget), targetToSource))
		{
			_sourceToTarget = sourceToTarget;
			_items = (KeyedBulkObservableList<TSource, TTarget>) Items;
		}

		private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					MirrorAdd(e.NewItems.Cast<TSource>(), e.NewItems.Count);
					break;

				case NotifyCollectionChangedAction.Remove:
					MirrorRemove(e.OldItems.Cast<TSource>(), e.OldItems.Count);
					break;

				case NotifyCollectionChangedAction.Reset:
					MirrorReset((IEnumerable<TSource>) sender);
					break;
			}
		}

		protected virtual void MirrorAdd(IEnumerable<TSource> items, int count)
		{
			if (count == 1)
			{
				_items.Add(_sourceToTarget(items.First()));
			}
			else
			{
				using (_items.BulkUpdate())
					_items.AddRange(items.Select(item => _sourceToTarget(item)));
			}
		}

		protected virtual void MirrorRemove(IEnumerable<TSource> items, int count)
		{
			if (count == 1)
			{
				_items.Remove(items.First());
			}
			else
			{
				using (_items.BulkUpdate())
				{
					foreach (TSource item in items)
						_items.Remove(item);
				}
			}
		}

		protected virtual void MirrorReset(IEnumerable<TSource> source)
		{
			using (_items.BulkUpdate())
				_items.ReplaceAll(source.Select(item => _sourceToTarget(item)));
		}

		public bool TryGetValue(TSource key, out TTarget item)
		{
			return _items.TryGetValue(key, out item);
		}

		public TTarget this[TSource key]
		{
			get { return _items[key]; }
		}

		public bool Contains(TSource key)
		{
			return _items.Contains(key);
		}

		bool IKeyedCollection<TSource, TTarget>.Remove(TSource key)
		{
			throw new NotSupportedException();
		}
	}
}
