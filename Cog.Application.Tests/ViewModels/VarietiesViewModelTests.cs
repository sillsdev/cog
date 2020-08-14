using System;
using System.Windows.Data;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Application.Services;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.Extensions;
using SIL.Cog.TestUtils;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Application.Tests.ViewModels
{
	[TestFixture]
	public class VarietiesViewModelTests
	{
		private class TestEnvironment : IDisposable
		{
			private readonly IProjectService _projectService;

			public TestEnvironment()
			{
				DispatcherHelper.Initialize();
				_projectService = Substitute.For<IProjectService>();
				DialogService = Substitute.For<IDialogService>();
				var busyService = Substitute.For<IBusyService>();
				var analysisService = Substitute.For<IAnalysisService>();

				WordsViewModel.Factory wordsFactory = words => new WordsViewModel(busyService, words);
				WordViewModel.Factory wordFactory = word => new WordViewModel(busyService, analysisService, word);
				VarietiesVarietyViewModel.Factory varietyFactory = variety => new VarietiesVarietyViewModel(_projectService, DialogService, wordsFactory, wordFactory, variety);

				VarietiesViewModel = new VarietiesViewModel(_projectService, DialogService, analysisService, varietyFactory);
			}

			public void OpenProject(CogProject project)
			{
				_projectService.Project.Returns(project);
				_projectService.ProjectOpened += Raise.Event();
				VarietiesViewModel.VarietiesView = new ListCollectionView(VarietiesViewModel.Varieties);
				if (VarietiesViewModel.SelectedVariety != null)
				{
					WordsViewModel wordsViewModel = VarietiesViewModel.SelectedVariety.Words;
					wordsViewModel.WordsView = new ListCollectionView(wordsViewModel.Words);
				}
			}

			public void OpenFindDialog()
			{
				DialogService.ShowModelessDialog(VarietiesViewModel, Arg.Do((Action<FindViewModel>)(vm => FindViewModel = vm)), Arg.Any<Action>());
				VarietiesViewModel.FindCommand.Execute(null);
			}

			public IDialogService DialogService { get; }

			public VarietiesViewModel VarietiesViewModel { get; }

			public FindViewModel FindViewModel { get; private set; }

			public void Dispose()
			{
				Messenger.Reset();
			}
		}

		[Test]
		public void Varieties_AddVariety_NewVarietySelected()
		{
			using (var env = new TestEnvironment())
			{
				var project = new CogProject();
				env.OpenProject(project);

				Assert.That(env.VarietiesViewModel.Varieties, Is.Empty);
				Assert.That(env.VarietiesViewModel.IsVarietySelected, Is.False);
				Assert.That(env.VarietiesViewModel.SelectedVariety, Is.Null);

				project.Varieties.Add(new Variety("variety1"));

				Assert.That(env.VarietiesViewModel.Varieties.Count, Is.EqualTo(1));
				Assert.That(env.VarietiesViewModel.Varieties[0].Name, Is.EqualTo("variety1"));
				Assert.That(env.VarietiesViewModel.IsVarietySelected, Is.True);
				Assert.That(env.VarietiesViewModel.SelectedVariety, Is.EqualTo(env.VarietiesViewModel.VarietiesView.Cast<VarietiesVarietyViewModel>().First()));
			}
		}

		[Test]
		public void Varieties_ExistingVarieties_VarietiesSorted()
		{
			using (var env = new TestEnvironment())
			{
				Assert.That(env.VarietiesViewModel.IsVarietySelected, Is.False);
				Assert.That(env.VarietiesViewModel.SelectedVariety, Is.Null);

				var project = new CogProject()
				{
					Varieties = {new Variety("French"), new Variety("English"), new Variety("Spanish")}
				};
				env.OpenProject(project);

				env.VarietiesViewModel.VarietiesView = new ListCollectionView(env.VarietiesViewModel.Varieties);
				VarietiesVarietyViewModel[] varietiesViewArray = env.VarietiesViewModel.VarietiesView.Cast<VarietiesVarietyViewModel>().ToArray();
				Assert.That(env.VarietiesViewModel.IsVarietySelected, Is.True);
				Assert.That(env.VarietiesViewModel.SelectedVariety, Is.EqualTo(varietiesViewArray[0]));
				// should be sorted
				Assert.That(varietiesViewArray.Select(v => v.Name), Is.EqualTo(new[] {"English", "French", "Spanish"}));
			}
		}

		private CogProject SetupProjectWithWords(TestEnvironment env)
		{
			var segmentPool = new SegmentPool();
			CogProject project = TestHelpers.GetTestProject(segmentPool);
			project.Meanings.AddRange(new[] {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3")});
			project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2")});
			project.Varieties[0].Words.AddRange(new[] {new Word("hello", project.Meanings[0]), new Word("good", project.Meanings[1]), new Word("bad", project.Meanings[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("help", project.Meanings[0]), new Word("google search", project.Meanings[1]), new Word("goofy", project.Meanings[2])});

			var segmenter = new Segmenter();
			var varietySegmenter = new VarietySegmenter(project.Segmenter);
			foreach (Variety variety in project.Varieties)
				varietySegmenter.Process(variety);

			env.OpenProject(project);
			return project;
		}

		[Test]
		public void FindCommand_DialogOpen_NotOpenedAgain()
		{
			using (var env = new TestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				env.DialogService.ClearReceivedCalls();
				env.VarietiesViewModel.FindCommand.Execute(null);
				env.DialogService.DidNotReceive().ShowModelessDialog(env.VarietiesViewModel, Arg.Any<FindViewModel>(), Arg.Any<Action>());
			}
		}

		[Test]
		public void FindCommand_FormNothingSelectedNoMatches_NoWordSelected()
		{
			using (var env = new TestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				env.FindViewModel.Field = FindField.Form;
				env.FindViewModel.String = "fall";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.VarietiesViewModel.SelectedVariety.Words.SelectedWords, Is.Empty);
			}
		}

		[Test]
		public void FindCommand_FormNothingSelectedMatches_CorrectWordsSelected()
		{
			using (var env = new TestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				env.FindViewModel.Field = FindField.Form;
				env.FindViewModel.String = "he";
				env.FindViewModel.FindNextCommand.Execute(null);

				WordsViewModel wordsViewModel = env.VarietiesViewModel.SelectedVariety.Words;
				WordViewModel[] wordsViewArray = wordsViewModel.WordsView.Cast<WordViewModel>().ToArray();
				Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[0].ToEnumerable()));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[0].ToEnumerable()));
			}
		}

		[Test]
		public void FindCommand_FormFirstWordSelectedMatches_CorrectWordsSelected()
		{
			using (var env = new TestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				env.FindViewModel.Field = FindField.Form;
				WordsViewModel wordsViewModel = env.VarietiesViewModel.SelectedVariety.Words;
				WordViewModel[] wordsViewArray = wordsViewModel.WordsView.Cast<WordViewModel>().ToArray();
				wordsViewModel.SelectedWords.Add(wordsViewArray[0]);
				env.FindViewModel.String = "o";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[1].ToEnumerable()));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[0].ToEnumerable()));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[0].ToEnumerable()));
				// start search over
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[1].ToEnumerable()));
			}
		}

		[Test]
		public void FindCommand_FormLastWordSelectedMatches_CorrectWordsSelected()
		{
			using (var env = new TestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				env.FindViewModel.Field = FindField.Form;
				WordsViewModel wordsViewModel = env.VarietiesViewModel.SelectedVariety.Words;
				WordViewModel[] wordsViewArray = wordsViewModel.WordsView.Cast<WordViewModel>().ToArray();
				wordsViewModel.SelectedWords.Add(wordsViewArray[2]);
				env.FindViewModel.String = "ba";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[2].ToEnumerable()));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[2].ToEnumerable()));
			}
		}

		[Test]
		public void FindCommand_SwitchVarietyFormNothingSelectedMatches_CorrectWordsSelected()
		{
			using (var env = new TestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				env.VarietiesViewModel.SelectedVariety = env.VarietiesViewModel.Varieties[1];
				WordsViewModel wordsViewModel = env.VarietiesViewModel.SelectedVariety.Words;
				wordsViewModel.WordsView = new ListCollectionView(wordsViewModel.Words);
				WordViewModel[] wordsViewArray = wordsViewModel.WordsView.Cast<WordViewModel>().ToArray();
				env.FindViewModel.Field = FindField.Form;
				env.FindViewModel.String = "go";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[1].ToEnumerable()));
				wordsViewModel.SelectedWords.Clear();
				wordsViewModel.SelectedWords.Add(wordsViewArray[0]);
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[1].ToEnumerable()));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[2].ToEnumerable()));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[2].ToEnumerable()));
			}
		}

		[Test]
		public void FindCommand_GlossNothingSelectedMatches_CorrectWordsSelected()
		{
			using (var env = new TestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				// gloss searches
				env.FindViewModel.Field = FindField.Gloss;
				env.FindViewModel.String = "gloss2";
				env.FindViewModel.FindNextCommand.Execute(null);
				WordsViewModel wordsViewModel = env.VarietiesViewModel.SelectedVariety.Words;
				WordViewModel[] wordsViewArray = wordsViewModel.WordsView.Cast<WordViewModel>().ToArray();
				Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[1].ToEnumerable()));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[1].ToEnumerable()));
			}
		}

		[Test]
		public void TaskAreas_AddVariety_VarietyAdded()
		{
			using (var env = new TestEnvironment())
			{
				SetupProjectWithWords(env);

				var commonTasks = (TaskAreaItemsViewModel) env.VarietiesViewModel.TaskAreas[0];

				var addVariety = (TaskAreaCommandViewModel) commonTasks.Items[0];
				env.DialogService.ShowModalDialog(env.VarietiesViewModel, Arg.Do<EditVarietyViewModel>(vm => vm.Name = "variety3")).Returns(true);
				addVariety.Command.Execute(null);
				Assert.That(env.VarietiesViewModel.SelectedVariety.Name, Is.EqualTo("variety3"));
				Assert.That(env.VarietiesViewModel.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2", "variety3"}));
			}
		}

		[Test]
		public void TaskAreas_RenameVariety_VarietyRenamed()
		{
			using (var env = new TestEnvironment())
			{
				CogProject project = SetupProjectWithWords(env);

				var commonTasks = (TaskAreaItemsViewModel) env.VarietiesViewModel.TaskAreas[0];
				var renameVariety = (TaskAreaCommandViewModel) commonTasks.Items[1];
				env.VarietiesViewModel.SelectedVariety = env.VarietiesViewModel.Varieties.First(v => v.Name == "variety2");
				env.DialogService.ShowModalDialog(env.VarietiesViewModel, Arg.Do<EditVarietyViewModel>(vm => vm.Name = "variety3")).Returns(true);
				renameVariety.Command.Execute(null);
				Assert.That(env.VarietiesViewModel.SelectedVariety.Name, Is.EqualTo("variety3"));
				Assert.That(env.VarietiesViewModel.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety3"}));
				Assert.That(project.Varieties.Contains("variety2"), Is.False);
			}
		}

		[Test]
		public void TaskAreas_RemoveVariety_VarietyRemoved()
		{
			using (var env = new TestEnvironment())
			{
				SetupProjectWithWords(env);

				var commonTasks = (TaskAreaItemsViewModel) env.VarietiesViewModel.TaskAreas[0];
				var removeVariety = (TaskAreaCommandViewModel) commonTasks.Items[2];
				env.VarietiesViewModel.SelectedVariety = env.VarietiesViewModel.Varieties.First(v => v.Name == "variety2");
				env.DialogService.ShowYesNoQuestion(env.VarietiesViewModel, Arg.Any<string>(), Arg.Any<string>()).Returns(true);
				removeVariety.Command.Execute(null);
				Assert.That(env.VarietiesViewModel.SelectedVariety.Name, Is.EqualTo("variety1"));
				Assert.That(env.VarietiesViewModel.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1"}));
			}
		}

		[Test]
		public void TaskAreas_SortBy_WordsSortedCorrectly()
		{
			using (var env = new TestEnvironment())
			{
				SetupProjectWithWords(env);

				var commonTasks = (TaskAreaItemsViewModel) env.VarietiesViewModel.TaskAreas[0];
				env.VarietiesViewModel.SelectedVariety = env.VarietiesViewModel.Varieties.First(v => v.Name == "variety2");

				WordsViewModel wordsViewModel = env.VarietiesViewModel.SelectedVariety.Words;
				wordsViewModel.WordsView = new ListCollectionView(wordsViewModel.Words);
				var sortWordsByItems = (TaskAreaItemsViewModel) commonTasks.Items[4];
				var sortWordsByGroup = (TaskAreaCommandGroupViewModel) sortWordsByItems.Items[0];
				// default sorting is by gloss, change to form
				sortWordsByGroup.SelectedCommand = sortWordsByGroup.Commands[1];
				sortWordsByGroup.SelectedCommand.Command.Execute(null);
				Assert.That(wordsViewModel.WordsView.Cast<WordViewModel>().Select(w => w.StrRep), Is.EqualTo(new[] {"goofy", "google search", "help"}));
				// sort by validity
				sortWordsByGroup.SelectedCommand = sortWordsByGroup.Commands[2];
				sortWordsByGroup.SelectedCommand.Command.Execute(null);
				Assert.That(wordsViewModel.WordsView.Cast<WordViewModel>().Select(w => w.StrRep), Is.EqualTo(new[] {"google search", "help", "goofy"}));
				// change sorting back to gloss
				sortWordsByGroup.SelectedCommand = sortWordsByGroup.Commands[0];
				sortWordsByGroup.SelectedCommand.Command.Execute(null);
				Assert.That(wordsViewModel.WordsView.Cast<WordViewModel>().Select(w => w.StrRep), Is.EqualTo(new[] {"help", "google search", "goofy"}));
			}
		}
	}
}
