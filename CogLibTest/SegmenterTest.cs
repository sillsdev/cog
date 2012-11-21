using System.Linq;
using NUnit.Framework;
using SIL.Cog;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace CogLibTest
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
		public void ToShape()
		{
			Shape shape;
			Assert.That(_segmenter.ToShape("call", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(4));
			AssertShapeNodeEqual(shape.First, "c", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.ElementAt(1), "a", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(shape.ElementAt(2), "l", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.Last, "l", CogFeatureSystem.ConsonantType);

			Assert.That(_segmenter.ToShape("church", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(4));
			AssertShapeNodeEqual(shape.First, "ch", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.ElementAt(1), "u", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(shape.ElementAt(2), "r", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.Last, "ch", CogFeatureSystem.ConsonantType);

			Assert.That(_segmenter.ToShape("ex-wife", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(7));
			AssertShapeNodeEqual(shape.ElementAt(2), "-", CogFeatureSystem.BoundaryType);

			Assert.That(_segmenter.ToShape("señor", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(5));
			AssertShapeNodeEqual(shape.ElementAt(2), "n", CogFeatureSystem.ConsonantType);
			Assert.That(shape.ElementAt(2).OriginalStrRep(), Is.EqualTo("ñ"));

			Assert.That(_segmenter.ToShape("John", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(4));
			AssertShapeNodeEqual(shape.First, "j", CogFeatureSystem.ConsonantType);
			Assert.That(shape.First.OriginalStrRep(), Is.EqualTo("J"));
		}

		[Test]
		public void ToShapeWithComplexConsonants()
		{
			Shape shape;
			Assert.That(_segmenter.ToShape("cal͡l", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(3));
			AssertShapeNodeEqual(shape.First, "c", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.ElementAt(1), "a", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(shape.Last, "ll", CogFeatureSystem.ConsonantType);

			Assert.That(_segmenter.ToShape("s͡tand", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(4));
			AssertShapeNodeEqual(shape.First, "st", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.ElementAt(1), "a", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(shape.ElementAt(2), "n", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.Last, "d", CogFeatureSystem.ConsonantType);

			_segmenter.MaxConsonantLength = 2;

			Assert.That(_segmenter.ToShape("call", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(3));
			AssertShapeNodeEqual(shape.First, "c", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.ElementAt(1), "a", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(shape.Last, "ll", CogFeatureSystem.ConsonantType);

			Assert.That(_segmenter.ToShape("church", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(3));
			AssertShapeNodeEqual(shape.First, "ch", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.ElementAt(1), "u", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(shape.Last, "rch", CogFeatureSystem.ConsonantType);
		}

		[Test]
		public void ToShapeWithAffixes()
		{
			Shape shape;
			Assert.That(_segmenter.ToShape(null, "call", "ed", out shape), Is.True);
			Assert.That(shape.Count, Is.EqualTo(6));
			AssertShapeNodeEqual(shape.First, "c", CogFeatureSystem.ConsonantType);
			AssertShapeNodeEqual(shape.ElementAt(1), "a", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(shape.ElementAt(4), "e", CogFeatureSystem.VowelType);
			AssertShapeNodeEqual(shape.Last, "d", CogFeatureSystem.ConsonantType);
			Annotation<ShapeNode> stemAnn = shape.Annotations.Single(a => a.Type() == CogFeatureSystem.StemType);
			Assert.That(stemAnn.Span, Is.EqualTo(_spanFactory.Create(shape.First, shape.ElementAt(3))));
			Annotation<ShapeNode> suffixAnn = shape.Annotations.Single(a => a.Type() == CogFeatureSystem.SuffixType);
			Assert.That(suffixAnn.Span, Is.EqualTo(_spanFactory.Create(shape.ElementAt(4), shape.Last)));
		}

		[Test]
		public void InvalidShape()
		{
			Shape shape;
			Assert.That(_segmenter.ToShape("hello@", out shape), Is.False);
			Assert.That(_segmenter.ToShape("!test", out shape), Is.False);
			Assert.That(_segmenter.ToShape("wo.rd", out shape), Is.False);
			Assert.That(_segmenter.ToShape("née", out shape), Is.False);
		}

		private void AssertShapeNodeEqual(ShapeNode actualNode, string expectedStrRep, FeatureSymbol expectedType)
		{
			Assert.That(actualNode.StrRep(), Is.EqualTo(expectedStrRep));
			Assert.That(actualNode.Type(), Is.EqualTo(expectedType));
		}
	}
}
