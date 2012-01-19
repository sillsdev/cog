using SIL.Machine;

namespace SIL.Cog
{
	public abstract class EditDistance
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 

		protected EditDistance(SpanFactory<ShapeNode> spanFactory)
		{
			_spanFactory = spanFactory;
		}

		public EditDistanceMatrix Compute(WordPair wordPair)
		{
			return new EditDistanceMatrix(this, wordPair);
		}

		public SpanFactory<ShapeNode> SpanFactory
		{
			get { return _spanFactory; }
		}

		public abstract int SigmaInsertion(WordPair wordPair, ShapeNode q);

		public abstract int SigmaDeletion(WordPair wordPair, ShapeNode p);

		public abstract int SigmaSubstitution(WordPair wordPair, ShapeNode p, ShapeNode q);

		public abstract int SigmaExpansion(WordPair wordPair, ShapeNode p, ShapeNode q1, ShapeNode q2);

		public abstract int SigmaCompression(WordPair wordPair, ShapeNode p1, ShapeNode p2, ShapeNode q);

		public abstract int Delta(ShapeNode p, ShapeNode q);

		public abstract int GetMaxScore(WordPair wordPair, ShapeNode node);
	}
}
