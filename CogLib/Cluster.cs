using System.Collections.Generic;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog
{
	public class Cluster<T> : OrderedBidirTreeNode<Cluster<T>>, IIDBearer
	{
		private readonly string _id;
		private readonly string _desc;
		private readonly SimpleReadOnlyCollection<T> _dataObjects;
		private readonly bool _noise;
		private readonly double _height;

		public Cluster(string id, IEnumerable<T> dataObjects)
			: this(id, dataObjects, 0)
		{
		}

		public Cluster(string id, IEnumerable<T> dataObjects, double height)
			: this(id, dataObjects, height, false)
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
			: this(id, dataObjects, 0, noise, desc)
		{
		}

		public Cluster(string id, IEnumerable<T> dataObjects, double height, bool noise)
			: this(id, dataObjects, height, noise, id)
		{
		}

		public Cluster(string id, IEnumerable<T> dataObjects, double height, string desc)
			: this(id, dataObjects, height, false, desc)
		{
		}

		public Cluster(string id, IEnumerable<T> dataObjects, double height, bool noise, string desc)
			: base(begin => new Cluster<T>(null, Enumerable.Empty<T>()))
		{
			_id = id;
			_desc = desc;
			_dataObjects = new SimpleReadOnlyCollection<T>(dataObjects.ToArray());
			_noise = noise;
			_height = height;
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

		public double Height
		{
			get { return _height; }
		}
	}
}
