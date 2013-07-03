using System.Linq;
using NUnit.Framework;
using SIL.Cog.Components;
using SIL.Cog.NgramModeling;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.Test
{
	[TestFixture]
	public class NgramModelTest
	{
		private SpanFactory<ShapeNode> _spanFactory;
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

			var segDistCalc = new SegmentDistributionCalculator();
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
			var model = NgramModel.Train(2, _variety, new MaxLikelihoodSmoother());
			Assert.That(model.GetProbability(_variety.SegmentPool.GetExisting("l"), _variety.SegmentPool.GetExisting("a")), Is.EqualTo(0.666).Within(0.001));
			Assert.That(model.GetProbability(Segment.Anchor, _variety.SegmentPool.GetExisting("a")), Is.EqualTo(0.333).Within(0.001));
			Assert.That(model.GetProbability(_variety.SegmentPool.GetExisting("a"), _variety.SegmentPool.GetExisting("a")), Is.EqualTo(0.0));

			Assert.That(model.GetProbability(_variety.SegmentPool.GetExisting("l"), _variety.SegmentPool.GetExisting("l")), Is.EqualTo(0.5));
			Assert.That(model.GetProbability(_variety.SegmentPool.GetExisting("o"), _variety.SegmentPool.GetExisting("l")), Is.EqualTo(0.166).Within(0.001));
			Assert.That(model.GetProbability(Segment.Anchor, _variety.SegmentPool.GetExisting("l")), Is.EqualTo(0.333).Within(0.001));
			Assert.That(model.GetProbability(_variety.SegmentPool.GetExisting("a"), _variety.SegmentPool.GetExisting("l")), Is.EqualTo(0.0));

			model = NgramModel.Train(3, _variety, new MaxLikelihoodSmoother());
			Assert.That(model.GetProbability(_variety.SegmentPool.GetExisting("l"), new Ngram(_variety.SegmentPool.GetExisting("a"), _variety.SegmentPool.GetExisting("t"))), Is.EqualTo(0.0));

			Assert.That(model.GetProbability(_variety.SegmentPool.GetExisting("l"), new Ngram(_variety.SegmentPool.GetExisting("a"), _variety.SegmentPool.GetExisting("l"))), Is.EqualTo(1.0));
			Assert.That(model.GetProbability(_variety.SegmentPool.GetExisting("t"), new Ngram(_variety.SegmentPool.GetExisting("a"), _variety.SegmentPool.GetExisting("l"))), Is.EqualTo(0.0));
		}

		[Test]
		public void Ngrams()
		{
			NgramModel[] models = NgramModel.TrainAll(10, _variety, () => new MaxLikelihoodSmoother()).ToArray();
			Assert.That(models[0].Ngrams.Count, Is.EqualTo(17));
			Assert.That(models[1].Ngrams.Count, Is.EqualTo(37));
			Assert.That(models[7].Ngrams.Count, Is.EqualTo(5));
			Assert.That(models[8].Ngrams.Count, Is.EqualTo(3));
			Assert.That(models[9].Ngrams.Count, Is.EqualTo(2));
		}

		[Test]
		public void GetProbabilityRightToLeft()
		{
			var model = NgramModel.Train(2, _variety, Direction.RightToLeft, new MaxLikelihoodSmoother());
			Assert.That(model.GetProbability(_variety.SegmentPool.GetExisting("a"), _variety.SegmentPool.GetExisting("l")), Is.EqualTo(0.333).Within(0.001));
			Assert.That(model.GetProbability(_variety.SegmentPool.GetExisting("l"), _variety.SegmentPool.GetExisting("l")), Is.EqualTo(0.5));
			Assert.That(model.GetProbability(_variety.SegmentPool.GetExisting("e"), _variety.SegmentPool.GetExisting("l")), Is.EqualTo(0.166).Within(0.001));
			Assert.That(model.GetProbability(_variety.SegmentPool.GetExisting("t"), _variety.SegmentPool.GetExisting("l")), Is.EqualTo(0.0));

			Assert.That(model.GetProbability(_variety.SegmentPool.GetExisting("c"), _variety.SegmentPool.GetExisting("a")), Is.EqualTo(0.333).Within(0.001));
			Assert.That(model.GetProbability(_variety.SegmentPool.GetExisting("t"), _variety.SegmentPool.GetExisting("a")), Is.EqualTo(0.333).Within(0.001));
			Assert.That(model.GetProbability(Segment.Anchor, _variety.SegmentPool.GetExisting("a")), Is.EqualTo(0.333).Within(0.001));
			Assert.That(model.GetProbability(_variety.SegmentPool.GetExisting("l"), _variety.SegmentPool.GetExisting("a")), Is.EqualTo(0.0));
		}
	}
}
