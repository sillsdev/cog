using System.Collections.Generic;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog
{
	public class ClusterList<T> : OrderedBidirList<Cluster<T>>
	{
		private readonly Dictionary<Cluster<T>, double> _lengths;
		private readonly Cluster<T> _parent; 

		internal ClusterList(Cluster<T> parent)
			: base(EqualityComparer<Cluster<T>>.Default, begin => new Cluster<T>(null, Enumerable.Empty<T>()))
		{
			_parent = parent;
			_lengths = new Dictionary<Cluster<T>, double>();
		}

		internal Cluster<T> Parent
		{
			get { return _parent; }
		}

		public void Add(Cluster<T> cluster, double length)
		{
			Add(cluster);
			_lengths[cluster] = length;
		}

		public override void Clear()
		{
			base.Clear();
			_lengths.Clear();
		}

		public override bool Remove(Cluster<T> node)
		{
			if (base.Remove(node))
			{
				_lengths.Remove(node);
				return true;
			}

			return false;
		}

		public double GetLength(Cluster<T> cluster)
		{
			double length;
			if (_lengths.TryGetValue(cluster, out length))
				return length;
			return 0;
		}
	}
}
