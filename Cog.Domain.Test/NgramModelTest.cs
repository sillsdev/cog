using System.Linq;
using NUnit.Framework;
using SIL.Cog.Domain.Components;
using SIL.Cog.Domain.NgramModeling;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.Domain.Test
{
	[TestFixture]
	public class NgramModelTest
	{
		private SpanFactory<ShapeNode> _spanFactory;
		private SegmentPool _segmentPool;
		private Segmenter _segmenter;
		private Variety _variety;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			_spanFactory = new ShapeSpanFactory();
			_segmenter = new Segmenter(_spanFactory)
				{
					Consonants = {"b", "c", "ch", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "sh", "t", "th", "v", "w", "x", "z"},
					Vowels = {"a", "e", "i", "o", "u"},
					Boundaries = {"-"},
					Modifiers = {"\u0303", "\u0308"},
					Joiners = {"\u0361"},
				};

			_variety = new Variety("test");
			AddWord("call");
			AddWord("stall");
			AddWord("hello");
			AddWord("the");
			AddWord("a");
			AddWord("test");
			AddWord("income");
			AddWord("unproduce");

			_segmentPool = new SegmentPool();
			var segDistCalc = new SegmentFrequencyDistributionCalculator(_segmentPool);
			segDistCalc.Process(_variety);
		}

		private void AddWord(string str)
		{
			var word = new Word(str, new Sense(str, null));
			_segmenter.Segment(word);
			_variety.Words.Add(word);
		}

		[Test]
		public void GetProbability()
		{
			var model = NgramModel.Train(_segmentPool, 2, _variety, new MaxLikelihoodSmoother());
			Assert.That(model.GetProbability(_segmentPool.GetExisting("l"), _segmentPool.GetExisting("a")), Is.EqualTo(0.666).Within(0.001));
			Assert.That(model.GetProbability(Segment.Anchor, _segmentPool.GetExisting("a")), Is.EqualTo(0.333).Within(0.001));
			Assert.That(model.GetProbability(_segmentPool.GetExisting("a"), _segmentPool.GetExisting("a")), Is.EqualTo(0.0));

			Assert.That(model.GetProbability(_segmentPool.GetExisting("l"), _segmentPool.GetExisting("l")), Is.EqualTo(0.5));
			Assert.That(model.GetProbability(_segmentPool.GetExisting("o"), _segmentPool.GetExisting("l")), Is.EqualTo(0.166).Within(0.001));
			Assert.That(model.GetProbability(Segment.Anchor, _segmentPool.GetExisting("l")), Is.EqualTo(0.333).Within(0.001));
			Assert.That(model.GetProbability(_segmentPool.GetExisting("a"), _segmentPool.GetExisting("l")), Is.EqualTo(0.0));

			model = NgramModel.Train(_segmentPool, 3, _variety, new MaxLikelihoodSmoother());
			Assert.That(model.GetProbability(_segmentPool.GetExisting("l"), new Ngram(_segmentPool.GetExisting("a"), _segmentPool.GetExisting("t"))), Is.EqualTo(0.0));

			Assert.That(model.GetProbability(_segmentPool.GetExisting("l"), new Ngram(_segmentPool.GetExisting("a"), _segmentPool.GetExisting("l"))), Is.EqualTo(1.0));
			Assert.That(model.GetProbability(_segmentPool.GetExisting("t"), new Ngram(_segmentPool.GetExisting("a"), _segmentPool.GetExisting("l"))), Is.EqualTo(0.0));
		}

		[Test]
		public void Ngrams()
		{
			NgramModel[] models = NgramModel.TrainAll(_segmentPool, 10, _variety, () => new MaxLikelihoodSmoother()).ToArray();
			Assert.That(models[0].Ngrams.Count, Is.EqualTo(17));
			Assert.That(models[1].Ngrams.Count, Is.EqualTo(37));
			Assert.That(models[7].Ngrams.Count, Is.EqualTo(5));
			Assert.That(models[8].Ngrams.Count, Is.EqualTo(3));
			Assert.That(models[9].Ngrams.Count, Is.EqualTo(2));
		}

		[Test]
		public void GetProbabilityRightToLeft()
		{
			var model = NgramModel.Train(_segmentPool, 2, _variety, Direction.RightToLeft, new MaxLikelihoodSmoother());
			Assert.That(model.GetProbability(_segmentPool.GetExisting("a"), _segmentPool.GetExisting("l")), Is.EqualTo(0.333).Within(0.001));
			Assert.That(model.GetProbability(_segmentPool.GetExisting("l"), _segmentPool.GetExisting("l")), Is.EqualTo(0.5));
			Assert.That(model.GetProbability(_segmentPool.GetExisting("e"), _segmentPool.GetExisting("l")), Is.EqualTo(0.166).Within(0.001));
			Assert.That(model.GetProbability(_segmentPool.GetExisting("t"), _segmentPool.GetExisting("l")), Is.EqualTo(0.0));

			Assert.That(model.GetProbability(_segmentPool.GetExisting("c"), _segmentPool.GetExisting("a")), Is.EqualTo(0.333).Within(0.001));
			Assert.That(model.GetProbability(_segmentPool.GetExisting("t"), _segmentPool.GetExisting("a")), Is.EqualTo(0.333).Within(0.001));
			Assert.That(model.GetProbability(Segment.Anchor, _segmentPool.GetExisting("a")), Is.EqualTo(0.333).Within(0.001));
			Assert.That(model.GetProbability(_segmentPool.GetExisting("l"), _segmentPool.GetExisting("a")), Is.EqualTo(0.0));
		}
	}
}
