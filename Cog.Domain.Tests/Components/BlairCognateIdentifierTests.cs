using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Domain.Components;
using SIL.Cog.TestUtils;
using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;
using SIL.Machine.Statistics;

namespace SIL.Cog.Domain.Tests.Components
{
	[TestFixture]
	public class BlairCognateIdentifierTests
	{
		private class TestEnvironment
		{
			private readonly ShapeSpanFactory _spanFactory = new ShapeSpanFactory();
			private readonly CogProject _project;
			private readonly BlairCognateIdentifier _cognateIdentifier;
			private readonly TestWordAligner _aligner;
			private readonly SegmentPool _segmentPool;

			public TestEnvironment(string word1, string word2, bool ignoreRegularInsertionDeletion = false, bool regularConsEqual = false, bool automaticRegularCorrThreshold = false)
			{
				_segmentPool = new SegmentPool();
				_project = TestHelpers.GetTestProject(_spanFactory, _segmentPool);
				_project.Meanings.Add(new Meaning("gloss1", "cat1"));
				_project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2")});
				_project.Varieties[0].Words.Add(new Word(word1, _project.Meanings[0]));
				_project.Varieties[1].Words.Add(new Word(word2, _project.Meanings[0]));

				var varSegementer = new VarietySegmenter(_project.Segmenter);
				foreach (Variety variety in _project.Varieties)
					varSegementer.Process(variety);

				var vp = new VarietyPair(_project.Varieties[0], _project.Varieties[1]);
				_project.VarietyPairs.Add(vp);

				var wordPairGenerator = new SimpleWordPairGenerator(_segmentPool, _project, 0.3, "primary");
				wordPairGenerator.Process(vp);
				vp.AllSoundCorrespondenceFrequencyDistribution = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();

				var ignoredMappings = Substitute.For<ISegmentMappings>();
				var similarSegmentsMappings = Substitute.For<ISegmentMappings>();
				_cognateIdentifier = new BlairCognateIdentifier(_segmentPool, ignoreRegularInsertionDeletion, regularConsEqual, automaticRegularCorrThreshold,
					3, ignoredMappings, similarSegmentsMappings);

				_aligner = new TestWordAligner(_segmentPool);
			}

			public void UpdateCognacy()
			{
				_cognateIdentifier.UpdateCognacy(WordPair, _aligner.Compute(WordPair));
			}

			public BlairCognateIdentifier CognateIdentifier
			{
				get { return _cognateIdentifier; }
			}

			public WordPair WordPair
			{
				get { return VarietyPair.WordPairs[0]; }
			}

			public VarietyPair VarietyPair
			{
				get { return _project.VarietyPairs[0]; }
			}

			public SegmentPool SegmentPool
			{
				get { return _segmentPool; }
			}
		}

		[Test]
		public void UpdateCognacy_NoSimilarSegments()
		{
			var env = new TestEnvironment("hɛ.lo", "he.ɬa");
			env.UpdateCognacy();
			Assert.That(env.WordPair.PredictedCognacy, Is.False);
			Assert.That(env.WordPair.AlignmentNotes, Is.EqualTo(new[] {"1", "2", "3", "2"}));
		}

		[Test]
		public void UpdateCognacy_SimilarNotRegularConsonant()
		{
			var env = new TestEnvironment("hɛ.lo", "he.ɬa");
			env.CognateIdentifier.SimilarSegments.IsMapped(Arg.Any<ShapeNode>(), env.SegmentPool.GetExisting("l"), Arg.Any<ShapeNode>(), Arg.Any<ShapeNode>(),
				env.SegmentPool.GetExisting("ɬ"), Arg.Any<ShapeNode>()).Returns(true);
			env.VarietyPair.AllSoundCorrespondenceFrequencyDistribution[new SoundContext(env.SegmentPool.GetExisting("l"))].Increment(env.SegmentPool.GetExisting("ɬ"), 2);
			env.UpdateCognacy();
			Assert.That(env.WordPair.PredictedCognacy, Is.False);
			Assert.That(env.WordPair.AlignmentNotes, Is.EqualTo(new[] {"1", "2", "2", "2"}));
		}

