using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using SIL.Collections;

namespace SIL.Cog.Domain
{
	public sealed class VarietyVarietyPairCollection : IReadOnlyObservableCollection<VarietyPair>, IReadOnlyKeyedCollection<Variety, VarietyPair>, IObservableCollection<VarietyPair>,
		IKeyedCollection<Variety, VarietyPair>
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { PropertyChanged += value; }
			remove { PropertyChanged -= value; }
		}

		private event PropertyChangedEventHandler PropertyChanged;

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
			Variety otherVariety = varietyPair.GetOtherVariety(_variety);
			_varietyPairs.Add(otherVariety, varietyPair);
			OnPropertyChanged(new PropertyChangedEventArgs("Count"));
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, varietyPair));
		}

		internal void VarietyPairRemoved(VarietyPair varietyPair)
		{
			CheckReentrancy();
			Variety otherVariety = varietyPair.GetOtherVariety(_variety);
			_varietyPairs.Remove(otherVariety);
			OnPropertyChanged(new PropertyChangedEventArgs("Count"));
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, varietyPair));
		}

		internal void VarietyPairsCleared()
		{
			CheckReentrancy();
			int count = _varietyPairs.Count;
			_varietyPairs.Clear();
			if (count > 0)
			{
				OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		IEnumerator<VarietyPair> IEnumerable<VarietyPair>.GetEnumerator()
		{
			return _varietyPairs.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _varietyPairs.Values.GetEnumerator();
		}

		void ICollection<VarietyPair>.Add(VarietyPair item)
		{
			throw new NotSupportedException();
		}

		void ICollection<VarietyPair>.Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(VarietyPair item)
		{
			return _varietyPairs.ContainsValue(item);
		}

		public void CopyTo(VarietyPair[] array, int arrayIndex)
		{
			_varietyPairs.Values.CopyTo(array, arrayIndex);
		}

		bool ICollection<VarietyPair>.Remove(VarietyPair item)
		{
			throw new NotSupportedException();
		}

		public int Count
		{
			get { return _varietyPairs.Count; }
		}

		bool ICollection<VarietyPair>.IsReadOnly
		{
			get { return true; }
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

		bool IKeyedCollection<Variety, VarietyPair>.Remove(Variety key)
		{
			throw new NotSupportedException();
		}

		private void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				using (_reentrancyMonitor.Enter())
					handler(this, e);
			}
		}

		private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler handler = CollectionChanged;
			if (handler != null)
			{
				using (_reentrancyMonitor.Enter())
					handler(this, e);
			}
		}

		private void CheckReentrancy()
		{
			if (_reentrancyMonitor.Busy)
				throw new InvalidOperationException("This collection cannot be changed during a CollectionChanged event.");
		}
	}
}
