using System.Collections.Generic;
using System.Linq;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Domain.Components
{
	public abstract class WordAlignerBase : IWordAligner
	{
		private readonly WordPairAlignerSettings _settings;

		protected WordAlignerBase(WordPairAlignerSettings settings)
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

		public IWordAlignerResult Compute(Word word1, Word word2)
		{
			return new PairwiseWordAlignerResult(this, Scorer, _settings, word1, word2);
		}

		public IWordAlignerResult Compute(WordPair wordPair)
		{
			return new PairwiseWordAlignerResult(this, Scorer, _settings, wordPair.Word1, wordPair.Word2);
		}

		public IWordAlignerResult Compute(IEnumerable<Word> words)
		{
			Word[] wordArray = words.ToArray();
			if (wordArray.Length == 2)
				return new PairwiseWordAlignerResult(this, Scorer, new WordPairAlignerSettings(), wordArray[0], wordArray[1]);
			return new MultipleWordAlignerResult(this, Scorer, wordArray);
		}

		public WordPairAlignerSettings Settings
		{
			get { return _settings; }
		}

		protected abstract IPairwiseAlignmentScorer<Word, ShapeNode> Scorer { get; }

		public abstract int Delta(FeatureStruct fs1, FeatureStruct fs2);
	}
}
