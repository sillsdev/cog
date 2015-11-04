using SIL.Cog.Domain;

namespace SIL.Cog.CommandLine
{
	public class MeaningFactory
	{
		private static int _counter = 0;

		public static Meaning Create()
		{
			_counter++;
			return new Meaning(string.Format("Meaning {0}", _counter), string.Format("Category {0}", _counter));
		}
	}
}
