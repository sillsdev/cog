using QuickGraph;

namespace SIL.Cog
{
	public class ClusterEdge<T> : Edge<Cluster<T>>
	{
		private readonly double _length;

		public ClusterEdge(Cluster<T> source, Cluster<T> target)
			: this(source, target, 0)
		{
		}

		public ClusterEdge(Cluster<T> source, Cluster<T> target, double length)
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
