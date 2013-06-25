﻿using System.Collections.Generic;
using QuickGraph;

namespace SIL.Cog
{
	public interface IUnrootedHierarchicalClusterer<T>
	{
		IUndirectedGraph<Cluster<T>, ClusterEdge<T>> GenerateClusters(IEnumerable<T> dataObjects);
	}
}
