using SIL.Cog.Applications.GraphAlgorithms;

namespace SIL.Cog.Applications.ViewModels
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
