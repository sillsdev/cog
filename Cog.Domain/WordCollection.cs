using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog.Domain
{
	public sealed class WordCollection : IObservableCollection<Word>
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
		private readonly Dictionary<Meaning, HashSet<Word>> _words;
		private readonly ReadOnlyCollection<Word> _emptyWords;

		internal WordCollection(Variety variety)
		{
			_variety = variety;
			_words = new Dictionary<Meaning, HashSet<Word>>();
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

		public IEnumerable<Meaning> Meanings
		{
			get { return _words.Keys; }
		}

		public ReadOnlyCollection<Word> this[Meaning meaning]
		{
			get
			{
				HashSet<Word> words;
				if (_words.TryGetValue(meaning, out words))
					return words.ToReadOnlyCollection();
				return _emptyWords;
			}
		}

		public void Add(Word item)
		{
			CheckReentrancy();
			HashSet<Word> meaningWords = _words.GetValue(item.Meaning, () => new HashSet<Word>());
			if (meaningWords.Add(item))
			{
				item.Variety = _variety;
				OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
			}
		}

		public void AddRange(IEnumerable<Word> words)
		{
			CheckReentrancy();
			var added = new List<Word>();
			foreach (Word word in words)
			{
				HashSet<Word> meaningWords = _words.GetValue(word.Meaning, () => new HashSet<Word>());
				if (meaningWords.Add(word))
				{
					word.Variety = _variety;
					added.Add(word);
				}
			}

			if (added.Count > 0)
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added));
		}

		public void Clear()
		{
			CheckReentrancy();
			int count = _words.Count;
			foreach (Word word in this)
				word.Variety = null;
			_words.Clear();
			if (count > 0)
			{
				OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		public bool Contains(Word item)
		{
			HashSet<Word> meaningWords;
			if (_words.TryGetValue(item.Meaning, out meaningWords))
				return meaningWords.Contains(item);
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
			HashSet<Word> meaningWords;
			if (_words.TryGetValue(item.Meaning, out meaningWords))
			{
				if (meaningWords.Remove(item))
				{
					item.Variety = null;
					if (meaningWords.Count == 0)
						_words.Remove(item.Meaning);
					OnPropertyChanged(new PropertyChangedEventArgs("Count"));
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
					return true;
				}
			}
			return false;
		}

		public void RemoveAll(Meaning meaning)
		{
			_words.Remove(meaning);
		}

		public int Count
		{
			get { return _words.Values.Sum(list => list.Count); }
		}

		public bool IsReadOnly
		{
			get { return false; }
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

		private void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
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
