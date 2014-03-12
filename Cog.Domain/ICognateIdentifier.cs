namespace SIL.Cog.Domain
{
	public interface ICognateIdentifier
	{
		void UpdateCognicity(WordPair wordPair, IWordAlignerResult alignerResult);
	}
}
