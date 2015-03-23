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
	public class WordListsVarietyMeaningViewModelTests
	{
		private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();

		[Test]
		public void StrRep()
		{
			DispatcherHelper.Initialize();
			var busyService = Substitute.For<IBusyService>();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var analysisService = Substitute.For<IAnalysisService>();
			var importService = Substitute.For<IImportService>();
			var exportService = Substitute.For<IExportService>();

			var project = new CogProject(_spanFactory);
			project.Meanings.Add(new Meaning("gloss1", "cat1"));
			project.Varieties.Add(new Variety("variety1"));

			WordViewModel.Factory wordFactory = word => new WordViewModel(busyService, analysisService, word);
			WordListsVarietyMeaningViewModel.Factory varietyMeaningFactory = (v, meaning) => new WordListsVarietyMeaningViewModel(busyService, analysisService, wordFactory, v, meaning);
			WordListsVarietyViewModel.Factory varietyFactory = (p, v) => new WordListsVarietyViewModel(projectService, varietyMeaningFactory, p, v);

			projectService.Project.Returns(project);

			var parent = new WordListsViewModel(projectService, dialogService, importService, exportService, analysisService, varietyFactory);
			WordListsVarietyViewModel variety = varietyFactory(parent, project.Varieties[0]);
			WordListsVarietyMeaningViewModel varietyMeaning = variety.Meanings[0];

			Assert.That(varietyMeaning.Words, Is.Empty);
			Assert.That(varietyMeaning.StrRep, Is.Empty);

			project.Varieties[0].Words.Add(new Word("hɛ.loʊ", project.Meanings[0]));

			Assert.That(varietyMeaning.StrRep, Is.EqualTo("hɛ.loʊ"));
			Assert.That(varietyMeaning.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.loʊ"}));

			project.Varieties[0].Words.Add(new Word("gu.gəl", project.Meanings[0]));

			Assert.That(varietyMeaning.StrRep, Is.EqualTo("hɛ.loʊ,gu.gəl"));
			Assert.That(varietyMeaning.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.loʊ", "gu.gəl"}));

			varietyMeaning.StrRep = "hɛ.lp,gu.gəl";
			Assert.That(varietyMeaning.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.lp", "gu.gəl"}));

			varietyMeaning.StrRep = "hɛ.lp";
			Assert.That(varietyMeaning.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.lp"}));

			varietyMeaning.StrRep = "";
			Assert.That(varietyMeaning.Words, Is.Empty);

			varietyMeaning.StrRep = null;
			Assert.That(varietyMeaning.Words, Is.Empty);

			varietyMeaning.StrRep = " hɛ.lp,gu.gəl ,gu.fi ";
			Assert.That(varietyMeaning.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.lp", "gu.gəl", "gu.fi"}));
			Assert.That(varietyMeaning.StrRep, Is.EqualTo("hɛ.lp,gu.gəl,gu.fi"));

			varietyMeaning.StrRep = "hɛ.lp,gu.gəl,gu";
			Assert.That(varietyMeaning.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.lp", "gu.gəl", "gu"}));
		}
	}
}
