using System.Linq;
using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Application.Services;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.Cog.TestUtils;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.Tests.ViewModels
{
	[TestFixture]
	public class WordViewModelTests
	{
		private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();

		[Test]
		public void Segments()
		{
			var segmentPool = new SegmentPool();
			var busyService = Substitute.For<IBusyService>();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var analysisService = new AnalysisService(_spanFactory, segmentPool, projectService, dialogService, busyService);
			var project = TestHelpers.GetTestProject(_spanFactory, segmentPool);
			project.Meanings.Add(new Meaning("gloss1", "cat1"));
			project.Varieties.Add(new Variety("variety1"));
			var w = new Word("gugəl", project.Meanings[0]);
			project.Varieties[0].Words.Add(w);
			projectService.Project.Returns(project);

			var word = new WordViewModel(busyService, analysisService, w);

			Assert.That(word.Segments, Is.Empty);
			Assert.That(word.IsValid, Is.False);

			project.Segmenter.Segment(w);

			Assert.That(word.IsValid, Is.True);
			Assert.That(word.Segments.Select(s => s.StrRep), Is.EqualTo(new[] {"|", "g", "u", "g", "ə", "l", "|"}));

			word.Segments.Move(0, 2);

			Assert.That(word.Segments.Select(s => s.StrRep), Is.EqualTo(new[] {"g", "u", "|", "g", "ə", "l", "|"}));
			Annotation<ShapeNode> prefixAnn = w.Prefix;
			Assert.That(prefixAnn, Is.Not.Null);
			Assert.That(w.Shape.GetNodes(prefixAnn.Span).Select(n => n.OriginalStrRep()), Is.EqualTo(new[] {"g", "u"}));
			Assert.That(w.Shape.GetNodes(w.Stem.Span).Select(n => n.OriginalStrRep()), Is.EqualTo(new[] {"g", "ə", "l"}));
			Assert.That(w.Suffix, Is.Null);
			Assert.That(w.StemIndex, Is.EqualTo(2));
			Assert.That(w.StemLength, Is.EqualTo(3));

			WordSegmentViewModel seg = word.Segments[6];
			word.Segments.RemoveAt(6);
			word.Segments.Insert(5, seg);

			Assert.That(word.Segments.Select(s => s.StrRep), Is.EqualTo(new[] {"g", "u", "|", "g", "ə", "|", "l"}));
			prefixAnn = w.Prefix;
			Assert.That(prefixAnn, Is.Not.Null);
			Assert.That(w.Shape.GetNodes(prefixAnn.Span).Select(n => n.OriginalStrRep()), Is.EqualTo(new[] {"g", "u"}));
			Assert.That(w.Shape.GetNodes(w.Stem.Span).Select(n => n.OriginalStrRep()), Is.EqualTo(new[] {"g", "ə"}));
			Annotation<ShapeNode> suffixAnn = w.Suffix;
			Assert.That(suffixAnn, Is.Not.Null);
			Assert.That(w.Shape.GetNodes(suffixAnn.Span).Select(n => n.OriginalStrRep()), Is.EqualTo(new[] {"l"}));
			Assert.That(w.StemIndex, Is.EqualTo(2));
			Assert.That(w.StemLength, Is.EqualTo(2));
		}
	}
}
