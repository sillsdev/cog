using System.Collections.Generic;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public interface IAligner
	{
		IEnumerable<NaturalClass> NaturalClasses { get; }

		IAlignerResult Compute(VarietyPair varietyPair, Word word1, Word word2);
		IAlignerResult Compute(WordPair wordPair);

		int SigmaInsertion(VarietyPair varietyPair, ShapeNode q);
		int SigmaDeletion(VarietyPair varietyPair, ShapeNode p);
		int SigmaSubstitution(VarietyPair varietyPair, ShapeNode p, ShapeNode q);
		int SigmaExpansion(VarietyPair varietyPair, ShapeNode p, ShapeNode q1, ShapeNode q2);
		int SigmaCompression(VarietyPair varietyPair, ShapeNode p1, ShapeNode p2, ShapeNode q);
		int Delta(FeatureStruct fs1, FeatureStruct fs2);
		int GetMaxScore1(VarietyPair varietyPair, ShapeNode p);
		int GetMaxScore2(VarietyPair varietyPair, ShapeNode q);
	}
}
