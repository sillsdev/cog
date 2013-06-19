using System.Collections.Generic;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public interface IWordPairAligner
	{
		IEnumerable<SoundClass> ContextualSoundClasses { get; }
		bool ExpansionCompressionEnabled { get; }

		IWordPairAlignerResult Compute(VarietyPair varietyPair, Word word1, Word word2);
		IWordPairAlignerResult Compute(WordPair wordPair);

		int Delta(FeatureStruct fs1, FeatureStruct fs2);
	}
}
