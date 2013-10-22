using System;
using System.Windows.Data;
using System.Linq;
using GalaSoft.MvvmLight.Threading;
using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Applications.Services;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.Applications.Test.ViewModels
{
	[TestFixture]
	public class VarietiesViewModelTest
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

			WordsViewModel.Factory wordsFactory = words => new WordsViewModel(busyService, words);
			WordViewModel.Factory wordFactory = word => new WordViewModel(busyService, analysisService, word);
			VarietiesVarietyViewModel.Factory varietyFactory = variety => new VarietiesVarietyViewModel(projectService, dialogService, wordsFactory, wordFactory, variety);

			var varieties = new VarietiesViewModel(projectService, dialogService, analysisService, varietyFactory);

			var project = new CogProject(_spanFactory);
			projectService.Project.Returns(project);
			projectService.ProjectOpened += Raise.Event();

			varieties.VarietiesView = new ListCollectionView(varieties.Varieties);

			Assert.That(varieties.Varieties, Is.Empty);
			Assert.That(varieties.IsVarietySelected, Is.False);
			Assert.That(varieties.SelectedVariety, Is.Null);

			project.Varieties.Add(new Variety("variety1"));

			Assert.That(varieties.Varieties.Count, Is.EqualTo(1));
			Assert.That(varieties.Varieties[0].Name, Is.EqualTo("variety1"));
			Assert.That(varieties.IsVarietySelected, Is.True);
			Assert.That(varieties.SelectedVariety, Is.EqualTo(varieties.VarietiesView.Cast<VarietiesVarietyViewModel>().First()));

			project = new CogProject(_spanFactory)
				{
					Varieties = {new Variety("French"), new Variety("English"), new Variety("Spanish")}
				};
			projectService.Project.Returns(project);
			projectService.ProjectOpened += Raise.Event();

			Assert.That(varieties.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"French", "English", "Spanish"}));
			Assert.That(varieties.IsVarietySelected, Is.False);
			Assert.That(varieties.SelectedVariety, Is.Null);

			varieties.VarietiesView = new ListCollectionView(varieties.Varieties);
			VarietiesVarietyViewModel[] varietiesViewArray = varieties.VarietiesView.Cast<VarietiesVarietyViewModel>().ToArray();
			Assert.That(varieties.IsVarietySelected, Is.True);
			Assert.That(varieties.SelectedVariety, Is.EqualTo(varietiesViewArray[0]));
			// should be sorted
			Assert.That(varietiesViewArray.Select(v => v.Name), Is.EqualTo(new[] {"English", "French", "Spanish"}));
		}

		[Test]
		public void FindCommand()
		{
			DispatcherHelper.Initialize();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			var analysisService = Substitute.For<IAnalysisService>();

			WordsViewModel.Factory wordsFactory = words => new WordsViewModel(busyService, words);
			WordViewModel.Factory wordFactory = word => new WordViewModel(busyService, analysisService, word);
			VarietiesVarietyViewModel.Factory varietyFactory = variety => new VarietiesVarietyViewModel(projectService, dialogService, wordsFactory, wordFactory, variety);

			var varieties = new VarietiesViewModel(projectService, dialogService, analysisService, varietyFactory);

			var project = new CogProject(_spanFactory)
				{
					Senses = {new Sense("sense1", "cat1"), new Sense("sense2", "cat2"), new Sense("sense3", "cat3")},
					Varieties = {new Variety("variety1"), new Variety("variety2")}
				};
			project.Varieties[0].Words.AddRange(new[] {new Word("hello", project.Senses[0]), new Word("good", project.Senses[1]), new Word("bad", project.Senses[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("help", project.Senses[0]), new Word("google", project.Senses[1]), new Word("goofy", project.Senses[2])});
			projectService.Project.Returns(project);
			projectService.ProjectOpened += Raise.Event();

			varieties.VarietiesView = new ListCollectionView(varieties.Varieties);
			WordsViewModel wordsViewModel = varieties.SelectedVariety.Words;
			wordsViewModel.WordsView = new ListCollectionView(wordsViewModel.Words);
			WordViewModel[] wordsViewArray = wordsViewModel.WordsView.Cast<WordViewModel>().ToArray();

			FindViewModel findViewModel = null;
			Action closeCallback = null;
			dialogService.ShowModelessDialog(varieties, Arg.Do<FindViewModel>(vm => findViewModel = vm), Arg.Do<Action>(callback => closeCallback = callback));
			varieties.FindCommand.Execute(null);
			Assert.That(findViewModel, Is.Not.Null);
			Assert.That(closeCallback, Is.Not.Null);

			// already open, shouldn't get opened twice
			dialogService.ClearReceivedCalls();
			varieties.FindCommand.Execute(null);
			dialogService.DidNotReceive().ShowModelessDialog(varieties, Arg.Any<FindViewModel>(), Arg.Any<Action>());

			// form searches
			findViewModel.Field = FindField.Form;

			// nothing selected, no match
			findViewModel.String = "fall";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordsViewModel.SelectedWords, Is.Empty);

			// nothing selected, matches
			findViewModel.String = "he";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[0].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[0].ToEnumerable()));

			// first word selected, matches
			findViewModel.String = "o";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[1].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[0].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[0].ToEnumerable()));
			// start search over
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[1].ToEnumerable()));

			// last word selected, matches
			wordsViewModel.SelectedWords.Clear();
			wordsViewModel.SelectedWords.Add(wordsViewArray[2]);
			findViewModel.String = "ba";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[2].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[2].ToEnumerable()));

			// switch variety, nothing selected, matches, change selected word
			varieties.SelectedVariety = varieties.Varieties[1];
			wordsViewModel = varieties.SelectedVariety.Words;
			wordsViewModel.WordsView = new ListCollectionView(wordsViewModel.Words);
			wordsViewArray = wordsViewModel.WordsView.Cast<WordViewModel>().ToArray();
			findViewModel.String = "go";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[1].ToEnumerable()));
			wordsViewModel.SelectedWords.Clear();
			wordsViewModel.SelectedWords.Add(wordsViewArray[0]);
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[1].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[2].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[2].ToEnumerable()));

			// sense searches
			findViewModel.Field = FindField.Gloss;
			findViewModel.String = "sense2";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[1].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(wordsViewModel.SelectedWords, Is.EquivalentTo(wordsViewArray[1].ToEnumerable()));
		}

		[Test]
		public void TaskAreas()
		{
			DispatcherHelper.Initialize();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			var analysisService = Substitute.For<IAnalysisService>();

			WordsViewModel.Factory wordsFactory = words => new WordsViewModel(busyService, words);
			WordViewModel.Factory wordFactory = word => new WordViewModel(busyService, analysisService, word);
			VarietiesVarietyViewModel.Factory varietyFactory = variety => new VarietiesVarietyViewModel(projectService, dialogService, wordsFactory, wordFactory, variety);

			var varieties = new VarietiesViewModel(projectService, dialogService, analysisService, varietyFactory);

			var project = new CogProject(_spanFactory)
				{
					Senses = {new Sense("sense1", "cat1"), new Sense("sense2", "cat2"), new Sense("sense3", "cat3")},
					Varieties = {new Variety("variety1"), new Variety("variety2")}
				};
			project.Varieties[0].Words.AddRange(new[] {new Word("hello", project.Senses[0]), new Word("good", project.Senses[1]), new Word("bad", project.Senses[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("help", project.Senses[0]), new Word("google", project.Senses[1]), new Word("goofy", project.Senses[2])});
			projectService.Project.Returns(project);
			projectService.ProjectOpened += Raise.Event();

			varieties.VarietiesView = new ListCollectionView(varieties.Varieties);

			var commonTasks = (TaskAreaItemsViewModel) varieties.TaskAreas[0];

			var addVariety = (TaskAreaCommandViewModel) commonTasks.Items[0];
			dialogService.ShowModalDialog(varieties, Arg.Do<EditVarietyViewModel>(vm => vm.Name = "variety3")).Returns(true);
			addVariety.Command.Execute(null);
			Assert.That(varieties.SelectedVariety.Name, Is.EqualTo("variety3"));
			Assert.That(varieties.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2", "variety3"}));

			var renameVariety = (TaskAreaCommandViewModel) commonTasks.Items[1];
			dialogService.ShowModalDialog(varieties, Arg.Do<EditVarietyViewModel>(vm => vm.Name = "variety4")).Returns(true);
			renameVariety.Command.Execute(null);
			Assert.That(varieties.SelectedVariety.Name, Is.EqualTo("variety4"));
			Assert.That(varieties.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2", "variety4"}));

			var removeVariety = (TaskAreaCommandViewModel) commonTasks.Items[2];
			dialogService.ShowYesNoQuestion(varieties, Arg.Any<string>(), Arg.Any<string>()).Returns(true);
			removeVariety.Command.Execute(null);
			Assert.That(varieties.SelectedVariety.Name, Is.EqualTo("variety1"));
			Assert.That(varieties.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));

			varieties.SelectedVariety = varieties.Varieties.First(v => v.Name == "variety2");

			WordsViewModel wordsViewModel = varieties.SelectedVariety.Words;
			wordsViewModel.WordsView = new ListCollectionView(wordsViewModel.Words);
			var sortWordsByItems = (TaskAreaItemsViewModel) commonTasks.Items[4];
			var sortWordsByGroup = (TaskAreaCommandGroupViewModel) sortWordsByItems.Items[0];
			// default sorting is by sense, change to form
			sortWordsByGroup.SelectedCommand = sortWordsByGroup.Commands[1];
			sortWordsByGroup.SelectedCommand.Command.Execute(null);
			Assert.That(wordsViewModel.WordsView.Cast<WordViewModel>().Select(w => w.StrRep), Is.EqualTo(new[] {"goofy", "google", "help"}));
			// change sorting back to sense
			sortWordsByGroup.SelectedCommand = sortWordsByGroup.Commands[0];
			sortWordsByGroup.SelectedCommand.Command.Execute(null);
			Assert.That(wordsViewModel.WordsView.Cast<WordViewModel>().Select(w => w.StrRep), Is.EqualTo(new[] {"help", "google", "goofy"}));
		}
	}
}
