using System.ComponentModel;

namespace SIL.Cog.Applications.ViewModels
{
	public enum SoundCorrespondenceType
	{
		[Description("Stem-initial consonants")]
		StemInitialConsonants,
		[Description("Stem-medial consonants")]
		StemMedialConsonants,
		[Description("Stem-final consonants")]
		StemFinalConsonants,
		[Description("Onsets")]
		Onsets,
		[Description("Codas")]
		Codas,
		[Description("Vowels")]
		Vowels
	}
}
