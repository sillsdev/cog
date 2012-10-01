namespace SIL.Cog
{
	public interface IProcessor<in T>
	{
		void Process(T data);
	}
}
