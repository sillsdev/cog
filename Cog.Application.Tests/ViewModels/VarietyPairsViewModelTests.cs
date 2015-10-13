using System;
using System.Linq;
using System.Windows.Data;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Application.Services;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Cog.TestUtils;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;
using SIL.Machine.Statistics;

namespace SIL.Cog.Application.Tests.ViewModels
{
	[TestFixture]
	public class VarietyPairsViewModelTests
	{
		[Test]
		public void Varieties_AddDataToEmptyProject_VarietyPairSelected()
		{
			using (var env = new TestEnvironment())
			{
				env.OpenProject();

				Assert.That(env.VarietyPairs.Varieties, Is.Empty);
				Assert.That(env.VarietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.NotSelected));
				Assert.That(env.VarietyPairs.SelectedVariety1, Is.Null);
				Assert.That(env.VarietyPairs.SelectedVariety2, Is.Null);
				Assert.That(env.VarietyPairs.SelectedVarietyPair, Is.Null);

				AddVarietyData(env);
				Messenger.Default.Send(new DomainModelChangedMessage(true));

				Assert.That(env.VarietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.SelectedAndNotCompared));
				Assert.That(env.VarietyPairs.SelectedVariety1, Is.EqualTo(env.VarietyPairs.VarietiesView1.Cast<VarietyViewModel>().First()));
				Assert.That(env.VarietyPairs.SelectedVariety2, Is.EqualTo(env.VarietyPairs.VarietiesView2.Cast<VarietyViewModel>().ElementAt(1)));
				Assert.That(env.VarietyPairs.SelectedVarietyPair, Is.Null);
			}
		}

		[Test]
		public void Varieties_PerformComparison_VarietyPairStateUpdated()
		{
			using (var env = new TestEnvironment())
			{
				AddVarietyData(env);
				env.OpenProject();

				Assert.That(env.VarietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.SelectedAndNotCompared));
				Assert.That(env.VarietyPairs.SelectedVariety1, Is.EqualTo(env.VarietyPairs.VarietiesView1.Cast<VarietyViewModel>().First()));
				Assert.That(env.VarietyPairs.SelectedVariety2, Is.EqualTo(env.VarietyPairs.VarietiesView2.Cast<VarietyViewModel>().ElementAt(1)));
				Assert.That(env.VarietyPairs.SelectedVarietyPair, Is.Null);

				Messenger.Default.Send(new PerformingComparisonMessage());
				AddComparisonData(env);
				Messenger.Default.Send(new ComparisonPerformedMessage());

				Assert.That(env.VarietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.SelectedAndCompared));
				Assert.That(env.VarietyPairs.SelectedVarietyPair, Is.Not.Null);
				Assert.That(env.VarietyPairs.SelectedVarietyPair.AreVarietiesInOrder, Is.True);
			}
		}

		[Test]
		public void Varieties_SwapSelectedVarieties_AreVarietiesInOrderEqualsTrue()
		{
			using (var env = new TestEnvironment())
			{
				AddVarietyData(env);
				AddComparisonData(env);
				env.OpenProject();

				Assert.That(env.VarietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.SelectedAndCompared));
				Assert.That(env.VarietyPairs.SelectedVarietyPair, Is.Not.Null);
				Assert.That(env.VarietyPairs.SelectedVarietyPair.AreVarietiesInOrder, Is.True);

				VarietyViewModel temp = env.VarietyPairs.SelectedVariety2;
				env.VarietyPairs.SelectedVariety2 = env.VarietyPairs.SelectedVariety1;
				env.VarietyPairs.SelectedVariety1 = temp;

				Assert.That(env.VarietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.SelectedAndCompared));
				Assert.That(env.VarietyPairs.SelectedVarietyPair, Is.Not.Null);
				Assert.That(env.VarietyPairs.SelectedVarietyPair.AreVarietiesInOrder, Is.False);
			}
		}

		[Test]
		public void Varieties_ChangeDomainModel_VarietyPairStateUpdated()
		{
			using (var env = new TestEnvironment())
			{
				AddVarietyData(env);
				AddComparisonData(env);
				env.OpenProject();

				Assert.That(env.VarietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.SelectedAndCompared));

				Messenger.Default.Send(new DomainModelChangedMessage(true));

				Assert.That(env.VarietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.SelectedAndNotCompared));
			}
		}

		[Test]
		public void Varieties_RemoveSelectedVarieties_SelectedVarietiesUpdated()
		{
			using (var env = new TestEnvironment())
			{
				AddVarietyData(env);
				env.OpenProject();

				env.VarietyPairs.VarietiesView1 = new ListCollectionView(env.VarietyPairs.Varieties);
				env.VarietyPairs.VarietiesView2 = new ListCollectionView(env.VarietyPairs.Varieties);

				Assert.That(env.VarietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.SelectedAndNotCompared));
				Assert.That(env.VarietyPairs.SelectedVariety1, Is.EqualTo(env.VarietyPairs.VarietiesView1.Cast<VarietyViewModel>().First()));
				Assert.That(env.VarietyPairs.SelectedVariety2, Is.EqualTo(env.VarietyPairs.VarietiesView2.Cast<VarietyViewModel>().ElementAt(1)));

				env.Project.Varieties.RemoveRangeAt(0, 2);
				Messenger.Default.Send(new DomainModelChangedMessage(true));

				Assert.That(env.VarietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.NotSelected));
				Assert.That(env.VarietyPairs.SelectedVariety1, Is.EqualTo(env.VarietyPairs.VarietiesView1.Cast<VarietyViewModel>().First()));
				Assert.That(env.VarietyPairs.SelectedVariety2, Is.EqualTo(env.VarietyPairs.VarietiesView2.Cast<VarietyViewModel>().First()));
			}
		}

		[Test]
		public void FindCommand_DialogOpen_NotOpenedAgain()
		{
			using (var env = new TestEnvironment())
			{
				SetupFindCommandTests(env);

				// already open, shouldn't get opened twice
				env.DialogService.ClearReceivedCalls();
				env.VarietyPairs.FindCommand.Execute(null);
				env.DialogService.DidNotReceive().ShowModelessDialog(env.VarietyPairs, Arg.Any<FindViewModel>(), Arg.Any<Action>());
			}
		}

		private static void SetupFindCommandTests(TestEnvironment env)
		{
			AddVarietyData(env);
			AddComparisonData(env);
			env.OpenProject();
			env.OpenFindDialog();
		}

		[Test]
		public void FindCommand_FormNothingSelectedNoMatches_NoWordPairSelected()
		{
			using (var env = new TestEnvironment())
			{
				SetupFindCommandTests(env);

				env.FindViewModel.Field = FindField.Form;
				env.FindViewModel.String = "nothing";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.Empty);
				Assert.That(env.Noncognates.SelectedWordPairs, Is.Empty);
			}
		}

		[Test]
		public void FindCommand_FormNothingSelectedMatches_CorrectWordPairsSelected()
		{
			using (var env = new TestEnvironment())
			{
				SetupFindCommandTests(env);

				WordPairViewModel[] cognatesArray = env.Cognates.WordPairsView.Cast<WordPairViewModel>().ToArray();
				WordPairViewModel[] noncognatesArray = env.Noncognates.WordPairsView.Cast<WordPairViewModel>().ToArray();

				env.FindViewModel.Field = FindField.Form;
				env.FindViewModel.String = "g";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.EquivalentTo(cognatesArray[1].ToEnumerable()));
				Assert.That(env.Noncognates.SelectedWordPairs, Is.Empty);
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.Empty);
				Assert.That(env.Noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.Empty);
				Assert.That(env.Noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));
			}
		}

		[Test]
		public void FindCommand_FormFirstWordSelectedMatches_CorrectWordPairsSelected()
		{
			using (var env = new TestEnvironment())
			{
				SetupFindCommandTests(env);

				WordPairViewModel[] cognatesArray = env.Cognates.WordPairsView.Cast<WordPairViewModel>().ToArray();
				WordPairViewModel[] noncognatesArray = env.Noncognates.WordPairsView.Cast<WordPairViewModel>().ToArray();

				env.Noncognates.SelectedWordPairs.Clear();
				env.Cognates.SelectedWordPairs.Add(cognatesArray[0]);
				env.FindViewModel.Field = FindField.Form;
				env.FindViewModel.String = "ʊ";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.Empty);
				Assert.That(env.Noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.EquivalentTo(cognatesArray[0].ToEnumerable()));
				Assert.That(env.Noncognates.SelectedWordPairs, Is.Empty);
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.EquivalentTo(cognatesArray[0].ToEnumerable()));
				Assert.That(env.Noncognates.SelectedWordPairs, Is.Empty);
				// start over
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.Empty);
				Assert.That(env.Noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));
			}
		}

		[Test]
		public void FindCommand_FormLastWordSelectedMatches_CorrectWordPairsSelected()
		{
			using (var env = new TestEnvironment())
			{
				SetupFindCommandTests(env);

				WordPairViewModel[] cognatesArray = env.Cognates.WordPairsView.Cast<WordPairViewModel>().ToArray();
				WordPairViewModel[] noncognatesArray = env.Noncognates.WordPairsView.Cast<WordPairViewModel>().ToArray();

				env.Cognates.SelectedWordPairs.Clear();
				env.Noncognates.SelectedWordPairs.Add(noncognatesArray[0]);
				env.FindViewModel.Field = FindField.Form;
				env.FindViewModel.String = "h";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.EquivalentTo(cognatesArray[0].ToEnumerable()));
				Assert.That(env.Noncognates.SelectedWordPairs, Is.Empty);
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.EquivalentTo(cognatesArray[0].ToEnumerable()));
				Assert.That(env.Noncognates.SelectedWordPairs, Is.Empty);
			}
		}

		[Test]
		public void FindCommand_SwitchVarietyPairChangeSelectedWordMatches_CorrectWordPairsSelected()
		{
			using (var env = new TestEnvironment())
			{
				SetupFindCommandTests(env);

				env.VarietyPairs.SelectedVariety2 = env.VarietyPairs.Varieties[2];
				env.SetupWordPairsViews();

				WordPairViewModel[] noncognatesArray = env.Noncognates.WordPairsView.Cast<WordPairViewModel>().ToArray();

				env.FindViewModel.Field = FindField.Form;
				env.FindViewModel.String = "l";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.Empty);
				Assert.That(env.Noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.Empty);
				Assert.That(env.Noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[2].ToEnumerable()));
				env.Noncognates.SelectedWordPairs.Clear();
				env.Noncognates.SelectedWordPairs.Add(noncognatesArray[1]);
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.Empty);
				Assert.That(env.Noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[2].ToEnumerable()));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.Empty);
				Assert.That(env.Noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.Empty);
				Assert.That(env.Noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));
			}
		}

		[Test]
		public void FindCommand_GlossNothingSelectedMatches_CorrectWordPairsSelected()
		{
			using (var env = new TestEnvironment())
			{
				SetupFindCommandTests(env);

				WordPairViewModel[] noncognatesArray = env.Noncognates.WordPairsView.Cast<WordPairViewModel>().ToArray();

				env.FindViewModel.Field = FindField.Gloss;
				env.FindViewModel.String = "gloss2";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.Empty);
				Assert.That(env.Noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.Cognates.SelectedWordPairs, Is.Empty);
				Assert.That(env.Noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));
			}
		}

		[Test]
		public void TaskAreas_ChangeSorting_WordPairOrderingUpdated()
		{
			using (var env = new TestEnvironment())
			{
				env.Project.Meanings.AddRange(new[] {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3"), new Meaning("gloss4", "cat4")});
				env.Project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2"), new Variety("variety3")});
				env.Project.Varieties[0].Words.AddRange(new[] {new Word("hɛ.loʊ", env.Project.Meanings[0]), new Word("gʊd", env.Project.Meanings[1]), new Word("bæd", env.Project.Meanings[2]), new Word("wɜrd", env.Project.Meanings[3])});
				env.Project.Varieties[1].Words.AddRange(new[] {new Word("hɛlp", env.Project.Meanings[0]), new Word("gu.gəl", env.Project.Meanings[1]), new Word("gu.fi", env.Project.Meanings[2]), new Word("kɑr", env.Project.Meanings[3])});
				env.AnalysisService.SegmentAll();

				var varietyPairGenerator = new VarietyPairGenerator();
				varietyPairGenerator.Process(env.Project);
				var wordPairGenerator = new SimpleWordPairGenerator(env.SegmentPool, env.Project, 0.3, ComponentIdentifiers.PrimaryWordAligner);
				foreach (VarietyPair vp in env.Project.VarietyPairs)
				{
					wordPairGenerator.Process(vp);
					vp.SoundChangeFrequencyDistribution = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
					vp.SoundChangeProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(vp.SoundChangeFrequencyDistribution, (sc, fd) => new MaxLikelihoodProbabilityDistribution<Ngram<Segment>>(fd));
				}

				int i = 0;
				foreach (WordPair wp in env.Project.VarietyPairs[0].WordPairs)
				{
					wp.PhoneticSimilarityScore = (1.0 / env.Project.VarietyPairs[0].WordPairs.Count) * (i + 1);
					wp.AreCognatePredicted = wp.Meaning.Gloss.IsOneOf("gloss1", "gloss3");
					i++;
				}

				env.OpenProject();

				env.SetupWordPairsViews();
				WordPairViewModel[] cognatesArray = env.Cognates.WordPairsView.Cast<WordPairViewModel>().ToArray();
				WordPairViewModel[] noncognatesArray = env.Noncognates.WordPairsView.Cast<WordPairViewModel>().ToArray();

				Assert.That(cognatesArray.Select(wp => wp.Meaning.Gloss), Is.EqualTo(new[] {"gloss3", "gloss1"}));
				Assert.That(noncognatesArray.Select(wp => wp.Meaning.Gloss), Is.EqualTo(new[] {"gloss4", "gloss2"}));

				var commonTasks = (TaskAreaItemsViewModel) env.VarietyPairs.TaskAreas[0];
				var sortWordsByItems = (TaskAreaItemsViewModel) commonTasks.Items[2];
				var sortWordsByGroup = (TaskAreaCommandGroupViewModel) sortWordsByItems.Items[0];
				// default sorting is by similarity, change to gloss
				sortWordsByGroup.SelectedCommand = sortWordsByGroup.Commands[1];
				sortWordsByGroup.SelectedCommand.Command.Execute(null);
				cognatesArray = env.Cognates.WordPairsView.Cast<WordPairViewModel>().ToArray();
				noncognatesArray = env.Noncognates.WordPairsView.Cast<WordPairViewModel>().ToArray();
				Assert.That(cognatesArray.Select(wp => wp.Meaning.Gloss), Is.EqualTo(new[] {"gloss1", "gloss3"}));
				Assert.That(noncognatesArray.Select(wp => wp.Meaning.Gloss), Is.EqualTo(new[] {"gloss2", "gloss4"}));
			}
		}

		private static void AddVarietyData(TestEnvironment env)
		{
			env.Project.Meanings.AddRange(new[] {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3")});
			env.Project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2"), new Variety("variety3")});
			env.Project.Varieties[0].Words.AddRange(new[] {new Word("hɛ.loʊ", env.Project.Meanings[0]), new Word("gʊd", env.Project.Meanings[1]), new Word("bæd", env.Project.Meanings[2])});
			env.Project.Varieties[1].Words.AddRange(new[] {new Word("hɛlp", env.Project.Meanings[0]), new Word("gu.gəl", env.Project.Meanings[1]), new Word("gu.fi", env.Project.Meanings[2])});
			env.Project.Varieties[2].Words.AddRange(new[] {new Word("wɜrd", env.Project.Meanings[0]), new Word("kɑr", env.Project.Meanings[1]), new Word("fʊt.bɔl", env.Project.Meanings[2])});
			env.AnalysisService.SegmentAll();
		}

		private static void AddComparisonData(TestEnvironment env)
		{
			var varietyPairGenerator = new VarietyPairGenerator();
			varietyPairGenerator.Process(env.Project);
			var wordPairGenerator = new SimpleWordPairGenerator(env.SegmentPool, env.Project, 0.3, ComponentIdentifiers.PrimaryWordAligner);
			foreach (VarietyPair vp in env.Project.VarietyPairs)
			{
				wordPairGenerator.Process(vp);
				vp.SoundChangeFrequencyDistribution = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
				vp.SoundChangeProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(vp.SoundChangeFrequencyDistribution, (sc, fd) => new MaxLikelihoodProbabilityDistribution<Ngram<Segment>>(fd));
			}

			int i = 0;
			foreach (WordPair wp in env.Project.VarietyPairs[0].WordPairs)
			{
				wp.PhoneticSimilarityScore = (1.0 / env.Project.VarietyPairs[0].WordPairs.Count) * (env.Project.VarietyPairs[0].WordPairs.Count - i);
				wp.AreCognatePredicted = wp.Meaning.Gloss.IsOneOf("gloss1", "gloss3");
				i++;
			}

			i = 0;
			foreach (WordPair wp in env.Project.VarietyPairs[1].WordPairs)
			{
				wp.PhoneticSimilarityScore = (1.0 / env.Project.VarietyPairs[1].WordPairs.Count) * (env.Project.VarietyPairs[1].WordPairs.Count - i);
				i++;
			}
		}

		private class TestEnvironment : IDisposable
		{
			private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();
			private readonly VarietyPairsViewModel _varietyPairs;
			private readonly IAnalysisService _analysisService;
			private readonly IProjectService _projectService;
			private readonly SegmentPool _segmentPool;
			private readonly CogProject _project;
			private readonly IDialogService _dialogService;
			private FindViewModel _findViewModel;

			public TestEnvironment()
			{
				DispatcherHelper.Initialize();
				_segmentPool = new SegmentPool();
				_projectService = Substitute.For<IProjectService>();
				_dialogService = Substitute.For<IDialogService>();
				var busyService = Substitute.For<IBusyService>();
				var exportService = Substitute.For<IExportService>();
				_analysisService = new AnalysisService(_spanFactory, _segmentPool, _projectService, _dialogService, busyService);

				WordPairsViewModel.Factory wordPairsFactory = () => new WordPairsViewModel(busyService);
				VarietyPairViewModel.Factory varietyPairFactory = (vp, order) => new VarietyPairViewModel(_segmentPool, _projectService, wordPairsFactory, vp, order);

				_varietyPairs = new VarietyPairsViewModel(_projectService, busyService, _dialogService, exportService, _analysisService, varietyPairFactory);

				_project = TestHelpers.GetTestProject(_spanFactory, _segmentPool);
				_projectService.Project.Returns(_project);
			}

			public SegmentPool SegmentPool
			{
				get { return _segmentPool; }
			}

			public CogProject Project
			{
				get { return _project; }
			}

			public IAnalysisService AnalysisService
			{
				get { return _analysisService; }
			}

			public VarietyPairsViewModel VarietyPairs
			{
				get { return _varietyPairs; }
			}

			public IDialogService DialogService
			{
				get { return _dialogService; }
			}

			public FindViewModel FindViewModel
			{
				get { return _findViewModel; }
			}

			public WordPairsViewModel Cognates
			{
				get { return _varietyPairs.SelectedVarietyPair.Cognates; }
			}

			public WordPairsViewModel Noncognates
			{
				get { return _varietyPairs.SelectedVarietyPair.Noncognates; }
			}

			public void OpenProject()
			{
				_projectService.ProjectOpened += Raise.Event();

				_varietyPairs.VarietiesView1 = new ListCollectionView(_varietyPairs.Varieties);
				_varietyPairs.VarietiesView2 = new ListCollectionView(_varietyPairs.Varieties);
			}

			public void SetupWordPairsViews()
			{
				Cognates.WordPairsView = new ListCollectionView(Cognates.WordPairs);
				Noncognates.WordPairsView = new ListCollectionView(Noncognates.WordPairs);
			}

			public void OpenFindDialog()
			{
				SetupWordPairsViews();

				_dialogService.ShowModelessDialog(_varietyPairs, Arg.Do<FindViewModel>(vm => _findViewModel = vm), Arg.Any<Action>());
				_varietyPairs.FindCommand.Execute(null);
			}

			public void Dispose()
			{
				Messenger.Reset();
			}
		}
	}
}
