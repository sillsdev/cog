using System.Collections.Generic;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog
{
	public class Cluster<T> : OrderedBidirListNode<Cluster<T>>, IOrderedBidirTreeNode<Cluster<T>>
	{
		private readonly SimpleReadOnlyCollection<T> _dataObjects;
		private readonly bool _noise;
		private ClusterList<T> _children;

		public Cluster()
			: this(Enumerable.Empty<T>())
		{
		}

		public Cluster(IEnumerable<T> dataObjects)
			: this(dataObjects, false)
		{
		}

		public Cluster(IEnumerable<T> dataObjects, bool noise)
		{
			_dataObjects = new SimpleReadOnlyCollection<T>(dataObjects.ToArray());
			_noise = noise;
		}

		public string Description { get; set; }

		public IReadOnlyCollection<T> DataObjects
		{
			get { return _dataObjects; }
		}

		public IEnumerable<T> AllDataObjects
		{
			get
			{
				if (IsLeaf)
					return _dataObjects;

				return _children.Aggregate((IEnumerable<T>) _dataObjects, (res, c) => res.Concat(c.AllDataObjects));
			}
		}

		public bool Noise
		{
			get { return _noise; }
		}

		public Cluster<T> Parent { get; private set; }

		public int Depth { get; private set; }

		public bool IsLeaf
		{
			get
			{
				return _children == null || _children.Count == 0;
			}
		}

		public Cluster<T> Root { get; private set; }

		public ClusterList<T> Children
		{
			get
			{
				if (_children == null)
					_children = new ClusterList<T>(this);
				return _children;
			}
		}

		protected override void Clear()
		{
			base.Clear();
			Parent = null;
			Depth = 0;
			Root = this;
		}

		protected override void Init(OrderedBidirList<Cluster<T>> list)
		{
			base.Init(list);
			Parent = ((ClusterList<T>) list).Parent;
			if (Parent != null)
			{
				Depth = Parent.Depth + 1;
				Root = Parent.Root;
			}
		}


		IOrderedBidirList<Cluster<T>> IOrderedBidirTreeNode<Cluster<T>>.Children
		{
			get { return Children; }
		}

		IBidirList<Cluster<T>> IBidirTreeNode<Cluster<T>>.Children
		{
			get { return Children; }
		}

		public override string ToString()
		{
			return Description;
		}
	}
}
