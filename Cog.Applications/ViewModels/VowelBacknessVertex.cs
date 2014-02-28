using System.ComponentModel;

namespace SIL.Cog.Applications.ViewModels
{
	public enum VowelBackness
	{
		[Description("Front")]
		Front,
		[Description("Near-front")]
		NearFront,
		[Description("Central")]
		Central,
		[Description("Near-back")]
		NearBack,
		[Description("Back")]
		Back
	}

	public class VowelBacknessVertex : SegmentPropertyVertex
	{
		private readonly VowelBackness _backness;

		public VowelBacknessVertex(VowelBackness backness)
		{
			_backness = backness;
		}

		public VowelBackness Backness
		{
			get { return _backness; }
		}

		public override string StrRep
		{
			get { return GetEnumDescription(_backness); }
		}
	}
}
