using System;
using NUnit.Framework;
using SIL.Collections;

namespace SIL.Cog.Test
{
	[TestFixture]
	public abstract class ClustererTestBase
	{
		protected void AssertClustersEqual<T>(Cluster<T> actual, Cluster<T> expected)
		{
			Assert.That(actual.DataObjects, Is.EquivalentTo(expected.DataObjects));
			Assert.That(actual.Children.Count, Is.EqualTo(expected.Children.Count));
			foreach (Tuple<Cluster<T>, Cluster<T>> t in actual.Children.Zip(expected.Children))
			{
				Assert.That(actual.Children.GetLength(t.Item1), Is.EqualTo(expected.Children.GetLength(t.Item2)));
				AssertClustersEqual(t.Item1, t.Item2);
			}
		}
	}
}
