using System;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class CogFeatureSystem : FeatureSystem
	{
		public static readonly StringFeature StrRep;

		public const string AnchorType = "anchor";
		public const string VowelType = "vowel";
		public const string ConsonantType = "consonant";
		public const string BoundaryType = "boundary";
		public const string ToneType = "tone";
		public const string NullType = "null";
		public const string StemType = "stem";
		public const string ClusterType = "cluster";

		private static readonly CogFeatureSystem FeatureSystem;

		static CogFeatureSystem()
		{
			StrRep = new StringFeature(Guid.NewGuid().ToString()) {Description = "StrRep"};

			FeatureSystem = new CogFeatureSystem();
		}

		public static CogFeatureSystem Instance
		{
			get { return FeatureSystem; }
		}

		private CogFeatureSystem()
		{
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
