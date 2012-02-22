using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public abstract class EditDistance
	{
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private readonly EditDistanceSettings _settings;

		protected EditDistance(SpanFactory<ShapeNode> spanFactory, EditDistanceSettings settings)
		{
			_spanFactory = spanFactory;
			_settings = settings;
			_settings.ReadOnly = true;
		}

		public EditDistanceMatrix Compute(VarietyPair varietyPair, Word word1, Word word2)
		{
			return new EditDistanceMatrix(this, varietyPair, word1, word2);
		}

		public EditDistanceMatrix Compute(WordPair wordPair)
		{
			return new EditDistanceMatrix(this, wordPair.VarietyPair, wordPair.Word1, wordPair.Word2);
		}

		public SpanFactory<ShapeNode> SpanFactory
		{
			get { return _spanFactory; }
		}

		public EditDistanceSettings Settings
		{
			get { return _settings; }
		}

		public abstract int SigmaInsertion(VarietyPair varietyPair, ShapeNode q);

		public abstract int SigmaDeletion(VarietyPair varietyPair, ShapeNode p);

		public abstract int SigmaSubstitution(VarietyPair varietyPair, ShapeNode p, ShapeNode q);

		public abstract int SigmaExpansion(VarietyPair varietyPair, ShapeNode p, ShapeNode q1, ShapeNode q2);

		public abstract int SigmaCompression(VarietyPair varietyPair, ShapeNode p1, ShapeNode p2, ShapeNode q);

		public abstract int Delta(FeatureStruct fs1, FeatureStruct fs2);

		public abstract int GetMaxScore(VarietyPair varietyPair, ShapeNode node);
	}
}
