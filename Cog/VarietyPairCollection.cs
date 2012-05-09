using System;
using System.Collections;
using System.Collections.Generic;

namespace SIL.Cog
{
	public class VarietyPairCollection : ICollection<VarietyPair>
	{
		private readonly Variety _variety;
		private readonly Dictionary<string, VarietyPair> _pairs;
 
		internal VarietyPairCollection(Variety variety)
		{
			_variety = variety;
			_pairs = new Dictionary<string, VarietyPair>();
		}

		IEnumerator<VarietyPair> IEnumerable<VarietyPair>.GetEnumerator()
		{
			return _pairs.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _pairs.Values.GetEnumerator();
		}

		public void Add(VarietyPair item)
		{
			Variety otherVariety = GetOtherVariety(item);
			if (otherVariety == null)
				throw new ArgumentException("This variety is not in the specified variety pair.", "item");
			_pairs[otherVariety.ID] = item;
		}

		public void Clear()
		{
			_pairs.Clear();
		}

		public bool Contains(VarietyPair item)
		{
			Variety otherVariety = GetOtherVariety(item);
			if (otherVariety == null)
				return false;
			VarietyPair pair;
			return _pairs.TryGetValue(otherVariety.ID, out pair) && pair == item;
		}

		public void CopyTo(VarietyPair[] array, int arrayIndex)
		{
			_pairs.Values.CopyTo(array, arrayIndex);
		}

		public bool Remove(VarietyPair item)
		{
			Variety otherVariety = GetOtherVariety(item);
			if (otherVariety == null)
				return false;
			VarietyPair pair;
			if (_pairs.TryGetValue(otherVariety.ID, out pair) && pair == item)
			{
				_pairs.Remove(otherVariety.ID);
				return true;
			}
			return false;
		}

		public int Count
		{
			get { return _pairs.Count; }
		}

		bool ICollection<VarietyPair>.IsReadOnly
		{
			get { return false; }
		}

		public VarietyPair this[Variety variety]
		{
			get { return _pairs[variety.ID]; }
		}

		public VarietyPair this[string varietyID]
		{
			get { return _pairs[varietyID]; }
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
	}
}
