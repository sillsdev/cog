using System.ComponentModel;

namespace SIL.Cog.Application.ViewModels
{
	public enum SimilarityMetric
	{
		[Description("Lexical")]
		Lexical,
		[Description("Phonetic")]
		Phonetic
	}
}