		[Test]
		public void UpdateCognacy_SimilarRegularConsonant()
		{
			var env = new TestEnvironment("hɛ.lo", "he.ɬa");
			env.CognateIdentifier.SimilarSegments.IsMapped(Arg.Any<ShapeNode>(), env.SegmentPool.GetExisting("l"), Arg.Any<ShapeNode>(), Arg.Any<ShapeNode>(),
				env.SegmentPool.GetExisting("ɬ"), Arg.Any<ShapeNode>()).Returns(true);
			env.VarietyPair.AllSoundCorrespondenceFrequencyDistribution[new SoundContext(env.SegmentPool.GetExisting("l"))].Increment(env.SegmentPool.GetExisting("ɬ"), 3);
			env.UpdateCognacy();
			Assert.That(env.WordPair.PredictedCognacy, Is.True);
			Assert.That(env.WordPair.AlignmentNotes, Is.EqualTo(new[] {"1", "2", "1", "2"}));
		}

		[Test]
		public void UpdateCognacy_SimilarVowel()
		{
			var env = new TestEnvironment("hɛ.lo", "he.ɬa");
			env.CognateIdentifier.SimilarSegments.IsMapped(Arg.Any<ShapeNode>(), env.SegmentPool.GetExisting("o"), Arg.Any<ShapeNode>(), Arg.Any<ShapeNode>(),
				env.SegmentPool.GetExisting("a"), Arg.Any<ShapeNode>()).Returns(true);
			env.UpdateCognacy();
			Assert.That(env.WordPair.PredictedCognacy, Is.True);
			Assert.That(env.WordPair.AlignmentNotes, Is.EqualTo(new[] {"1", "2", "3", "1"}));
		}

		[Test]
		public void UpdateCognacy_IgnoreRegularInsertionDeletion()
		{
			var env = new TestEnvironment("hɛ.lo", "he.l", true);
			env.VarietyPair.AllSoundCorrespondenceFrequencyDistribution[new SoundContext(env.SegmentPool.GetExisting("o"))].Increment(new Ngram<Segment>(), 3);
			env.UpdateCognacy();
			Assert.That(env.WordPair.PredictedCognacy, Is.True);
			Assert.That(env.WordPair.AlignmentNotes, Is.EqualTo(new[] {"1", "2", "1", "-"}));
		}

		[Test]
		public void UpdateCognacy_RegularConsonantEqual()
		{
			var env = new TestEnvironment("hɛ.lo", "he.ɬa", regularConsEqual: true);
			env.VarietyPair.AllSoundCorrespondenceFrequencyDistribution[new SoundContext(env.SegmentPool.GetExisting("l"))].Increment(env.SegmentPool.GetExisting("ɬ"), 3);
			env.UpdateCognacy();
			Assert.That(env.WordPair.PredictedCognacy, Is.True);
			Assert.That(env.WordPair.AlignmentNotes, Is.EqualTo(new[] {"1", "2", "1", "2"}));
		}

		[Test]
		public void UpdateCognacy_AutomaticRegularCorrespondenceThreshold()
		{
			var env = new TestEnvironment("hɛ.lo", "he.ɬa", automaticRegularCorrThreshold: true);
			env.CognateIdentifier.SimilarSegments.IsMapped(Arg.Any<ShapeNode>(), env.SegmentPool.GetExisting("l"), Arg.Any<ShapeNode>(), Arg.Any<ShapeNode>(),
				env.SegmentPool.GetExisting("ɬ"), Arg.Any<ShapeNode>()).Returns(true);
			env.VarietyPair.AllSoundCorrespondenceFrequencyDistribution[new SoundContext(env.SegmentPool.GetExisting("l"))].Increment(env.SegmentPool.GetExisting("ɬ"), 3);
			env.UpdateCognacy();
			Assert.That(env.WordPair.PredictedCognacy, Is.True);
			Assert.That(env.WordPair.AlignmentNotes, Is.EqualTo(new[] {"1", "2", "1", "2"}));
		}
	}
}
