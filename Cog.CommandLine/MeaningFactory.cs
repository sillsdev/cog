using SIL.Cog.Domain;

namespace SIL.Cog.CommandLine
{
	public class MeaningFactory
	{
		private static int _counter;

		public static Meaning Create()
		{
			_counter++;
			return new Meaning($"Meaning {_counter}", $"Category {_counter}");
		}
	}
}
