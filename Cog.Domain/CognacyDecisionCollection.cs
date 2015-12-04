using System.Collections.Generic;
using SIL.Collections;

namespace SIL.Cog.Domain
{
	public class CognacyDecisionCollection : ObservableList<CognacyDecision>
	{
		private readonly Dictionary<UnorderedTuple<Variety, Variety>, Dictionary<Meaning, int>> _lookupDictionary;

		public CognacyDecisionCollection()
		{
			_lookupDictionary = new Dictionary<UnorderedTuple<Variety, Variety>, Dictionary<Meaning, int>>();
		}

		public CognacyDecision Add(WordPair wordPair, bool cognacy)
		{
			var decision = new CognacyDecision(wordPair.VarietyPair.Variety1, wordPair.VarietyPair.Variety2, wordPair.Meaning, cognacy);
			Add(decision);
			return decision;
		}

		public bool Remove(WordPair wordPair)
		{
			Dictionary<Meaning, int> decisions;
			UnorderedTuple<Variety, Variety> key = UnorderedTuple.Create(wordPair.VarietyPair.Variety1, wordPair.VarietyPair.Variety2);
			if (_lookupDictionary.TryGetValue(key, out decisions))
			{
				int index;
				if (decisions.TryGetValue(wordPair.Meaning, out index))
				{
					decisions.Remove(wordPair.Meaning);
					if (decisions.Count == 0)
						_lookupDictionary.Remove(key);
					base.RemoveItem(index);
					return true;
				}
			}
			return false;
		}

		public void UpdateActualCognacy(WordPair wordPair)
		{
			Dictionary<Meaning, int> decisions;
			if (_lookupDictionary.TryGetValue(UnorderedTuple.Create(wordPair.VarietyPair.Variety1, wordPair.VarietyPair.Variety2), out decisions))
			{
				int index;
				if (decisions.TryGetValue(wordPair.Meaning, out index))
				{
					wordPair.ActualCognacy = Items[index].Cognacy;
					return;
				}
			}

			wordPair.ActualCognacy = null;
		}

		protected override void InsertItem(int index, CognacyDecision item)
		{
			AddToLookupDictionary(index, item);
			base.InsertItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			RemoveFromLookupDictionary(index);
			base.RemoveItem(index);
		}

		protected override void MoveItem(int oldIndex, int newIndex)
		{
			CognacyDecision item = Items[oldIndex];
			UnorderedTuple<Variety, Variety> key = UnorderedTuple.Create(item.Variety1, item.Variety2);
			_lookupDictionary[key][item.Meaning] = newIndex;
			base.MoveItem(oldIndex, newIndex);
		}

		protected override void SetItem(int index, CognacyDecision item)
		{
			RemoveFromLookupDictionary(index);
			AddToLookupDictionary(index, item);
			base.SetItem(index, item);
		}

		protected override void ClearItems()
		{
			_lookupDictionary.Clear();
			base.ClearItems();
		}

		private void AddToLookupDictionary(int index, CognacyDecision item)
		{
			_lookupDictionary.GetValue(UnorderedTuple.Create(item.Variety1, item.Variety2), () => new Dictionary<Meaning, int>())[item.Meaning] = index;
		}

		private void RemoveFromLookupDictionary(int index)
		{
			CognacyDecision item = Items[index];
			UnorderedTuple<Variety, Variety> key = UnorderedTuple.Create(item.Variety1, item.Variety2);
			Dictionary<Meaning, int> decisions = _lookupDictionary[key];
			if (decisions.Remove(item.Meaning) && decisions.Count == 0)
				_lookupDictionary.Remove(key);
		}
	}
}
