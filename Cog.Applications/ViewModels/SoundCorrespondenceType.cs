using System.ComponentModel;

namespace SIL.Cog.Applications.ViewModels
{
	public enum SoundCorrespondenceType
	{
		[Description("Initial consonants")]
		InitialConsonants,
		[Description("Medial consonants")]
		MedialConsonants,
		[Description("Final consonants")]
		FinalConsonants,
		[Description("Onset consonants")]
		OnsetConsonants,
		[Description("Coda consonants")]
		CodaConsonants,
		[Description("Vowels")]
		Vowels
	}
}
