using System.Collections.Generic;
using SIL.Cog.SequenceAlignment;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Components
{
	public abstract class WordPairAlignerBase : IWordPairAligner
	{
		private readonly WordPairAlignerSettings _settings;

		protected WordPairAlignerBase(WordPairAlignerSettings settings)
		{
			_settings = settings;
			_settings.ReadOnly = true;
		}

		public IEnumerable<SoundClass> ContextualSoundClasses
		{
			get { return _settings.ContextualSoundClasses; }
		}

		public bool ExpansionCompressionEnabled
		{
			get { return _settings.ExpansionCompressionEnabled; }
		}

		public IWordPairAlignerResult Compute(Word word1, Word word2)
		{
			return new WordPairAlignerResult(Scorer, _settings, word1, word2);
		}

		public IWordPairAlignerResult Compute(WordPair wordPair)
		{
			return new WordPairAlignerResult(Scorer, _settings, wordPair.Word1, wordPair.Word2);
		}

		public WordPairAlignerSettings Settings
		{
			get { return _settings; }
		}

		protected abstract IPairwiseAlignmentScorer<Word, ShapeNode> Scorer { get; }

		public abstract int Delta(FeatureStruct fs1, FeatureStruct fs2);
	}
}
