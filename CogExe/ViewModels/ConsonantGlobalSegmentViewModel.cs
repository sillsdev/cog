namespace SIL.Cog.ViewModels
{
	public enum ConsonantPlace
	{
		Bilabial,
		Labiodental,
		Dental,
		Alveolar,
		Postaveolar,
		Retroflex,
		Palatal,
		Velar,
		Uvular,
		Pharyngeal,
		Glottal
	}

	public enum ConsonantManner
	{
		Plosive,
		Nasal,
		Trill,
		TapOrFlap,
		Fricative,
		LateralFricative,
		Approximant,
		LateralApproximant
	}

	public class ConsonantGlobalSegmentViewModel : GlobalSegmentViewModel
	{
		private readonly ConsonantPlace _place;
		private readonly ConsonantManner _manner;
		private readonly bool _voice;

		public ConsonantGlobalSegmentViewModel(ConsonantPlace place, ConsonantManner manner, bool voice)
		{
			_place = place;
			_manner = manner;
			_voice = voice;
		}

		public ConsonantPlace Place
		{
			get { return _place; }
		}

		public ConsonantManner Manner
		{
			get { return _manner; }
		}

		public bool Voice
		{
			get { return _voice; }
		}
	}
}
