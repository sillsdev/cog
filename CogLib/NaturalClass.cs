using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class NaturalClass : SoundClass
	{
		private readonly FeatureStruct _fs;

		public NaturalClass(string name, FeatureStruct fs)
			: base(name)
		{
			_fs = fs;
			_fs.Freeze();
		}

		public FeatureSymbol Type
		{
			get { return (FeatureSymbol) _fs.GetValue(CogFeatureSystem.Type); }
		}

		public FeatureStruct FeatureStruct
		{
			get { return _fs; }
		}

		public override bool Matches(Annotation<ShapeNode> ann)
		{
			return _fs.Subsumes(ann.FeatureStruct);
		}
	}
}
