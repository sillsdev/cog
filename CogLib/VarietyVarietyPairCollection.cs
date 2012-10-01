using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using SIL.Collections;

namespace SIL.Cog
{
	public class VarietyVarietyPairCollection : IKeyedReadOnlyCollection<Variety, VarietyPair>, INotifyCollectionChanged
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		private readonly SimpleMonitor _reentrancyMonitor = new SimpleMonitor();
		private readonly Variety _variety;
		private readonly Dictionary<Variety, VarietyPair> _varietyPairs; 
 
		internal VarietyVarietyPairCollection(Variety variety)
		{
			_variety = variety;
			_varietyPairs = new Dictionary<Variety, VarietyPair>();
		}

		internal void VarietyPairAdded(VarietyPair varietyPair)
		{
			CheckReentrancy();
			Variety otherVariety = GetOtherVariety(varietyPair);
			_varietyPairs.Add(otherVariety, varietyPair);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, varietyPair));
		}

		internal void VarietyPairRemoved(VarietyPair varietyPair)
		{
			CheckReentrancy();
			Variety otherVariety = GetOtherVariety(varietyPair);
			_varietyPairs.Remove(otherVariety);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, varietyPair));
		}

		internal void VarietyPairsCleared()
		{
			CheckReentrancy();
			int count = _varietyPairs.Count;
			_varietyPairs.Clear();
			if (count > 0)
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		IEnumerator<VarietyPair> IEnumerable<VarietyPair>.GetEnumerator()
		{
			return _varietyPairs.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _varietyPairs.Values.GetEnumerator();
		}

		public int Count
		{
			get { return _varietyPairs.Count; }
		}

		public bool TryGetValue(Variety key, out VarietyPair item)
		{
			return _varietyPairs.TryGetValue(key, out item);
		}

		public VarietyPair this[Variety key]
		{
			get { return _varietyPairs[key]; }
		}

		public bool Contains(Variety key)
		{
			return _varietyPairs.ContainsKey(key);
		}

		private Variety GetOtherVariety(VarietyPair pair)
		{
			Variety otherVariety = null;
			if (pair.Variety1 == _variety)
				otherVariety = pair.Variety2;
			else if (pair.Variety2 == _variety)
				otherVariety = pair.Variety1;
			return otherVariety;
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (CollectionChanged != null)
			{
				using (_reentrancyMonitor.Enter())
					CollectionChanged(this, e);
			}
		}

		protected void CheckReentrancy()
		{
			if (_reentrancyMonitor.Busy)
				throw new InvalidOperationException("This collection cannot be changed during a CollectionChanged event.");
		}

		protected IDisposable BlockReentrancy()
		{
			return _reentrancyMonitor.Enter();
		}
	}
}
