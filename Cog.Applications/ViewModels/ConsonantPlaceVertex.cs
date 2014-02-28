using System.ComponentModel;

namespace SIL.Cog.Applications.ViewModels
{
	public enum ConsonantPlace
	{
		[Description("Bilabial")]
		Bilabial,
		[Description("Labiodental")]
		Labiodental,
		[Description("Dental")]
		Dental,
		[Description("Alveolar")]
		Alveolar,
		[Description("Postalveolar")]
		Postalveolar,
		[Description("Retroflex")]
		Retroflex,
		[Description("Palatal")]
		Palatal,
		[Description("Velar")]
		Velar,
		[Description("Uvular")]
		Uvular,
		[Description("Pharyngeal")]
		Pharyngeal,
		[Description("Glottal")]
		Glottal
	}

	public class ConsonantPlaceVertex : SegmentPropertyVertex
	{
		private readonly ConsonantPlace _place;

		public ConsonantPlaceVertex(ConsonantPlace place)
		{
			_place = place;
		}

		public ConsonantPlace Place
		{
			get { return _place; }
		}

		public override string StrRep
		{
			get { return GetEnumDescription(_place); }
		}
	}
}
