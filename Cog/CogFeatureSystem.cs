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
		public static readonly FeatureSymbol StemType;
		public static readonly FeatureSymbol ClusterType;

		public static readonly StringFeature StrRep;

		private static readonly CogFeatureSystem FeatureSystem;

		static CogFeatureSystem()
		{
			Type = new SymbolicFeature(Guid.NewGuid().ToString()) {Description = "Type"};
			AnchorType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "anchor"};
			Type.AddPossibleSymbol(AnchorType);
			VowelType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "vowel"};
			Type.AddPossibleSymbol(VowelType);
			ConsonantType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "consonant"};
			Type.AddPossibleSymbol(ConsonantType);
			NullType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "null"};
			Type.AddPossibleSymbol(NullType);
			StemType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "stem"};
			Type.AddPossibleSymbol(StemType);
			ClusterType = new FeatureSymbol(Guid.NewGuid().ToString()) {Description = "cluster"};
			Type.AddPossibleSymbol(ClusterType);

			StrRep = new StringFeature(Guid.NewGuid().ToString()) {Description = "StrRep"};

			FeatureSystem = new CogFeatureSystem();
		}

		public static CogFeatureSystem Instance
		{
			get { return FeatureSystem; }
		}

		private CogFeatureSystem()
		{
			base.AddFeature(Type);
			base.AddFeature(StrRep);
		}

		public override void AddFeature(Feature feature)
		{
			throw new NotSupportedException("This feature system is readonly.");
		}

		public override void Reset()
		{
			throw new NotSupportedException("This feature system is readonly.");
		}
	}
}
