using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Domain.Components;
using SIL.Cog.TestUtils;
using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;

namespace SIL.Cog.Domain.Tests.Components
{
	[TestFixture]
	public class BlairCognateIdentifierTests
	{
		private readonly ShapeSpanFactory _spanFactory = new ShapeSpanFactory();

		[Test]
		public void UpdateCognicity_NoSimilarSegments()
		{
			var segmentPool = new SegmentPool();

			CogProject project = TestHelpers.GetTestProject(_spanFactory, segmentPool);
			project.Meanings.AddRange(new[] {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3")});
			project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2")});
			project.Varieties[0].Words.AddRange(new[] {new Word("hɛ.loʊ", project.Meanings[0]), new Word("gʊd", project.Meanings[1]), new Word("bæ", project.Meanings[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("hɛ.ɬa", project.Meanings[0]), new Word("gud", project.Meanings[1]), new Word("pæ", project.Meanings[2])});

			var varSegementer = new VarietySegmenter(project.Segmenter);
			foreach (Variety variety in project.Varieties)
				varSegementer.Process(variety);

			var vp = new VarietyPair(project.Varieties[0], project.Varieties[1]);
			project.VarietyPairs.Add(vp);

			var wordPairGenerator = new SimpleWordPairGenerator(segmentPool, project, 0.3, "primary");
			wordPairGenerator.Process(vp);

			var aligner = new TestWordAligner(segmentPool);
			var ignoredMappings = Substitute.For<ISegmentMappings>();
			var similarSegmentsMappings = Substitute.For<ISegmentMappings>();
			var cognateIdentifier = new BlairCognateIdentifier(segmentPool, false, false, ignoredMappings, similarSegmentsMappings);
			var wp = vp.WordPairs[0];
			cognateIdentifier.UpdateCognicity(wp, aligner.Compute(wp));
			Assert.That(wp.AreCognatePredicted, Is.False);

			wp = vp.WordPairs[1];
			cognateIdentifier.UpdateCognicity(wp, aligner.Compute(wp));
			Assert.That(wp.AreCognatePredicted, Is.True);

			wp = vp.WordPairs[2];
			cognateIdentifier.UpdateCognicity(wp, aligner.Compute(wp));
			Assert.That(wp.AreCognatePredicted, Is.False);
		}

		[Test]
		public void UpdateCognicity_SimilarSegments()
		{
			var segmentPool = new SegmentPool();

			CogProject project = TestHelpers.GetTestProject(_spanFactory, segmentPool);
			project.Meanings.AddRange(new[] {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3")});
			project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2")});
			project.Varieties[0].Words.AddRange(new[] {new Word("hɛ.loʊ", project.Meanings[0]), new Word("gʊd", project.Meanings[1]), new Word("bæ", project.Meanings[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("hɛ.ɬa", project.Meanings[0]), new Word("gud", project.Meanings[1]), new Word("pæ", project.Meanings[2])});

			var varSegementer = new VarietySegmenter(project.Segmenter);
			foreach (Variety variety in project.Varieties)
				varSegementer.Process(variety);

			var vp = new VarietyPair(project.Varieties[0], project.Varieties[1]);
			project.VarietyPairs.Add(vp);

			var wordPairGenerator = new SimpleWordPairGenerator(segmentPool, project, 0.3, "primary");
			wordPairGenerator.Process(vp);

			var aligner = new TestWordAligner(segmentPool);
			var ignoredMappings = Substitute.For<ISegmentMappings>();
			var similarSegmentsMappings = Substitute.For<ISegmentMappings>();
			similarSegmentsMappings.IsMapped(Arg.Any<ShapeNode>(), Arg.Any<Ngram<Segment>>(), Arg.Any<ShapeNode>(), Arg.Any<ShapeNode>(), Arg.Any<Ngram<Segment>>(), Arg.Any<ShapeNode>()).Returns(true);
			var cognateIdentifier = new BlairCognateIdentifier(segmentPool, false, false, ignoredMappings, similarSegmentsMappings);
			var wp = vp.WordPairs[0];
			cognateIdentifier.UpdateCognicity(wp, aligner.Compute(wp));
			Assert.That(wp.AreCognatePredicted, Is.True);

			wp = vp.WordPairs[1];
			cognateIdentifier.UpdateCognicity(wp, aligner.Compute(wp));
			Assert.That(wp.AreCognatePredicted, Is.True);

			wp = vp.WordPairs[2];
			cognateIdentifier.UpdateCognicity(wp, aligner.Compute(wp));
			Assert.That(wp.AreCognatePredicted, Is.True);
		}

		[Test]
		public void UpdateCognicity_RegularCorrespondences()
		{
			var segmentPool = new SegmentPool();

			CogProject project = TestHelpers.GetTestProject(_spanFactory, segmentPool);
			project.Meanings.AddRange(new[] {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3")});
			project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2")});
			project.Varieties[0].Words.AddRange(new[] {new Word("hɛ.lo", project.Meanings[0]), new Word("gʊd", project.Meanings[1]), new Word("bæ", project.Meanings[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("hɛ.ɬa", project.Meanings[0]), new Word("gud", project.Meanings[1]), new Word("pæ", project.Meanings[2])});

			var varSegementer = new VarietySegmenter(project.Segmenter);
			foreach (Variety variety in project.Varieties)
				varSegementer.Process(variety);

			var vp = new VarietyPair(project.Varieties[0], project.Varieties[1]);
			project.VarietyPairs.Add(vp);

			var wordPairGenerator = new SimpleWordPairGenerator(segmentPool, project, 0.3, "primary");
			wordPairGenerator.Process(vp);

			vp.SoundChangeFrequencyDistribution[new SoundContext(segmentPool.GetExisting("l"))].Increment(segmentPool.GetExisting("ɬ"), 3);
			vp.SoundChangeFrequencyDistribution[new SoundContext(segmentPool.GetExisting("b"))].Increment(segmentPool.GetExisting("p"), 3);

			var aligner = new TestWordAligner(segmentPool);
			var ignoredMappings = Substitute.For<ISegmentMappings>();
			ignoredMappings.IsMapped(Arg.Any<ShapeNode>(), Arg.Any<Ngram<Segment>>(), Arg.Any<ShapeNode>(), Arg.Any<ShapeNode>(), Arg.Any<Ngram<Segment>>(), Arg.Any<ShapeNode>()).Returns(false);
			var similarSegmentsMappings = Substitute.For<ISegmentMappings>();
			similarSegmentsMappings.IsMapped(Arg.Any<ShapeNode>(), segmentPool.GetExisting("b"), Arg.Any<ShapeNode>(), Arg.Any<ShapeNode>(), segmentPool.GetExisting("p"), Arg.Any<ShapeNode>()).Returns(true);
			var cognateIdentifier = new BlairCognateIdentifier(segmentPool, false, false, ignoredMappings, similarSegmentsMappings);
			var wp = vp.WordPairs[0];
			cognateIdentifier.UpdateCognicity(wp, aligner.Compute(wp));
			Assert.That(wp.AreCognatePredicted, Is.True);

			wp = vp.WordPairs[1];
			cognateIdentifier.UpdateCognicity(wp, aligner.Compute(wp));
			Assert.That(wp.AreCognatePredicted, Is.True);

			wp = vp.WordPairs[2];
			cognateIdentifier.UpdateCognicity(wp, aligner.Compute(wp));
			Assert.That(wp.AreCognatePredicted, Is.True);
		}
	}
}
