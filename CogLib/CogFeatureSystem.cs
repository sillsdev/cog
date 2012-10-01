using System;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class CogFeatureSystem : FeatureSystem
	{
		public static readonly SymbolicFeature Type;
		public static readonly FeatureSymbol AnchorType;
		public static readonly FeatureSymbol VowelType;
		public static readonly FeatureSymbol ConsonantType;
		public static readonly FeatureSymbol NullType;
		public static readonly FeatureSymbol PrefixType;
		public static readonly FeatureSymbol SuffixType;
		public static readonly FeatureSymbol StemType;
		public static readonly FeatureSymbol ClusterType;

		public static readonly StringFeature StrRep;

		private static readonly CogFeatureSystem FeatureSystem;

		static CogFeatureSystem()
		{
			Type = new SymbolicFeature(Guid.NewGuid().ToString()) {Description = "Type"};
			AnchorType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "anchor"};
			Type.PossibleSymbols.Add(AnchorType);
			VowelType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "vowel"};
			Type.PossibleSymbols.Add(VowelType);
			ConsonantType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "consonant"};
			Type.PossibleSymbols.Add(ConsonantType);
			NullType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "null"};
			Type.PossibleSymbols.Add(NullType);
			StemType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "stem"};
			Type.PossibleSymbols.Add(StemType);
			PrefixType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "prefix"};
			Type.PossibleSymbols.Add(PrefixType);
			SuffixType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "suffix"};
			Type.PossibleSymbols.Add(SuffixType);
			ClusterType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "cluster"};
			Type.PossibleSymbols.Add(ClusterType);

			StrRep = new StringFeature(Guid.NewGuid().ToString()) {Description = "StrRep"};

			FeatureSystem = new CogFeatureSystem();
		}

		public static CogFeatureSystem Instance
		{
			get { return FeatureSystem; }
		}

		private CogFeatureSystem()
		{
			Add(Type);
			Add(StrRep);
			IsReadOnly = true;
		}
	}
}
