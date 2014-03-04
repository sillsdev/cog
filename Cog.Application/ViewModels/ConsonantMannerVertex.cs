using System.ComponentModel;

namespace SIL.Cog.Application.ViewModels
{
	public enum ConsonantManner
	{
		[Description("Nasal")]
		Nasal,
		[Description("Stop")]
		Stop,
		[Description("Affricate")]
		Affricate,
		[Description("Fricative")]
		Fricative,
		[Description("Approximant")]
		Approximant,
		[Description("Flap or tap")]
		FlapOrTap,
		[Description("Trill")]
		Trill,
		[Description("Lateral fricative")]
		LateralFricative,
		[Description("Lateral approximant")]
		LateralApproximant
	}

	public class ConsonantMannerVertex : SegmentPropertyVertex
	{
		private readonly ConsonantManner _manner;

		public ConsonantMannerVertex(ConsonantManner manner)
		{
			_manner = manner;
		}

		public ConsonantManner Manner
		{
			get { return _manner; }
		}

		public override string StrRep
		{
			get { return GetEnumDescription(_manner); }
		}
	}
}
