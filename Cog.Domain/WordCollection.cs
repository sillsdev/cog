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
					return words.ToReadOnlyCollection();
				return _emptyWords;
			}
		}

		public void Add(Word item)
		{
			CheckReentrancy();
			HashSet<Word> senseWords = _words.GetValue(item.Sense, () => new HashSet<Word>());
			if (senseWords.Add(item))
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
				HashSet<Word> senseWords = _words.GetValue(word.Sense, () => new HashSet<Word>());
				if (senseWords.Add(word))
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
					item.Variety = null;
					if (senseWords.Count == 0)
						_words.Remove(item.Sense);
					OnPropertyChanged(new PropertyChangedEventArgs("Count"));
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

		private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (CollectionChanged != null)
			{
				using (_reentrancyMonitor.Enter())
					CollectionChanged(this, e);
			}
		}

		private void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				using (_reentrancyMonitor.Enter())
					PropertyChanged(this, e);
			}
		}

		private void CheckReentrancy()
		{
			if (_reentrancyMonitor.Busy)
				throw new InvalidOperationException("This collection cannot be changed during a CollectionChanged event.");
		}
	}
}
