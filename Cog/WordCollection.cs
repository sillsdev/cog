using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog
{
	public class WordCollection : IReadOnlyCollection<Word>
	{
		private readonly Dictionary<Sense, ReadOnlyCollection<Word>> _words;
		private readonly ReadOnlyCollection<Word> _emptyWords; 

		internal WordCollection(IEnumerable<Word> words)
		{
			_words = words.GroupBy(w => w.Sense).ToDictionary(grouping => grouping.Key, grouping => new ReadOnlyCollection<Word>(grouping.ToArray()));
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
				ReadOnlyCollection<Word> words;
				if (_words.TryGetValue(sense, out words))
					return words.AsReadOnlyCollection();
				return _emptyWords;
			}
		}

		public int Count
		{
			get { return _words.Values.Sum(list => list.Count); }
		}
	}
}
