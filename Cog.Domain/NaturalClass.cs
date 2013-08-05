using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain
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

		public override bool Matches(ShapeNode leftNode, Ngram target, ShapeNode rightNode)
		{
			foreach (Segment seg in target)
			{
				if (_fs.IsUnifiable(seg.FeatureStruct))
					return true;
			}
			return false;
		}
	}
}
