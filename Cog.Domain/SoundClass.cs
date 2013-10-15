using SIL.Machine;
using SIL.Machine.NgramModeling;

namespace SIL.Cog.Domain
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

		public abstract bool Matches(ShapeNode leftNode, Ngram<Segment> target, ShapeNode rightNode);
	}
}
