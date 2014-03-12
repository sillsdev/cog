using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Domain.Components;
using SIL.Cog.TestUtils;
using SIL.Machine.Annotations;

namespace SIL.Cog.Domain.Tests.Components
{
	[TestFixture]
	public class CognicityWordPairGeneratorTests
	{
		private readonly ShapeSpanFactory _spanFactory = new ShapeSpanFactory();

		[Test]
		public void Process()
		{
			var segmentPool = new SegmentPool();
			CogProject project = TestHelpers.GetTestProject(_spanFactory, segmentPool);
			project.Senses.AddRange(new[] {new Sense("sense1", "cat1"), new Sense("sense2", "cat2"), new Sense("sense3", "cat3")});
			project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2")});
			project.Varieties[0].Words.AddRange(new[] {new Word("hɛ.loʊ", project.Senses[0]), new Word("gan", project.Senses[0]), new Word("gʊd", project.Senses[1]), new Word("bæ", project.Senses[2]), new Word("ban", project.Senses[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("hɛ.ɬa", project.Senses[0]), new Word("gud", project.Senses[1]), new Word("tan", project.Senses[1]), new Word("pæ", project.Senses[2])});
			project.WordAligners["primary"] = new TestWordAligner(segmentPool);
			var cognateIdentifier = Substitute.For<ICognateIdentifier>();
			cognateIdentifier.When(ci => ci.UpdateCognicity(Arg.Any<WordPair>(), Arg.Any<IWordAlignerResult>())).Do(ci =>
				{
					var wordPair = ci.Arg<WordPair>();
					if ((wordPair.Word1.StrRep == "hɛ.loʊ" && wordPair.Word2.StrRep == "hɛ.ɬa")
					    || (wordPair.Word1.StrRep == "gʊd" && wordPair.Word2.StrRep == "tan")
					    || (wordPair.Word1.StrRep == "bæ" && wordPair.Word2.StrRep == "pæ")
					    || (wordPair.Word1.StrRep == "ban" && wordPair.Word2.StrRep == "pæ"))
					{
						wordPair.AreCognatePredicted = true;
						wordPair.CognicityScore = 1.0;
					}
				});
			project.CognateIdentifiers["primary"] = cognateIdentifier;

			var varSegementer = new VarietySegmenter(project.Segmenter);
			foreach (Variety variety in project.Varieties)
				varSegementer.Process(variety);

			var vp = new VarietyPair(project.Varieties[0], project.Varieties[1]);
			project.VarietyPairs.Add(vp);

			var wordPairGenerator = new CognicityWordPairGenerator(segmentPool, project, 0.3, "primary", "primary");
			wordPairGenerator.Process(vp);

			WordPair wp = vp.WordPairs[0];
			Assert.That(wp.Word1.StrRep, Is.EqualTo("hɛ.loʊ"));
			Assert.That(wp.Word2.StrRep, Is.EqualTo("hɛ.ɬa"));

			wp = vp.WordPairs[1];
			Assert.That(wp.Word1.StrRep, Is.EqualTo("gʊd"));
			Assert.That(wp.Word2.StrRep, Is.EqualTo("tan"));

			wp = vp.WordPairs[2];
			Assert.That(wp.Word1.StrRep, Is.EqualTo("bæ"));
			Assert.That(wp.Word2.StrRep, Is.EqualTo("pæ"));
		}
	}
}
