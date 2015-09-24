namespace SIL.Cog.Domain
{
	public interface ICognateIdentifier
	{
		void UpdateCognacy(WordPair wordPair, IWordAlignerResult alignerResult);
	}
}
