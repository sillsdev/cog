using System.Collections.Generic;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog
{
	public class Cluster<T> : OrderedBidirTreeNode<Cluster<T>>, IIDBearer
	{
		private readonly string _id;
		private readonly HashSet<T> _dataObjects;
		private readonly bool _noise;

		public Cluster(string id, IEnumerable<T> dataObjects)
			: this(id, dataObjects, false)
		{
		}

		public Cluster(string id, IEnumerable<T> dataObjects, bool noise)
			: base(begin => new Cluster<T>(null, Enumerable.Empty<T>()))
		{
			_id = id;
			_dataObjects = new HashSet<T>(dataObjects);
			_noise = noise;
		}

		public string ID
		{
			get { return _id; }
		}

		public string Description { get; set; }

		public IEnumerable<T> DataObjects
		{
			get { return _dataObjects; }
		}

		public int DataObjectCount
		{
			get { return _dataObjects.Count; }
		}

		public bool Noise
		{
			get { return _noise; }
		}
	}
}
