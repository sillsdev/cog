using System.Linq;
using NUnit.Framework;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain.Test
{
	[TestFixture]
	public class SegmenterTest
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
		public void TrySegment()
		{
			Shape shape;
			Assert.That(_segmenter.TrySegment("call", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(4));
			AssertShapeNodeEqual(shape.First, "c", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.ElementAt(1), "a", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(shape.ElementAt(2), "l", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.Last, "l", CogFeatureSystem.ConsonantType);

			Assert.That(_segmenter.TrySegment("church", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(4));
			AssertShapeNodeEqual(shape.First, "ch", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.ElementAt(1), "u", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(shape.ElementAt(2), "r", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.Last, "ch", CogFeatureSystem.ConsonantType);

			Assert.That(_segmenter.TrySegment("ex-wife", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(7));
			AssertShapeNodeEqual(shape.ElementAt(2), "-", CogFeatureSystem.BoundaryType);

			Assert.That(_segmenter.TrySegment("señor", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(5));
			AssertShapeNodeEqual(shape.ElementAt(2), "n", CogFeatureSystem.ConsonantType);
			Assert.That(shape.ElementAt(2).OriginalStrRep(), Is.EqualTo("ñ"));

			Assert.That(_segmenter.TrySegment("John", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(4));
			AssertShapeNodeEqual(shape.First, "j", CogFeatureSystem.ConsonantType);
			Assert.That(shape.First.OriginalStrRep(), Is.EqualTo("J"));
		}

		[Test]
		public void TrySegmentWithComplexConsonants()
		{
			Shape shape;
			Assert.That(_segmenter.TrySegment("cal͡l", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(3));
			AssertShapeNodeEqual(shape.First, "c", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.ElementAt(1), "a", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(shape.Last, "ll", CogFeatureSystem.ConsonantType);

			Assert.That(_segmenter.TrySegment("s͡tand", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(4));
			AssertShapeNodeEqual(shape.First, "st", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.ElementAt(1), "a", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(shape.ElementAt(2), "n", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.Last, "d", CogFeatureSystem.ConsonantType);

			_segmenter.MaxConsonantLength = 2;

			Assert.That(_segmenter.TrySegment("call", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(3));
			AssertShapeNodeEqual(shape.First, "c", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.ElementAt(1), "a", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(shape.Last, "ll", CogFeatureSystem.ConsonantType);

			Assert.That(_segmenter.TrySegment("church", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(3));
			AssertShapeNodeEqual(shape.First, "ch", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.ElementAt(1), "u", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(shape.Last, "rch", CogFeatureSystem.ConsonantType);
		}

		[Test]
		public void SegmentWord()
		{
			var sense = new Sense("gloss", "category");
			var word = new Word("called", 0, 4, sense);

			_segmenter.Segment(word);
			Assert.That(word.Shape.Count, Is.EqualTo(6));
			AssertShapeNodeEqual(word.Shape.First, "c", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(word.Shape.ElementAt(1), "a", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(word.Shape.ElementAt(4), "e", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(word.Shape.Last, "d", CogFeatureSystem.ConsonantType);
			Annotation<ShapeNode> stemAnn = word.Shape.Annotations.Single(a => a.Type() == CogFeatureSystem.StemType);
			Assert.That(stemAnn.Span, Is.EqualTo(_spanFactory.Create(word.Shape.First, word.Shape.ElementAt(3))));
			Annotation<ShapeNode> suffixAnn = word.Shape.Annotations.Single(a => a.Type() == CogFeatureSystem.SuffixType);
			Assert.That(suffixAnn.Span, Is.EqualTo(_spanFactory.Create(word.Shape.ElementAt(4), word.Shape.Last)));
		}

		[Test]
		public void InvalidShape()
		{
			Shape shape;
			Assert.That(_segmenter.TrySegment("hello@", out shape), Is.False);
			Assert.That(_segmenter.TrySegment("!test", out shape), Is.False);
			Assert.That(_segmenter.TrySegment("wo.rd", out shape), Is.False);
			Assert.That(_segmenter.TrySegment("née", out shape), Is.False);
		}

		private void AssertShapeNodeEqual(ShapeNode actualNode, string expectedStrRep, FeatureSymbol expectedType)
		{
			Assert.That(actualNode.StrRep(), Is.EqualTo(expectedStrRep));
			Assert.That(actualNode.Type(), Is.EqualTo(expectedType));
		}
	}
}
