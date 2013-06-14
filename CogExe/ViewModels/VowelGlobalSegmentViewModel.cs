namespace SIL.Cog.ViewModels
{
	public enum VowelHeight
	{
		Close,
		NearClose,
		CloseMid,
		Mid,
		OpenMid,
		NearOpen,
		Open
	}

	public enum VowelBackness
	{
		Front,
		NearFront,
		Central,
		NearBack,
		Back
	}

	public class VowelGlobalSegmentViewModel : GlobalSegmentViewModel
	{
		private readonly VowelHeight _height;
		private readonly VowelBackness _backness;
		private readonly bool _round;

		public VowelGlobalSegmentViewModel(VowelHeight height, VowelBackness backness, bool round)
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
