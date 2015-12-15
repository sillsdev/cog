using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Domain.Components;
using SIL.Cog.TestUtils;
using SIL.Machine.Annotations;

namespace SIL.Cog.Domain.Tests.Components
{
	[TestFixture]
	public class CognacyWordPairGeneratorTests
	{
		private readonly ShapeSpanFactory _spanFactory = new ShapeSpanFactory();

		[Test]
		public void Process()
		{
			var segmentPool = new SegmentPool();
			CogProject project = TestHelpers.GetTestProject(_spanFactory, segmentPool);
			project.Meanings.AddRange(new[] {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3"), new Meaning("gloss4", "cat4")});
			project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2")});
			project.Varieties[0].Words.AddRange(new[]
			{
				new Word("hɛ.loʊ", project.Meanings[0]), new Word("gan", project.Meanings[0]),
				new Word("gʊd", project.Meanings[1]),
				new Word("bæ", project.Meanings[2]), new Word("ban", project.Meanings[2]),
				new Word("had", project.Meanings[3])
			});
			project.Varieties[1].Words.AddRange(new[]
			{
				new Word("hɛ.ɬa", project.Meanings[0]),
				new Word("gud", project.Meanings[1]), new Word("tan", project.Meanings[1]),
				new Word("pæ", project.Meanings[2]),
				new Word("ban", project.Meanings[3]) 
			});
			project.WordAligners["primary"] = new TestWordAligner(segmentPool);
			var cognateIdentifier = Substitute.For<ICognateIdentifier>();
			cognateIdentifier.When(ci => ci.UpdateCognacy(Arg.Any<WordPair>(), Arg.Any<IWordAlignerResult>())).Do(ci =>
				{
					var wordPair = ci.Arg<WordPair>();
					if ((wordPair.Word1.StrRep == "hɛ.loʊ" && wordPair.Word2.StrRep == "hɛ.ɬa")
					    || (wordPair.Word1.StrRep == "gʊd" && wordPair.Word2.StrRep == "tan")
					    || (wordPair.Word1.StrRep == "bæ" && wordPair.Word2.StrRep == "pæ")
					    || (wordPair.Word1.StrRep == "ban" && wordPair.Word2.StrRep == "pæ"))
					{
						wordPair.PredictedCognacy = true;
						wordPair.PredictedCognacyScore = 1.0;
					}
				});
			project.CognateIdentifiers["primary"] = cognateIdentifier;

			var varSegementer = new VarietySegmenter(project.Segmenter);
			foreach (Variety variety in project.Varieties)
				varSegementer.Process(variety);

			var vp = new VarietyPair(project.Varieties[0], project.Varieties[1]);
			project.VarietyPairs.Add(vp);

			var wordPairGenerator = new CognacyWordPairGenerator(segmentPool, project, 0.3, "primary", "primary");
			wordPairGenerator.Process(vp);

			WordPair wp = vp.WordPairs[0];
			Assert.That(wp.Word1.StrRep, Is.EqualTo("had"));
			Assert.That(wp.Word2.StrRep, Is.EqualTo("ban"));

			wp = vp.WordPairs[1];
			Assert.That(wp.Word1.StrRep, Is.EqualTo("hɛ.loʊ"));
			Assert.That(wp.Word2.StrRep, Is.EqualTo("hɛ.ɬa"));

			wp = vp.WordPairs[2];
			Assert.That(wp.Word1.StrRep, Is.EqualTo("gʊd"));
			Assert.That(wp.Word2.StrRep, Is.EqualTo("tan"));

			wp = vp.WordPairs[3];
			Assert.That(wp.Word1.StrRep, Is.EqualTo("bæ"));
			Assert.That(wp.Word2.StrRep, Is.EqualTo("pæ"));

			Assert.That(vp.AllSoundCorrespondenceFrequencyDistribution[new SoundContext(segmentPool.GetExisting("d"))][segmentPool.GetExisting("n")], Is.EqualTo(2));
			Assert.That(vp.AllSoundCorrespondenceFrequencyDistribution[new SoundContext(segmentPool.GetExisting("h"))][segmentPool.GetExisting("h")], Is.EqualTo(1));
			Assert.That(vp.AllSoundCorrespondenceFrequencyDistribution[new SoundContext(segmentPool.GetExisting("h"))][segmentPool.GetExisting("b")], Is.EqualTo(1));

			Assert.That(vp.CognateSoundCorrespondenceFrequencyDistribution[new SoundContext(segmentPool.GetExisting("d"))][segmentPool.GetExisting("n")], Is.EqualTo(1));
			Assert.That(vp.CognateSoundCorrespondenceFrequencyDistribution[new SoundContext(segmentPool.GetExisting("h"))][segmentPool.GetExisting("h")], Is.EqualTo(0));
			Assert.That(vp.CognateSoundCorrespondenceFrequencyDistribution[new SoundContext(segmentPool.GetExisting("h"))][segmentPool.GetExisting("b")], Is.EqualTo(1));
		}
	}
}
