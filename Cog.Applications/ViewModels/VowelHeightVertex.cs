using System.ComponentModel;

namespace SIL.Cog.Applications.ViewModels
{
	public enum VowelHeight
	{
		[Description("Close")]
		Close,
		[Description("Near-close")]
		NearClose,
		[Description("Close-mid")]
		CloseMid,
		[Description("Mid")]
		Mid,
		[Description("Open-mid")]
		OpenMid,
		[Description("Near-open")]
		NearOpen,
		[Description("Open")]
		Open
	}

	public class VowelHeightVertex : SegmentPropertyVertex
	{
		private readonly VowelHeight _height;

		public VowelHeightVertex(VowelHeight height)
		{
			_height = height;
		}

		public VowelHeight Height
		{
			get { return _height; }
		}

		public override string StrRep
		{
			get { return GetEnumDescription(_height); }
		}
	}
}
