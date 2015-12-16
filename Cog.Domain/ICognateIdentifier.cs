namespace SIL.Cog.Domain
{
	public interface ICognateIdentifier
	{
		void UpdatePredictedCognacy(WordPair wordPair, IWordAlignerResult alignerResult);
	}
}
