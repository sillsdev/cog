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
		private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();

		[Test]
		public void Varieties()
		{
			DispatcherHelper.Initialize();
			var segmentPool = new SegmentPool();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			var exportService = Substitute.For<IExportService>();
			var analysisService = new AnalysisService(_spanFactory, segmentPool, projectService, dialogService, busyService);

			WordPairsViewModel.Factory wordPairsFactory = () => new WordPairsViewModel(busyService);
			VarietyPairViewModel.Factory varietyPairFactory = (vp, order) => new VarietyPairViewModel(segmentPool, projectService, wordPairsFactory, vp, order);

			var varietyPairs = new VarietyPairsViewModel(projectService, busyService, dialogService, exportService, analysisService, varietyPairFactory);

			CogProject project = TestHelpers.GetTestProject(_spanFactory, segmentPool);
			projectService.Project.Returns(project);
			projectService.ProjectOpened += Raise.Event();

			varietyPairs.VarietiesView1 = new ListCollectionView(varietyPairs.Varieties);
			varietyPairs.VarietiesView2 = new ListCollectionView(varietyPairs.Varieties);

			Assert.That(varietyPairs.Varieties, Is.Empty);
			Assert.That(varietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.NotSelected));
			Assert.That(varietyPairs.SelectedVariety1, Is.Null);
			Assert.That(varietyPairs.SelectedVariety2, Is.Null);
			Assert.That(varietyPairs.SelectedVarietyPair, Is.Null);

			project.Meanings.AddRange(new[] {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3")});
			project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2"), new Variety("variety3")});
			project.Varieties[0].Words.AddRange(new[] {new Word("hɛ.loʊ", project.Meanings[0]), new Word("gʊd", project.Meanings[1]), new Word("bæd", project.Meanings[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("hɛlp", project.Meanings[0]), new Word("gu.gəl", project.Meanings[1]), new Word("gu.fi", project.Meanings[2])});
			project.Varieties[2].Words.AddRange(new[] {new Word("wɜrd", project.Meanings[0]), new Word("kɑr", project.Meanings[1]), new Word("fʊt.bɔl", project.Meanings[2])});
			analysisService.SegmentAll();
			Messenger.Default.Send(new DomainModelChangedMessage(true));

			Assert.That(varietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.SelectedAndNotCompared));
			Assert.That(varietyPairs.SelectedVariety1, Is.EqualTo(varietyPairs.VarietiesView1.Cast<VarietyViewModel>().First()));
			Assert.That(varietyPairs.SelectedVariety2, Is.EqualTo(varietyPairs.VarietiesView2.Cast<VarietyViewModel>().ElementAt(1)));
			Assert.That(varietyPairs.SelectedVarietyPair, Is.Null);

			Messenger.Default.Send(new PerformingComparisonMessage());
			var varietyPairGenerator = new VarietyPairGenerator();
			varietyPairGenerator.Process(project);
			var wordPairGenerator = new SimpleWordPairGenerator(segmentPool, project, 0.3, ComponentIdentifiers.PrimaryWordAligner);
			foreach (VarietyPair vp in project.VarietyPairs)
			{
				wordPairGenerator.Process(vp);
				vp.SoundChangeFrequencyDistribution = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
				vp.SoundChangeProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(vp.SoundChangeFrequencyDistribution, (sc, fd) => new MaxLikelihoodProbabilityDistribution<Ngram<Segment>>(fd));
			}
			Messenger.Default.Send(new ComparisonPerformedMessage());

			Assert.That(varietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.SelectedAndCompared));
			Assert.That(varietyPairs.SelectedVarietyPair, Is.Not.Null);
			Assert.That(varietyPairs.SelectedVarietyPair.AreVarietiesInOrder, Is.True);

			VarietyViewModel temp = varietyPairs.SelectedVariety2;
			varietyPairs.SelectedVariety2 = varietyPairs.SelectedVariety1;
			varietyPairs.SelectedVariety1 = temp;

			Assert.That(varietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.SelectedAndCompared));
			Assert.That(varietyPairs.SelectedVarietyPair, Is.Not.Null);
			Assert.That(varietyPairs.SelectedVarietyPair.AreVarietiesInOrder, Is.False);

			Messenger.Default.Send(new DomainModelChangedMessage(true));
			Assert.That(varietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.SelectedAndNotCompared));

			project.Varieties.RemoveRangeAt(0, 2);
			Messenger.Default.Send(new DomainModelChangedMessage(true));
			Assert.That(varietyPairs.VarietyPairState, Is.EqualTo(VarietyPairState.NotSelected));
			Assert.That(varietyPairs.SelectedVariety1, Is.EqualTo(varietyPairs.VarietiesView1.Cast<VarietyViewModel>().First()));
			Assert.That(varietyPairs.SelectedVariety2, Is.EqualTo(varietyPairs.VarietiesView2.Cast<VarietyViewModel>().First()));
		}

		[Test]
		public void FindCommand()
		{
			DispatcherHelper.Initialize();
			var segmentPool = new SegmentPool();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			var exportService = Substitute.For<IExportService>();
			var analysisService = new AnalysisService(_spanFactory, segmentPool, projectService, dialogService, busyService);

			WordPairsViewModel.Factory wordPairsFactory = () => new WordPairsViewModel(busyService);
			VarietyPairViewModel.Factory varietyPairFactory = (vp, order) => new VarietyPairViewModel(segmentPool, projectService, wordPairsFactory, vp, order);

			var varietyPairs = new VarietyPairsViewModel(projectService, busyService, dialogService, exportService, analysisService, varietyPairFactory);

			CogProject project = TestHelpers.GetTestProject(_spanFactory, segmentPool);
			project.Meanings.AddRange(new[] {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3")});
			project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2"), new Variety("variety3")});
			project.Varieties[0].Words.AddRange(new[] {new Word("hɛ.loʊ", project.Meanings[0]), new Word("gʊd", project.Meanings[1]), new Word("bæd", project.Meanings[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("hɛlp", project.Meanings[0]), new Word("gu.gəl", project.Meanings[1]), new Word("gu.fi", project.Meanings[2])});
			project.Varieties[2].Words.AddRange(new[] {new Word("wɜrd", project.Meanings[0]), new Word("kɑr", project.Meanings[1]), new Word("fʊt.bɔl", project.Meanings[2])});
			projectService.Project.Returns(project);
			analysisService.SegmentAll();
			var varietyPairGenerator = new VarietyPairGenerator();
			varietyPairGenerator.Process(project);
			var wordPairGenerator = new SimpleWordPairGenerator(segmentPool, project, 0.3, ComponentIdentifiers.PrimaryWordAligner);
			foreach (VarietyPair vp in project.VarietyPairs)
			{
				wordPairGenerator.Process(vp);
				vp.SoundChangeFrequencyDistribution = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
				vp.SoundChangeProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(vp.SoundChangeFrequencyDistribution, (sc, fd) => new MaxLikelihoodProbabilityDistribution<Ngram<Segment>>(fd));
			}

			int i = 0;
			foreach (WordPair wp in project.VarietyPairs[0].WordPairs)
			{
				wp.PhoneticSimilarityScore = (1.0 / project.VarietyPairs[0].WordPairs.Count) * (project.VarietyPairs[0].WordPairs.Count - i);
				wp.AreCognatePredicted = wp.Meaning.Gloss.IsOneOf("gloss1", "gloss3");
				i++;
			}

			i = 0;
			foreach (WordPair wp in project.VarietyPairs[1].WordPairs)
			{
				wp.PhoneticSimilarityScore = (1.0 / project.VarietyPairs[1].WordPairs.Count) * (project.VarietyPairs[1].WordPairs.Count - i);
				i++;
			}

			projectService.ProjectOpened += Raise.Event();

			varietyPairs.VarietiesView1 = new ListCollectionView(varietyPairs.Varieties);
			varietyPairs.VarietiesView2 = new ListCollectionView(varietyPairs.Varieties);

			WordPairsViewModel cognates = varietyPairs.SelectedVarietyPair.Cognates;
			cognates.WordPairsView = new ListCollectionView(cognates.WordPairs);
			WordPairViewModel[] cognatesArray = cognates.WordPairsView.Cast<WordPairViewModel>().ToArray();
			WordPairsViewModel noncognates = varietyPairs.SelectedVarietyPair.Noncognates;
			noncognates.WordPairsView = new ListCollectionView(noncognates.WordPairs);
			WordPairViewModel[] noncognatesArray = noncognates.WordPairsView.Cast<WordPairViewModel>().ToArray();

			FindViewModel findViewModel = null;
			Action closeCallback = null;
			dialogService.ShowModelessDialog(varietyPairs, Arg.Do<FindViewModel>(vm => findViewModel = vm), Arg.Do<Action>(callback => closeCallback = callback));
			varietyPairs.FindCommand.Execute(null);
			Assert.That(findViewModel, Is.Not.Null);
			Assert.That(closeCallback, Is.Not.Null);

			// already open, shouldn't get opened twice
			dialogService.ClearReceivedCalls();
			varietyPairs.FindCommand.Execute(null);
			dialogService.DidNotReceive().ShowModelessDialog(varietyPairs, Arg.Any<FindViewModel>(), Arg.Any<Action>());

			// form searches
			findViewModel.Field = FindField.Form;

			// nothing selected, no match
			findViewModel.String = "nothing";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.Empty);
			Assert.That(noncognates.SelectedWordPairs, Is.Empty);

			// nothing selected, matches
			findViewModel.String = "g";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.EquivalentTo(cognatesArray[1].ToEnumerable()));
			Assert.That(noncognates.SelectedWordPairs, Is.Empty);
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.Empty);
			Assert.That(noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.Empty);
			Assert.That(noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));

			// first word selected, matches
			noncognates.SelectedWordPairs.Clear();
			cognates.SelectedWordPairs.Add(cognatesArray[0]);
			findViewModel.String = "ʊ";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.Empty);
			Assert.That(noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.EquivalentTo(cognatesArray[0].ToEnumerable()));
			Assert.That(noncognates.SelectedWordPairs, Is.Empty);
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.EquivalentTo(cognatesArray[0].ToEnumerable()));
			Assert.That(noncognates.SelectedWordPairs, Is.Empty);
			// start over
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.Empty);
			Assert.That(noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));

			// last word selected, matches
			cognates.SelectedWordPairs.Clear();
			noncognates.SelectedWordPairs.Add(noncognatesArray[0]);
			findViewModel.String = "h";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.EquivalentTo(cognatesArray[0].ToEnumerable()));
			Assert.That(noncognates.SelectedWordPairs, Is.Empty);
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.EquivalentTo(cognatesArray[0].ToEnumerable()));
			Assert.That(noncognates.SelectedWordPairs, Is.Empty);

			// switch variety pair, nothing selected, no cognates, matches, change selected word
			varietyPairs.SelectedVariety2 = varietyPairs.Varieties[2];
			cognates = varietyPairs.SelectedVarietyPair.Cognates;
			cognates.WordPairsView = new ListCollectionView(cognates.WordPairs);
			noncognates = varietyPairs.SelectedVarietyPair.Noncognates;
			noncognates.WordPairsView = new ListCollectionView(noncognates.WordPairs);
			noncognatesArray = noncognates.WordPairsView.Cast<WordPairViewModel>().ToArray();

			findViewModel.String = "l";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.Empty);
			Assert.That(noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.Empty);
			Assert.That(noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[2].ToEnumerable()));
			noncognates.SelectedWordPairs.Clear();
			noncognates.SelectedWordPairs.Add(noncognatesArray[1]);
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.Empty);
			Assert.That(noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[2].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.Empty);
			Assert.That(noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.Empty);
			Assert.That(noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[0].ToEnumerable()));

			// gloss searches
			findViewModel.Field = FindField.Gloss;
			findViewModel.String = "gloss2";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.Empty);
			Assert.That(noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[1].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(cognates.SelectedWordPairs, Is.Empty);
			Assert.That(noncognates.SelectedWordPairs, Is.EquivalentTo(noncognatesArray[1].ToEnumerable()));
		}

		[Test]
		public void TaskAreas()
		{
			DispatcherHelper.Initialize();
			var segmentPool = new SegmentPool();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			var exportService = Substitute.For<IExportService>();
			var analysisService = new AnalysisService(_spanFactory, segmentPool, projectService, dialogService, busyService);

			WordPairsViewModel.Factory wordPairsFactory = () => new WordPairsViewModel(busyService);
			VarietyPairViewModel.Factory varietyPairFactory = (vp, order) => new VarietyPairViewModel(segmentPool, projectService, wordPairsFactory, vp, order);

			var varietyPairs = new VarietyPairsViewModel(projectService, busyService, dialogService, exportService, analysisService, varietyPairFactory);

			CogProject project = TestHelpers.GetTestProject(_spanFactory, segmentPool);
			project.Meanings.AddRange(new[] {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3"), new Meaning("gloss4", "cat4")});
			project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2"), new Variety("variety3")});
			project.Varieties[0].Words.AddRange(new[] {new Word("hɛ.loʊ", project.Meanings[0]), new Word("gʊd", project.Meanings[1]), new Word("bæd", project.Meanings[2]), new Word("wɜrd", project.Meanings[3])});
			project.Varieties[1].Words.AddRange(new[] {new Word("hɛlp", project.Meanings[0]), new Word("gu.gəl", project.Meanings[1]), new Word("gu.fi", project.Meanings[2]), new Word("kɑr", project.Meanings[3])});
			projectService.Project.Returns(project);
			analysisService.SegmentAll();
			var varietyPairGenerator = new VarietyPairGenerator();
			varietyPairGenerator.Process(project);
			var wordPairGenerator = new SimpleWordPairGenerator(segmentPool, project, 0.3, ComponentIdentifiers.PrimaryWordAligner);
			foreach (VarietyPair vp in project.VarietyPairs)
			{
				wordPairGenerator.Process(vp);
				vp.SoundChangeFrequencyDistribution = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
				vp.SoundChangeProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(vp.SoundChangeFrequencyDistribution, (sc, fd) => new MaxLikelihoodProbabilityDistribution<Ngram<Segment>>(fd));
			}

			int i = 0;
			foreach (WordPair wp in project.VarietyPairs[0].WordPairs)
			{
				wp.PhoneticSimilarityScore = (1.0 / project.VarietyPairs[0].WordPairs.Count) * (i + 1);
				wp.AreCognatePredicted = wp.Meaning.Gloss.IsOneOf("gloss1", "gloss3");
				i++;
			}

			projectService.ProjectOpened += Raise.Event();

			varietyPairs.VarietiesView1 = new ListCollectionView(varietyPairs.Varieties);
			varietyPairs.VarietiesView2 = new ListCollectionView(varietyPairs.Varieties);

			WordPairsViewModel cognates = varietyPairs.SelectedVarietyPair.Cognates;
			cognates.WordPairsView = new ListCollectionView(cognates.WordPairs);
			WordPairViewModel[] cognatesArray = cognates.WordPairsView.Cast<WordPairViewModel>().ToArray();
			WordPairsViewModel noncognates = varietyPairs.SelectedVarietyPair.Noncognates;
			noncognates.WordPairsView = new ListCollectionView(noncognates.WordPairs);
			WordPairViewModel[] noncognatesArray = noncognates.WordPairsView.Cast<WordPairViewModel>().ToArray();

			Assert.That(cognatesArray.Select(wp => wp.Meaning.Gloss), Is.EqualTo(new[] {"gloss3", "gloss1"}));
			Assert.That(noncognatesArray.Select(wp => wp.Meaning.Gloss), Is.EqualTo(new[] {"gloss4", "gloss2"}));

			var commonTasks = (TaskAreaItemsViewModel) varietyPairs.TaskAreas[0];
			var sortWordsByItems = (TaskAreaItemsViewModel) commonTasks.Items[2];
			var sortWordsByGroup = (TaskAreaCommandGroupViewModel) sortWordsByItems.Items[0];
			// default sorting is by similarity, change to gloss
			sortWordsByGroup.SelectedCommand = sortWordsByGroup.Commands[1];
			sortWordsByGroup.SelectedCommand.Command.Execute(null);
			cognatesArray = cognates.WordPairsView.Cast<WordPairViewModel>().ToArray();
			noncognatesArray = noncognates.WordPairsView.Cast<WordPairViewModel>().ToArray();
			Assert.That(cognatesArray.Select(wp => wp.Meaning.Gloss), Is.EqualTo(new[] {"gloss1", "gloss3"}));
			Assert.That(noncognatesArray.Select(wp => wp.Meaning.Gloss), Is.EqualTo(new[] {"gloss2", "gloss4"}));
		}
	}
}
