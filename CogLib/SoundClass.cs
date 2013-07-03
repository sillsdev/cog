using SIL.Machine;

namespace SIL.Cog
{
	public abstract class SoundClass
	{
		private readonly string _name;

		protected SoundClass(string name)
		{
			_name = name;
		}

		public string Name
		{
			get { return _name; }
		}

		public abstract bool Matches(ShapeNode leftNode, Ngram target, ShapeNode rightNode);
	}
}
