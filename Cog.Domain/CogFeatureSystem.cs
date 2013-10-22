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

		public static readonly SymbolicFeature SegmentType;

		public static readonly FeatureSymbol Complex;
		public static readonly FeatureSymbol Simple;

		public static readonly StringFeature OriginalStrRep;
		public static readonly StringFeature StrRep;

		public static readonly ComplexFeature First;

		public static readonly SymbolicFeature SyllablePosition;
		public static readonly FeatureSymbol Onset;
		public static readonly FeatureSymbol Nucleus;
		public static readonly FeatureSymbol Coda;

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

			Complex = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "complex"};
			Simple = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "simple"};

			SegmentType = new SymbolicFeature(Guid.NewGuid().ToString(), Complex, Simple) {Description = "SegmentType"};

			OriginalStrRep = new StringFeature(Guid.NewGuid().ToString()) {Description = "OriginalStrRep"};
			StrRep = new StringFeature(Guid.NewGuid().ToString()) {Description = "StrRep"};

			First = new ComplexFeature(Guid.NewGuid().ToString()) {Description = "First"};

			Onset = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "onset"};
			Nucleus = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "nucleus"};
			Coda = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "coda"};

			SyllablePosition = new SymbolicFeature(Guid.NewGuid().ToString(), Onset, Nucleus, Coda) {Description = "SyllablePosition"};

			Instance = new CogFeatureSystem();
		}

		private CogFeatureSystem()
		{
			Add(Type);
			Add(SegmentType);
			Add(OriginalStrRep);
			Add(StrRep);
			Add(First);
			Add(SyllablePosition);
			Freeze();
		}
	}
}
