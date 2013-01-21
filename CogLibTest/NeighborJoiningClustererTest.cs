using System.Linq;
using NUnit.Framework;
using SIL.Cog.Clusterers;

namespace SIL.Cog.Test
{
	public class NeighborJoiningClustererTest : ClustererTestBase
	{
		[Test]
		public void Cluster()
		{
			var matrix = new double[,]
				{
					{0, 1, 2, 3, 3},
					{1, 0, 2, 3, 3},
					{2, 2, 0, 3, 3},
					{3, 3, 3, 0, 1},
					{3, 3, 3, 1, 0}
				};
			var nj = new NeighborJoiningClusterer<char>((o1, o2) => matrix[o1 - 'A', o2 - 'A']);
			Cluster<char>[] clusters = nj.GenerateClusters(new[] {'A', 'B', 'C', 'D', 'E'}).ToArray();

			var root = new Cluster<char>("2") {Children =
				{
					{new Cluster<char>(new[] {'C'}) {Description = "C"}, 1.0},
					{new Cluster<char>("0") {Children =
						{
							{new Cluster<char>(new[] {'D'}) {Description = "D"}, 0.5},
							{new Cluster<char>(new[] {'E'}) {Description = "E"}, 0.5}
						}}, 1.5},
					{new Cluster<char>("1") {Children =
						{
							{new Cluster<char>(new[] {'A'}) {Description = "A"}, 0.5},
							{new Cluster<char>(new[] {'B'}) {Description = "B"}, 0.5}
						}}, 0.5}
				}};
			Assert.That(clusters.Length, Is.EqualTo(1));
			AssertClustersEqual(clusters[0], root);
		}

		[Test]
		public void ClusterNoDataObjects()
		{
			var nj = new NeighborJoiningClusterer<char>((o1, o2) => 0);
			Cluster<char>[] clusters = nj.GenerateClusters(Enumerable.Empty<char>()).ToArray();
			Assert.That(clusters, Is.Empty);
		}

		[Test]
		public void ClusterOneDataObject()
		{
			var nj = new NeighborJoiningClusterer<char>((o1, o2) => 0);
			Cluster<char>[] clusters = nj.GenerateClusters(new[] {'A'}).ToArray();
			Assert.That(clusters.Length, Is.EqualTo(1));
			AssertClustersEqual(clusters[0], new Cluster<char>(new[] {'A'}) {Description = "A"});
		}

		[Test]
		public void ClusterTwoDataObjects()
		{
			var nj = new NeighborJoiningClusterer<char>((o1, o2) => 1);
			Cluster<char>[] clusters = nj.GenerateClusters(new[] {'A', 'B'}).ToArray();
			Assert.That(clusters.Length, Is.EqualTo(1));
			AssertClustersEqual(clusters[0], new Cluster<char>(new[] {'B'}) {Description = "B", Children = {{new Cluster<char>(new[] {'A'}) {Description = "A"}, 1.0}}});
		}
	}
}
