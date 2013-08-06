using System.Linq;
using SIL.Collections;

namespace SIL.Cog.Domain.Components
{
	public class WordPairGenerator : IProcessor<VarietyPair>
	{
		private readonly CogProject _project;
		private readonly string _alignerID;

		public WordPairGenerator(CogProject project, string alignerID)
		{
			_project = project;
			_alignerID = alignerID;
		}

		public string AlignerID
		{
			get { return _alignerID; }
		}

		public void Process(VarietyPair varietyPair)
		{
			IWordAligner aligner = _project.WordAligners[_alignerID];
			varietyPair.WordPairs.Clear();
			foreach (Sense sense in varietyPair.Variety1.Words.Senses)
			{
				IReadOnlyCollection<Word> words1 = varietyPair.Variety1.Words[sense];
				IReadOnlyCollection<Word> words2 = varietyPair.Variety2.Words[sense];
				if (words1.Count == 1 && words2.Count == 1)
				{
					Word word1 = words1.Single();
					Word word2 = words2.Single();
					if (word1.Shape.Count > 0 && word2.Shape.Count > 0)
						varietyPair.WordPairs.Add(word1, word2);
				}
				else if (words2.Count > 0)
				{
					var candidates = words1.Where(word => word.Shape.Count > 0)
						.SelectMany(w1 => words2.Where(word => word.Shape.Count > 0).Select(w2 => new {Word1 = w1, Word2 = w2})).ToArray();
					if (candidates.Length > 0)
					{
						var bestwp = candidates.MaxBy(wp => aligner.Compute(wp.Word1, wp.Word2).GetAlignments().First().NormalizedScore);
						varietyPair.WordPairs.Add(bestwp.Word1, bestwp.Word2);
					}
				}
			}
		}
	}
}
