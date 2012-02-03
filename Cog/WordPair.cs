namespace SIL.Cog
{
	public class WordPair
	{
		private readonly VarietyPair _varietyPair;
		private readonly Word _word1;
		private readonly Word _word2;

		public WordPair(VarietyPair varietyPair, Word word1, Word word2)
		{
			_varietyPair = varietyPair;
			_word1 = word1;
			_word2 = word2;
		}

		public VarietyPair VarietyPair
		{
			get { return _varietyPair; }
		}

		public Word Word1
		{
			get { return _word1; }
		}

		public Word Word2
		{
			get { return _word2; }
		}

		public bool AreCognatesActual { get; set; }

		public bool AreCognatesPredicted { get; set; }

		public double PhoneticSimilarityScore { get; set; }
	}
}
