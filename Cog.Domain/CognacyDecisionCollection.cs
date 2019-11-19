using System.Collections.Generic;
using SIL.Extensions;
using SIL.ObjectModel;

namespace SIL.Cog.Domain
{
	public class CognacyDecisionCollection : ObservableList<CognacyDecision>
	{
		private readonly Dictionary<UnorderedTuple<Variety, Variety>, Dictionary<Meaning, CognacyDecision>> _lookupDictionary;

		public CognacyDecisionCollection()
		{
			_lookupDictionary = new Dictionary<UnorderedTuple<Variety, Variety>, Dictionary<Meaning, CognacyDecision>>();
		}

		public CognacyDecision Add(WordPair wordPair, bool cognacy)
		{
			var decision = new CognacyDecision(wordPair.VarietyPair.Variety1, wordPair.VarietyPair.Variety2, wordPair.Meaning, cognacy);
			Add(decision);
			return decision;
		}

		public bool Remove(WordPair wordPair)
		{
			Dictionary<Meaning, CognacyDecision> decisions;
			UnorderedTuple<Variety, Variety> key = UnorderedTuple.Create(wordPair.VarietyPair.Variety1, wordPair.VarietyPair.Variety2);
			if (_lookupDictionary.TryGetValue(key, out decisions))
			{
				CognacyDecision decision;
				if (decisions.TryGetValue(wordPair.Meaning, out decision))
					return Remove(decision);
			}
			return false;
		}

		public void UpdateActualCognacy(WordPair wordPair)
		{
			wordPair.ActualCognacy = GetCognacy(wordPair.VarietyPair, wordPair.Meaning);
		}

		public bool? GetCognacy(VarietyPair varietyPair, Meaning meaning)
		{
			Dictionary<Meaning, CognacyDecision> decisions;
			if (_lookupDictionary.TryGetValue(UnorderedTuple.Create(varietyPair.Variety1, varietyPair.Variety2),
				out decisions))
			{
				CognacyDecision decision;
				if (decisions.TryGetValue(meaning, out decision))
					return decision.Cognacy;
			}

			return null;
		}

		protected override void InsertItem(int index, CognacyDecision item)
		{
			AddToLookupDictionary(item);
			base.InsertItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			RemoveFromLookupDictionary(index);
			base.RemoveItem(index);
		}

		protected override void SetItem(int index, CognacyDecision item)
		{
			RemoveFromLookupDictionary(index);
			AddToLookupDictionary(item);
			base.SetItem(index, item);
		}

		protected override void ClearItems()
		{
			_lookupDictionary.Clear();
			base.ClearItems();
		}

		private void AddToLookupDictionary(CognacyDecision item)
		{
			Dictionary<Meaning, CognacyDecision> lookup = _lookupDictionary.GetOrCreate(UnorderedTuple.Create(item.Variety1, item.Variety2),
				() => new Dictionary<Meaning, CognacyDecision>());
			lookup[item.Meaning] = item;
		}

		private void RemoveFromLookupDictionary(int index)
		{
			CognacyDecision item = Items[index];
			UnorderedTuple<Variety, Variety> key = UnorderedTuple.Create(item.Variety1, item.Variety2);
			Dictionary<Meaning, CognacyDecision> decisions = _lookupDictionary[key];
			if (decisions.Remove(item.Meaning) && decisions.Count == 0)
				_lookupDictionary.Remove(key);
		}
	}
}
