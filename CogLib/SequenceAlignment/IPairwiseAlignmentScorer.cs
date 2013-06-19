namespace SIL.Cog.SequenceAlignment
{
	public interface IPairwiseAlignmentScorer<in T> where T : class
	{
		int GetInsertionScore(T p, T q);
		int GetDeletionScore(T p, T q);
		int GetSubstitutionScore(T p, T q);
		int GetExpansionScore(T p, T q1, T q2);
		int GetCompressionScore(T p1, T p2, T q);
		int GetMaxScore1(T p);
		int GetMaxScore2(T q);
	}
}
