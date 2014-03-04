using System;
using System.Linq;
using NUnit.Framework;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;

namespace SIL.Cog.Domain.Tests.Components
{
	[TestFixture]
	public class ListSegmentMappingsTests
	{
		private SpanFactory<ShapeNode> _spanFactory;
		private Segmenter _segmenter;

		[SetUp]
		public void SetUp()
		{
			_spanFactory = new ShapeSpanFactory();
			_segmenter = new Segmenter(_spanFactory)
				{
					Consonants = {"b", "c", "ch", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "sh", "t", "v", "w", "x", "z"},
					Vowels = {"a", "e", "i", "o", "u"},
					Boundaries = {"-"},
					Modifiers = {"\u0303", "\u0308"},
					Joiners = {"\u0361"}
				};
		}

		[Test]
		public void IsMapped()
		{
			var segmentPool = new SegmentPool();

			var mappings = new ListSegmentMappings(_segmenter, new[]
				{
					Tuple.Create("m", "n"),
					Tuple.Create("t", "-"),
					Tuple.Create("h#", "-#"),
					Tuple.Create("c", "#g"),
					Tuple.Create("f", "@"),
					Tuple.Create("a", "o"),
					Tuple.Create("Cw", "-V")
				}, false);

			Shape shape1 = _segmenter.Segment("ma͡et");
			Shape shape2 = _segmenter.Segment("no");

			Assert.That(mappings.IsMapped(shape1.First.Prev, segmentPool.Get(shape1.First), shape1.First.Next, shape2.First.Prev, segmentPool.Get(shape2.First), shape2.First.Next), Is.True);
			Assert.That(mappings.IsMapped(shape1.Last.Prev, segmentPool.Get(shape1.Last), shape1.Last.Next, shape2.Last, new Ngram<Segment>(), shape2.Last.Next), Is.True);
			Assert.That(mappings.IsMapped(shape1.First, segmentPool.Get(shape1.First.Next), shape1.Last, shape2.First, segmentPool.Get(shape2.Last), shape2.Last.Next), Is.False);

			shape1 = _segmenter.Segment("goh");
			shape2 = _segmenter.Segment("co");

			Assert.That(mappings.IsMapped(shape1.First.Prev, segmentPool.Get(shape1.First), shape1.First.Next, shape2.First.Prev, segmentPool.Get(shape2.First), shape2.First.Next), Is.True);
			Assert.That(mappings.IsMapped(shape1.Last.Prev, segmentPool.Get(shape1.Last), shape1.Last.Next, shape2.Last, new Ngram<Segment>(), shape2.Last.Next), Is.True);

			shape1 = _segmenter.Segment("hog");
			shape2 = _segmenter.Segment("oc");

			Assert.That(mappings.IsMapped(shape1.First.Prev, segmentPool.Get(shape1.First), shape1.First.Next, shape2.First.Prev, new Ngram<Segment>(), shape2.First), Is.False);
			Assert.That(mappings.IsMapped(shape1.Last.Prev, segmentPool.Get(shape1.Last), shape1.Last.Next, shape2.Last.Prev, segmentPool.Get(shape2.Last), shape2.Last.Next), Is.False);

			shape1 = _segmenter.Segment("swat");
			shape2 = _segmenter.Segment("sat");

			Assert.That(mappings.IsMapped(shape1.ElementAt(0), segmentPool.Get(shape1.ElementAt(1)), shape1.ElementAt(2), shape2.ElementAt(0), new Ngram<Segment>(), shape2.ElementAt(1)), Is.True);

			shape1 = _segmenter.Segment("sawat");
			shape2 = _segmenter.Segment("saat");
			Assert.That(mappings.IsMapped(shape1.ElementAt(1), segmentPool.Get(shape1.ElementAt(2)), shape1.ElementAt(3), shape2.ElementAt(1), new Ngram<Segment>(), shape2.ElementAt(2)), Is.False);
		}

		[Test]
		public void IsMapped_ImplicitComplexSegments()
		{
			var segmentPool = new SegmentPool();

			var mappings = new ListSegmentMappings(_segmenter, new[]
				{
					Tuple.Create("m", "n"),
					Tuple.Create("t", "-"),
					Tuple.Create("h#", "-#"),
					Tuple.Create("c", "#g"),
					Tuple.Create("f", "@")
				}, true);

			Shape shape1 = _segmenter.Segment("s͡mat͡h");
			Shape shape2 = _segmenter.Segment("k͡no");

			Assert.That(mappings.IsMapped(shape1.First.Prev, segmentPool.Get(shape1.First), shape1.First.Next, shape2.First.Prev, segmentPool.Get(shape2.First), shape2.First.Next), Is.True);
			Assert.That(mappings.IsMapped(shape1.Last.Prev, segmentPool.Get(shape1.Last), shape1.Last.Next, shape2.Last, new Ngram<Segment>(), shape2.Last.Next), Is.True);
			Assert.That(mappings.IsMapped(shape1.First, segmentPool.Get(shape1.First.Next), shape1.Last, shape2.First, segmentPool.Get(shape2.Last), shape2.Last.Next), Is.False);

			shape1 = _segmenter.Segment("got͡h");
			shape2 = _segmenter.Segment("c͡lo");

			Assert.That(mappings.IsMapped(shape1.First.Prev, segmentPool.Get(shape1.First), shape1.First.Next, shape2.First.Prev, segmentPool.Get(shape2.First), shape2.First.Next), Is.True);
			Assert.That(mappings.IsMapped(shape1.Last.Prev, segmentPool.Get(shape1.Last), shape1.Last.Next, shape2.Last, new Ngram<Segment>(), shape2.Last.Next), Is.True);

			shape1 = _segmenter.Segment("s͡hog");
			shape2 = _segmenter.Segment("oc͡t");

			Assert.That(mappings.IsMapped(shape1.First.Prev, segmentPool.Get(shape1.First), shape1.First.Next, shape2.First.Prev, new Ngram<Segment>(), shape2.First), Is.False);
			Assert.That(mappings.IsMapped(shape1.Last.Prev, segmentPool.Get(shape1.Last), shape1.Last.Next, shape2.Last.Prev, segmentPool.Get(shape2.Last), shape2.Last.Next), Is.False);
		}

		[Test]
		public void NoMappings()
		{
			var segmentPool = new SegmentPool();

			var mappings = new ListSegmentMappings(_segmenter, new Tuple<string, string>[0], false);

			Shape shape1 = _segmenter.Segment("mat");
			Shape shape2 = _segmenter.Segment("no");

			Assert.That(mappings.IsMapped(shape1.First.Prev, segmentPool.Get(shape1.First), shape1.First.Next, shape2.First.Prev, segmentPool.Get(shape2.First), shape2.First.Next), Is.False);
			Assert.That(mappings.IsMapped(shape1.Last.Prev, segmentPool.Get(shape1.Last), shape1.Last.Next, shape2.Last, new Ngram<Segment>(), shape2.Last.Next), Is.False);
		}
	}
}
