using System.Linq;
using NUnit.Framework;
using SIL.Cog;
using SIL.Cog.Clusterers;

namespace CogLibTest
{
	public class UpgmaClustererTest : ClustererTestBase
	{
		[Test]
		public void Cluster()
		{
			var matrix = new double[,]
				{
					{0, 2, 4, 6, 6, 8},
					{2, 0, 4, 6, 6, 8},
					{4, 4, 0, 6, 6, 8},
					{6, 6, 6, 0, 4, 8},
					{6, 6, 6, 4, 0, 8},
					{8, 8, 8, 8, 8, 0}
				};
			var upgma = new UpgmaClusterer<char>((o1, o2) => matrix[o1 - 'A', o2 - 'A']);
			Cluster<char>[] clusters = upgma.GenerateClusters(new[] {'A', 'B', 'C', 'D', 'E', 'F'}).ToArray();

			var root = new Cluster<char>("4") {Children =
				{
					{new Cluster<char>("F", new[] {'F'}), 4},
					{new Cluster<char>("3") {Children =
						{
							{new Cluster<char>("1") {Children =
								{
									{new Cluster<char>("C", new[] {'C'}), 2},
									{new Cluster<char>("0") {Children =
										{
											{new Cluster<char>("A", new[] {'A'}), 1},
											{new Cluster<char>("B", new[] {'B'}), 1}
										}}, 1}
								}}, 1},
							{new Cluster<char>("2") {Children =
								{
									{new Cluster<char>("D", new[] {'D'}), 2},
									{new Cluster<char>("E", new[] {'E'}), 2}
								}}, 1}
						}}, 1}
				}};
			Assert.That(clusters.Length, Is.EqualTo(1));
			AssertClustersEqual(clusters[0], root);
		}

		[Test]
		public void ClusterNoDataObjects()
		{
			var upgma = new UpgmaClusterer<char>((o1, o2) => 0);
			Cluster<char>[] clusters = upgma.GenerateClusters(Enumerable.Empty<char>()).ToArray();
			Assert.That(clusters, Is.Empty);
		}

		[Test]
		public void ClusterOneDataObject()
		{
			var upgma = new UpgmaClusterer<char>((o1, o2) => 0);
			Cluster<char>[] clusters = upgma.GenerateClusters(new[] {'A'}).ToArray();
			Assert.That(clusters.Length, Is.EqualTo(1));
			AssertClustersEqual(clusters[0], new Cluster<char>("A", new[] {'A'}));
		}

		[Test]
		public void ClusterTwoDataObjects()
		{
			var upgma = new UpgmaClusterer<char>((o1, o2) => 1);
			Cluster<char>[] clusters = upgma.GenerateClusters(new[] {'A', 'B'}).ToArray();
			Assert.That(clusters.Length, Is.EqualTo(1));
			AssertClustersEqual(clusters[0], new Cluster<char>("0") {Children =
				{
					{new Cluster<char>("A", new[] {'A'}), 0.5},
					{new Cluster<char>("B", new[] {'B'}), 0.5}
				}});
		}
	}
}
