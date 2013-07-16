using System.Collections.Generic;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain
{
	public interface IWordAligner
	{
		IEnumerable<SoundClass> ContextualSoundClasses { get; }
		bool ExpansionCompressionEnabled { get; }

		IWordAlignerResult Compute(Word word1, Word word2);
		IWordAlignerResult Compute(WordPair wordPair);
		IWordAlignerResult Compute(IEnumerable<Word> words);

		int Delta(FeatureStruct fs1, FeatureStruct fs2);
	}
}
