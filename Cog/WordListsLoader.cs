using System;
using System.Collections.Generic;

namespace SIL.Cog
{
	public abstract class WordListsLoader
	{
		private readonly Segmenter _segmenter;

		protected WordListsLoader(Segmenter segmenter)
		{
			_segmenter = segmenter;
		}

		protected Segmenter Segmenter
		{
			get { return _segmenter; }
		}

		public abstract IEnumerable<Variety> Load();

		protected void LoadVarietyPairs(IList<Variety> varieties)
		{
			LoadVarietyPairs(varieties, null);
		}

		protected void LoadVarietyPairs(IList<Variety> varieties, Action<VarietyPair> action)
		{
			for (int i = 0; i < varieties.Count; i++)
			{
				for (int j = i + 1; j < varieties.Count; j++)
				{
					VarietyPair varietyPair = VarietyPair.Create(varieties[i], varieties[j]);
					if (action != null)
						action(varietyPair);
				}
			}
		}
	}
}
