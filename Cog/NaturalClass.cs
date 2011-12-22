using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class NaturalClass : IDBearerBase
	{
		private readonly FeatureStruct _fs;

		public NaturalClass(string id, string type, FeatureStruct fs)
			: base(id)
		{
			_fs = fs;
			if (!string.IsNullOrEmpty(type))
				_fs.AddValue(AnnotationFeatureSystem.Type, type);
		}

		public string Type
		{
			get
			{
				StringFeatureValue sfv;
				if (FeatureStruct.TryGetValue(AnnotationFeatureSystem.Type, out sfv))
					return (string) sfv;
				return null;
			}
		}

		public FeatureStruct FeatureStruct
		{
			get { return _fs; }
		}
	}
}
