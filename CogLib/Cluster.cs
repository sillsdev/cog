using System.Collections.Generic;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog
{
	public class Cluster<T> : OrderedBidirListNode<Cluster<T>>, IOrderedBidirTreeNode<Cluster<T>>, IIDBearer
	{
		private readonly string _id;
		private readonly string _desc;
		private readonly SimpleReadOnlyCollection<T> _dataObjects;
		private readonly bool _noise;
		private ClusterList<T> _children; 

		public Cluster(string id, IEnumerable<T> dataObjects)
			: this(id, dataObjects, id)
		{
		}

		public Cluster(string id, IEnumerable<T> dataObjects, bool noise)
			: this(id, dataObjects, noise, id)
		{
		}

		public Cluster(string id, IEnumerable<T> dataObjects, string desc)
			: this(id, dataObjects, false, desc)
		{
		}

		public Cluster(string id, IEnumerable<T> dataObjects, bool noise, string desc)
		{
			_id = id;
			_desc = desc;
			_dataObjects = new SimpleReadOnlyCollection<T>(dataObjects.ToArray());
			_noise = noise;
		}

		public string ID
		{
			get { return _id; }
		}

		public string Description
		{
			get { return _desc; }
		}

		public IReadOnlyCollection<T> DataObjects
		{
			get { return _dataObjects; }
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
	}
}
