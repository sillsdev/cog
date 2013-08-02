using System;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain
{
	public class CogFeatureSystem : FeatureSystem
	{
		public static readonly SymbolicFeature Type;
		public static readonly FeatureSymbol AnchorType;
		
		public static readonly FeatureSymbol VowelType;
		public static readonly FeatureSymbol ConsonantType;
		public static readonly FeatureSymbol ToneLetterType;
		public static readonly FeatureSymbol BoundaryType;

		public static readonly FeatureSymbol SyllableType;

		public static readonly FeatureSymbol PrefixType;
		public static readonly FeatureSymbol SuffixType;
		public static readonly FeatureSymbol StemType;

		public static readonly StringFeature OriginalStrRep;
		public static readonly StringFeature StrRep;

		public static readonly CogFeatureSystem Instance;

		static CogFeatureSystem()
		{
			AnchorType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "anchor"};
			VowelType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "vowel"};
			ConsonantType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "consonant"};
			ToneLetterType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "toneLetter"};
			BoundaryType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "boundary"};
			SyllableType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "syllable"};
			StemType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "stem"};
			PrefixType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "prefix"};
			SuffixType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "suffix"};

			Type = new SymbolicFeature(Guid.NewGuid().ToString(), AnchorType, VowelType, ConsonantType, ToneLetterType, BoundaryType, SyllableType, StemType, PrefixType, SuffixType) {Description = "Type"};

			OriginalStrRep = new StringFeature(Guid.NewGuid().ToString()) {Description = "OriginalStrRep"};
			StrRep = new StringFeature(Guid.NewGuid().ToString()) {Description = "StrRep"};

			Instance = new CogFeatureSystem();
		}

		private CogFeatureSystem()
		{
			Add(Type);
			Add(OriginalStrRep);
			Add(StrRep);
			Freeze();
		}
	}
}
