using QuickGraph;
using SIL.Cog.Applications.GraphAlgorithms;

namespace SIL.Cog.Applications.ViewModels
{
	public class HierarchicalGraphEdge : Edge<HierarchicalGraphVertex>, ILengthEdge<HierarchicalGraphVertex>
	{
		private readonly double _length;

		public HierarchicalGraphEdge(HierarchicalGraphVertex source, HierarchicalGraphVertex target, double length)
			: base(source, target)
		{
			_length = length;
		}

		public double Length
		{
			get { return _length; }
		}
	}
}
