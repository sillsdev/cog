using SIL.Machine;
using System.Linq;

namespace SIL.Cog
{
	public enum AffixType
	{
		Prefix,
		Suffix
	}

	public class Affix
	{
		private readonly AffixType _type;
		private readonly Shape _shape;
		private readonly string _category;

		public Affix(AffixType type, Shape shape, string category)
		{
			_shape = shape;
			_type = type;
			_category = category;
		}

		public Shape Shape
		{
			get { return _shape; }
		}

		public AffixType Type
		{
			get { return _type; }
		}

		public string Category
		{
			get { return _category; }
		}

		public string StrRep
		{
			get { return string.Concat(_shape.Select(node => (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep))); }
		}

		public double Score { get; set; }

		public override string ToString()
		{
			string strRep = StrRep;
			return _type == AffixType.Prefix ? strRep + "-" : "-" + strRep;
		}
	}
}
