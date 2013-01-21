using GraphSharp;
using SIL.Cog.Controls;

namespace SIL.Cog.ViewModels
{
	public class HierarchicalGraphEdge : TypedEdge<HierarchicalGraphVertex>, ILengthEdge<HierarchicalGraphVertex>
	{
		private readonly double _length;

		public HierarchicalGraphEdge(HierarchicalGraphVertex source, HierarchicalGraphVertex target, EdgeTypes type, double length)
			: base(source, target, type)
		{
			_length = length;
		}

		public double Length
		{
			get { return _length; }
		}
	}
}
