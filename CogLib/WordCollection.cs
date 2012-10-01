using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog
{
	public class WordCollection : ICollection<Word>, INotifyCollectionChanged
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		private readonly SimpleMonitor _reentrancyMonitor = new SimpleMonitor();
		private readonly Variety _variety;
		private readonly Dictionary<Sense, HashSet<Word>> _words;
		private readonly ReadOnlyCollection<Word> _emptyWords;

		internal WordCollection(Variety variety)
		{
			_variety = variety;
			_words = new Dictionary<Sense, HashSet<Word>>();
			_emptyWords = new ReadOnlyCollection<Word>(new Word[0]);
		}

		IEnumerator<Word> IEnumerable<Word>.GetEnumerator()
		{
			return _words.Values.SelectMany(list => list).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Word>) this).GetEnumerator();
		}

		public IEnumerable<Sense> Senses
		{
			get { return _words.Keys; }
		}

		public IReadOnlyCollection<Word> this[Sense sense]
		{
			get
			{
				HashSet<Word> words;
				if (_words.TryGetValue(sense, out words))
					return words.AsReadOnlyCollection();
				return _emptyWords;
			}
		}

		public void Add(Word item)
		{
			CheckReentrancy();
			HashSet<Word> senseWords = _words.GetValue(item.Sense, () => new HashSet<Word>());
			if (senseWords.Add(item))
			{
				_variety.Segments.WordAdded(item);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
			}
		}

		public void Clear()
		{
			CheckReentrancy();
			int count = _words.Count;
			_words.Clear();
			_variety.Segments.WordsCleared();
			if (count > 0)
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public bool Contains(Word item)
		{
			HashSet<Word> senseWords;
			if (_words.TryGetValue(item.Sense, out senseWords))
				return senseWords.Contains(item);
			return false;
		}

		public void CopyTo(Word[] array, int arrayIndex)
		{
			foreach (Word word in this)
				array[arrayIndex++] = word;
		}

		public bool Remove(Word item)
		{
			CheckReentrancy();
			HashSet<Word> senseWords;
			if (_words.TryGetValue(item.Sense, out senseWords))
			{
				if (senseWords.Remove(item))
				{
					if (senseWords.Count == 0)
						_words.Remove(item.Sense);
					_variety.Segments.WordRemoved(item);
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
					return true;
				}
			}
			return false;
		}

		public void RemoveAll(Sense sense)
		{
			_words.Remove(sense);
		}

		public int Count
		{
			get { return _words.Values.Sum(list => list.Count); }
		}

		public bool IsReadOnly
		{
			get { return false; }
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
