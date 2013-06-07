using SIL.Cog.GraphAlgorithms;

namespace SIL.Cog.Views
{
	public class HeaderGridVertex : GridVertex
	{
		private readonly string _name;

		public HeaderGridVertex(string name)
		{
			_name = name;
		}

		public string Name
		{
			get { return _name; }
		}
	}
}
