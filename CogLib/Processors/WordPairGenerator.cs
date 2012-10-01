using System.Linq;
using SIL.Collections;

namespace SIL.Cog.Processors
{
	public class WordPairGenerator : ProcessorBase<VarietyPair>
	{
		private readonly string _alignerID;

		public WordPairGenerator(CogProject project, string alignerID)
			: base(project)
		{
			_alignerID = alignerID;
		}

		public string AlignerID
		{
			get { return _alignerID; }
		}

		public override void Process(VarietyPair varietyPair)
		{
			IAligner aligner = Project.Aligners[_alignerID];
			varietyPair.WordPairs.Clear();
			foreach (Sense sense in varietyPair.Variety1.Words.Senses)
			{
				IReadOnlyCollection<Word> words1 = varietyPair.Variety1.Words[sense];
				IReadOnlyCollection<Word> words2 = varietyPair.Variety2.Words[sense];
				if (words1.Count == 1 && words2.Count == 1)
				{
					varietyPair.WordPairs.Add(words1.Single(), words2.Single());
				}
				else if (words2.Count > 0)
				{
					var bestwp = words1.SelectMany(w1 => words2.Select(w2 => new {Word1 = w1, Word2 = w2})).MaxBy(wp => aligner.Compute(varietyPair, wp.Word1, wp.Word2).BestScore);
					varietyPair.WordPairs.Add(bestwp.Word1, bestwp.Word2);
				}
			}
		}
	}
}
