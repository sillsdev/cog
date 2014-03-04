namespace SIL.Cog.Application.ViewModels
{
	public class GlobalVowelVertex : GlobalSegmentVertex
	{
		private readonly VowelHeight _height;
		private readonly VowelBackness _backness;
		private readonly bool _round;

		public GlobalVowelVertex(VowelHeight height, VowelBackness backness, bool round)
		{
			_height = height;
			_backness = backness;
			_round = round;
		}

		public VowelHeight Height
		{
			get { return _height; }
		}

		public VowelBackness Backness
		{
			get { return _backness; }
		}

		public bool Round
		{
			get { return _round; }
		}
	}
}
