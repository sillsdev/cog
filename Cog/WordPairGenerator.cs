﻿using System.Linq;
using SIL.Machine;

namespace SIL.Cog
{
	public class WordPairGenerator : IProcessor<VarietyPair>
	{
		private readonly EditDistance _editDistance;

		public WordPairGenerator(EditDistance editDistance)
		{
			_editDistance = editDistance;
		}

		public void Process(VarietyPair varietyPair)
		{
			foreach (Sense sense in varietyPair.Variety1.Senses)
			{
				IReadOnlyCollection<Word> words1 = varietyPair.Variety1.GetWords(sense);
				IReadOnlyCollection<Word> words2 = varietyPair.Variety2.GetWords(sense);
				if (words1.Count == 1 && words2.Count == 1)
				{
					varietyPair.AddWordPair(words1.Single(), words2.Single());
				}
				else if (words2.Count > 0)
				{
					var bestwp = words1.SelectMany(w1 => words2.Select(w2 => new {Word1 = w1, Word2 = w2})).MaxBy(wp => _editDistance.Compute(varietyPair, wp.Word1, wp.Word2).BestScore);
					varietyPair.AddWordPair(bestwp.Word1, bestwp.Word2);
				}
			}
		}
	}
}