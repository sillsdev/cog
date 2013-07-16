namespace SIL.Cog.Domain
{
	public interface IProcessor<in T>
	{
		void Process(T data);
	}
}
