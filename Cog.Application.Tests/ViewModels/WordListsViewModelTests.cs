using System;
using System.Windows.Data;
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
	public class WordListsViewModelTests
	{
		private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();

		[Test]
		public void Varieties()
		{
			DispatcherHelper.Initialize();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			var analysisService = Substitute.For<IAnalysisService>();
			var importService = Substitute.For<IImportService>();
			var exportService = Substitute.For<IExportService>();

			WordViewModel.Factory wordFactory = word => new WordViewModel(busyService, analysisService, word);
			WordListsVarietySenseViewModel.Factory varietySenseFactory = (variety, sense) => new WordListsVarietySenseViewModel(busyService, analysisService, wordFactory, variety, sense);
			WordListsVarietyViewModel.Factory varietyFactory = variety => new WordListsVarietyViewModel(projectService, varietySenseFactory, variety);

			var wordLists = new WordListsViewModel(projectService, dialogService, importService, exportService, analysisService, varietyFactory);

			var project = new CogProject(_spanFactory)
				{
					Varieties = {new Variety("variety1"), new Variety("variety2"), new Variety("variety3")}
				};
			projectService.Project.Returns(project);
			projectService.ProjectOpened += Raise.Event();

			Assert.That(wordLists.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2", "variety3"}));
			Assert.That(wordLists.IsEmpty, Is.False);

			project.Varieties.RemoveAt(0);
			Assert.That(wordLists.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety2", "variety3"}));
			Assert.That(wordLists.IsEmpty, Is.False);

			project.Varieties.Add(new Variety("variety1"));
			Assert.That(wordLists.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety2", "variety3", "variety1"}));
			Assert.That(wordLists.IsEmpty, Is.False);

			project.Varieties.Clear();
			Assert.That(wordLists.Varieties.Count, Is.EqualTo(0));
			Assert.That(wordLists.IsEmpty, Is.True);

			project = new CogProject(_spanFactory) {Varieties = {new Variety("variety1")}};
			projectService.Project.Returns(project);
			projectService.ProjectOpened += Raise.Event();

			Assert.That(wordLists.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1"}));
			Assert.That(wordLists.IsEmpty, Is.False);
		}

		[Test]
		public void Senses()
		{
			DispatcherHelper.Initialize();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			var analysisService = Substitute.For<IAnalysisService>();
			var importService = Substitute.For<IImportService>();
			var exportService = Substitute.For<IExportService>();

			WordViewModel.Factory wordFactory = word => new WordViewModel(busyService, analysisService, word);
			WordListsVarietySenseViewModel.Factory varietySenseFactory = (variety, sense) => new WordListsVarietySenseViewModel(busyService, analysisService, wordFactory, variety, sense);
			WordListsVarietyViewModel.Factory varietyFactory = variety => new WordListsVarietyViewModel(projectService, varietySenseFactory, variety);

			var wordLists = new WordListsViewModel(projectService, dialogService, importService, exportService, analysisService, varietyFactory);

			var project = new CogProject(_spanFactory)
				{
					Senses = {new Sense("sense1", "cat1"), new Sense("sense2", "cat2"), new Sense("sense3", "cat3")}
				};
			projectService.Project.Returns(project);
			projectService.ProjectOpened += Raise.Event();

			Assert.That(wordLists.Senses.Select(s => s.Gloss), Is.EqualTo(new[] {"sense1", "sense2", "sense3"}));
			Assert.That(wordLists.IsEmpty, Is.False);

			project.Senses.RemoveAt(0);
			Assert.That(wordLists.Senses.Select(s => s.Gloss), Is.EqualTo(new[] {"sense2", "sense3"}));
			Assert.That(wordLists.IsEmpty, Is.False);

			project.Senses.Add(new Sense("sense1", "cat1"));
			Assert.That(wordLists.Senses.Select(s => s.Gloss), Is.EqualTo(new[] {"sense2", "sense3", "sense1"}));
			Assert.That(wordLists.IsEmpty, Is.False);

			project.Senses.Clear();
			Assert.That(wordLists.Senses.Count, Is.EqualTo(0));
			Assert.That(wordLists.IsEmpty, Is.True);

			project = new CogProject(_spanFactory) {Senses = {new Sense("sense1", "cat1")}};
			projectService.Project.Returns(project);
			projectService.ProjectOpened += Raise.Event();
			Assert.That(wordLists.Senses.Select(s => s.Gloss), Is.EqualTo(new[] {"sense1"}));
			Assert.That(wordLists.IsEmpty, Is.False);
		}

		[Test]
		public void FindCommand()
		{
			DispatcherHelper.Initialize();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			var analysisService = Substitute.For<IAnalysisService>();
			var importService = Substitute.For<IImportService>();
			var exportService = Substitute.For<IExportService>();

			WordViewModel.Factory wordFactory = word => new WordViewModel(busyService, analysisService, word);
			WordListsVarietySenseViewModel.Factory varietySenseFactory = (variety, sense) => new WordListsVarietySenseViewModel(busyService, analysisService, wordFactory, variety, sense);
			WordListsVarietyViewModel.Factory varietyFactory = variety => new WordListsVarietyViewModel(projectService, varietySenseFactory, variety);

			var wordLists = new WordListsViewModel(projectService, dialogService, importService, exportService, analysisService, varietyFactory);

			var project = new CogProject(_spanFactory)
				{
					Senses = {new Sense("sense1", "cat1"), new Sense("sense2", "cat2"), new Sense("sense3", "cat3")},
					Varieties = {new Variety("variety1"), new Variety("variety2")}
				};
			project.Varieties[0].Words.AddRange(new[] {new Word("hello", project.Senses[0]), new Word("good", project.Senses[1]), new Word("bad", project.Senses[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("help", project.Senses[0]), new Word("google", project.Senses[1]), new Word("batter", project.Senses[2])});
			projectService.Project.Returns(project);
			projectService.ProjectOpened += Raise.Event();

			wordLists.VarietiesView = new ListCollectionView(wordLists.Varieties);

			FindViewModel findViewModel = null;
			Action closeCallback = null;
			dialogService.ShowModelessDialog(wordLists, Arg.Do<FindViewModel>(vm => findViewModel = vm), Arg.Do<Action>(callback => closeCallback = callback));
			wordLists.FindCommand.Execute(null);
			Assert.That(findViewModel, Is.Not.Null);
			Assert.That(closeCallback, Is.Not.Null);

			// already open, shouldn't get opened twice
			dialogService.ClearReceivedCalls();
			wordLists.FindCommand.Execute(null);
			dialogService.DidNotReceive().ShowModelessDialog(wordLists, Arg.Any<FindViewModel>(), Arg.Any<Action>());

			// form searches
			findViewModel.Field = FindField.Form;

			// nothing selected, no match
			findViewModel.String = "fall";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.Null);

			// nothing selected, matches
			findViewModel.String = "he";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[0].Senses[0]));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[1].Senses[0]));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[1].Senses[0]));

			// first word selected, matches
			wordLists.SelectedVarietySense = wordLists.Varieties[0].Senses[0];
			findViewModel.String = "o";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[0].Senses[1]));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[1].Senses[1]));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[0].Senses[0]));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[0].Senses[0]));
			// start search over
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[0].Senses[1]));

			// last word selected, matches
			wordLists.SelectedVarietySense = wordLists.Varieties[1].Senses[2];
			findViewModel.String = "ba";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[0].Senses[2]));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[1].Senses[2]));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[1].Senses[2]));

			// last word selected, matches, change selected word
			findViewModel.String = "ba";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[0].Senses[2]));
			wordLists.SelectedVarietySense = wordLists.Varieties[0].Senses[0];
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[0].Senses[2]));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[1].Senses[2]));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[1].Senses[2]));

			// sense searches

			// nothing selected, no match
			wordLists.SelectedVarietySense = null;
			findViewModel.Field = FindField.Gloss;
			findViewModel.String = "sense4";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.Null);

			// nothing selected, matches
			wordLists.SelectedVarietySense = null;
			findViewModel.Field = FindField.Gloss;
			findViewModel.String = "sense2";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[0].Senses[1]));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[0].Senses[1]));

			// selected, matches
			findViewModel.String = "sense";
			wordLists.SelectedVarietySense = wordLists.Varieties[1].Senses[1];
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[1].Senses[2]));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[1].Senses[0]));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[1].Senses[1]));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordLists.SelectedVarietySense, Is.EqualTo(wordLists.Varieties[1].Senses[1]));
		}

		[Test]
		public void TaskAreas()
		{
			DispatcherHelper.Initialize();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			var analysisService = Substitute.For<IAnalysisService>();
			var importService = Substitute.For<IImportService>();
			var exportService = Substitute.For<IExportService>();

			WordViewModel.Factory wordFactory = word => new WordViewModel(busyService, analysisService, word);
			WordListsVarietySenseViewModel.Factory varietySenseFactory = (variety, sense) => new WordListsVarietySenseViewModel(busyService, analysisService, wordFactory, variety, sense);
			WordListsVarietyViewModel.Factory varietyFactory = variety => new WordListsVarietyViewModel(projectService, varietySenseFactory, variety);

			var wordLists = new WordListsViewModel(projectService, dialogService, importService, exportService, analysisService, varietyFactory);

			var project = new CogProject(_spanFactory)
				{
					Varieties = {new Variety("variety1"), new Variety("variety2"), new Variety("variety3")}
				};
			projectService.Project.Returns(project);
			projectService.ProjectOpened += Raise.Event();

			var commonTasks = (TaskAreaItemsViewModel) wordLists.TaskAreas[0];

			// add a new variety
			var addVariety = (TaskAreaCommandViewModel) commonTasks.Items[0];
			dialogService.ShowModalDialog(wordLists, Arg.Do<EditVarietyViewModel>(vm => vm.Name = "variety4")).Returns(true);
			addVariety.Command.Execute(null);

			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2", "variety3", "variety4"}));
		}
	}
}
