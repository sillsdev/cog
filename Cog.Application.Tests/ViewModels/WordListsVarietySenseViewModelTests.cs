using System.Linq;
using GalaSoft.MvvmLight.Threading;
using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Application.Services;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.Tests.ViewModels
{
	[TestFixture]
	public class WordListsVarietySenseViewModelTests
	{
		private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();

		[Test]
		public void StrRep()
		{
			DispatcherHelper.Initialize();
			var busyService = Substitute.For<IBusyService>();
			var projectService = Substitute.For<IProjectService>();
			var analysisService = Substitute.For<IAnalysisService>();

			var project = new CogProject(_spanFactory);
			project.Senses.Add(new Sense("sense1", "cat1"));
			project.Varieties.Add(new Variety("variety1"));

			WordViewModel.Factory wordFactory = word => new WordViewModel(busyService, analysisService, word);
			WordListsVarietySenseViewModel.Factory varietySenseFactory = (v, sense) => new WordListsVarietySenseViewModel(busyService, analysisService, wordFactory, v, sense);

			projectService.Project.Returns(project);

			var variety = new WordListsVarietyViewModel(projectService, varietySenseFactory, project.Varieties[0]);
			WordListsVarietySenseViewModel varietySense = variety.Senses[0];

			Assert.That(varietySense.Words, Is.Empty);
			Assert.That(varietySense.StrRep, Is.Empty);

			project.Varieties[0].Words.Add(new Word("hɛ.loʊ", project.Senses[0]));

			Assert.That(varietySense.StrRep, Is.EqualTo("hɛ.loʊ"));
			Assert.That(varietySense.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.loʊ"}));

			project.Varieties[0].Words.Add(new Word("gu.gəl", project.Senses[0]));

			Assert.That(varietySense.StrRep, Is.EqualTo("hɛ.loʊ,gu.gəl"));
			Assert.That(varietySense.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.loʊ", "gu.gəl"}));

			varietySense.StrRep = "hɛ.lp,gu.gəl";
			Assert.That(varietySense.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.lp", "gu.gəl"}));

			varietySense.StrRep = "hɛ.lp";
			Assert.That(varietySense.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.lp"}));

			varietySense.StrRep = "";
			Assert.That(varietySense.Words, Is.Empty);

			varietySense.StrRep = null;
			Assert.That(varietySense.Words, Is.Empty);

			varietySense.StrRep = " hɛ.lp,gu.gəl ,gu.fi ";
			Assert.That(varietySense.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.lp", "gu.gəl", "gu.fi"}));
			Assert.That(varietySense.StrRep, Is.EqualTo("hɛ.lp,gu.gəl,gu.fi"));

			varietySense.StrRep = "hɛ.lp,gu.gəl,gu";
			Assert.That(varietySense.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.lp", "gu.gəl", "gu"}));
		}
	}
}
