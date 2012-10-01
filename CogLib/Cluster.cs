using System.Collections.Generic;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog
{
	public class Cluster<T> : OrderedBidirTreeNode<Cluster<T>>, IIDBearer
	{
		private readonly string _id;
		private readonly string _desc;
		private readonly HashSet<T> _dataObjects;
		private readonly bool _noise;

		public Cluster(string id, IEnumerable<T> dataObjects)
			: this(id, dataObjects, id)
		{
		}

		public Cluster(string id, IEnumerable<T> dataObjects, string desc)
			: this(id, dataObjects, false, desc)
		{
		}

		public Cluster(string id, IEnumerable<T> dataObjects, bool noise)
			: this(id, dataObjects, noise, id)
		{
		}

		public Cluster(string id, IEnumerable<T> dataObjects, bool noise, string desc)
			: base(begin => new Cluster<T>(null, Enumerable.Empty<T>()))
		{
			_id = id;
			_desc = desc;
			_dataObjects = new HashSet<T>(dataObjects);
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
