using System;
using System.Linq;
using System.Windows.Data;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Applications.Services;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;
using SIL.Machine.Statistics;

namespace SIL.Cog.Applications.Test.ViewModels
{
	[TestFixture]
	public class GlobalCorrespondencesViewModelTest
	{
		private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();

		[Test]
		public void Graph()
		{
			DispatcherHelper.Initialize();
			var segmentPool = new SegmentPool();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			var graphService = new GraphService(projectService);
			var imageExportService = Substitute.For<IImageExportService>();
			var analysisService = new AnalysisService(_spanFactory, segmentPool, projectService, dialogService, busyService);

			WordPairsViewModel.Factory wordPairsFactory = () => new WordPairsViewModel(busyService);

			var globalCorrespondences = new GlobalCorrespondencesViewModel(projectService, busyService, dialogService, imageExportService, graphService, wordPairsFactory);

			CogProject project = TestHelpers.GetTestProject(_spanFactory, segmentPool);
			project.Senses.AddRange(new[] {new Sense("sense1", "cat1"), new Sense("sense2", "cat2"), new Sense("sense3", "cat3")});
			project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2"), new Variety("variety3")});
			project.Varieties[0].Words.AddRange(new[] {new Word("hɛ.loʊ", project.Senses[0]), new Word("gʊd", project.Senses[1]), new Word("bæd", project.Senses[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("hɛlp", project.Senses[0]), new Word("gu.gəl", project.Senses[1]), new Word("gu.fi", project.Senses[2])});
			project.Varieties[2].Words.AddRange(new[] {new Word("wɜrd", project.Senses[0]), new Word("kɑr", project.Senses[1]), new Word("fʊt.bɔl", project.Senses[2])});
			projectService.Project.Returns(project);
			analysisService.SegmentAll();
			projectService.ProjectOpened += Raise.Event();

			Assert.That(globalCorrespondences.Graph, Is.Null);

			Messenger.Default.Send(new PerformingComparisonMessage());
			var varietyPairGenerator = new VarietyPairGenerator();
			varietyPairGenerator.Process(project);
			var wordPairGenerator = new WordPairGenerator(project, "primary");
			var globalCorrIdentifier = new SoundCorrespondenceIdentifier(segmentPool, project, "primary");
			foreach (VarietyPair vp in project.VarietyPairs)
			{
				wordPairGenerator.Process(vp);
				foreach (WordPair wp in vp.WordPairs)
					wp.AreCognatePredicted = true;
				vp.SoundChangeFrequencyDistribution = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
				vp.SoundChangeProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(vp.SoundChangeFrequencyDistribution, (sc, fd) => new MaxLikelihoodProbabilityDistribution<Ngram<Segment>>(fd));
				globalCorrIdentifier.Process(vp);
			}

			Messenger.Default.Send(new ComparisonPerformedMessage());

			Assert.That(globalCorrespondences.Graph, Is.Not.Null);

			globalCorrespondences.SyllablePosition = SyllablePosition.Nucleus;
			Assert.That(globalCorrespondences.Graph, Is.Not.Null);

			Messenger.Default.Send(new DomainModelChangedMessage(true));
			Assert.That(globalCorrespondences.Graph, Is.Null);

			globalCorrespondences.SyllablePosition = SyllablePosition.Coda;
			Assert.That(globalCorrespondences.Graph, Is.Null);
		}

		[Test]
		public void ObservedWordPairs()
		{
			DispatcherHelper.Initialize();
			var segmentPool = new SegmentPool();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			var graphService = new GraphService(projectService);
			var imageExportService = Substitute.For<IImageExportService>();
			var analysisService = new AnalysisService(_spanFactory, segmentPool, projectService, dialogService, busyService);

			WordPairsViewModel.Factory wordPairsFactory = () => new WordPairsViewModel(busyService);

			var globalCorrespondences = new GlobalCorrespondencesViewModel(projectService, busyService, dialogService, imageExportService, graphService, wordPairsFactory);

			CogProject project = TestHelpers.GetTestProject(_spanFactory, segmentPool);
			project.Senses.AddRange(new[] {new Sense("sense1", "cat1"), new Sense("sense2", "cat2"), new Sense("sense3", "cat3")});
			project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2"), new Variety("variety3")});
			project.Varieties[0].Words.AddRange(new[] {new Word("hɛ.loʊ", project.Senses[0]), new Word("gʊd", project.Senses[1]), new Word("bæd", project.Senses[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("hɛlp", project.Senses[0]), new Word("gu.gəl", project.Senses[1]), new Word("gu.fi", project.Senses[2])});
			project.Varieties[2].Words.AddRange(new[] {new Word("wɜrd", project.Senses[0]), new Word("kɑr", project.Senses[1]), new Word("fʊt.bɔl", project.Senses[2])});
			projectService.Project.Returns(project);
			analysisService.SegmentAll();

			var varietyPairGenerator = new VarietyPairGenerator();
			varietyPairGenerator.Process(project);
			var wordPairGenerator = new WordPairGenerator(project, "primary");
			var globalCorrIdentifier = new SoundCorrespondenceIdentifier(segmentPool, project, "primary");
			foreach (VarietyPair vp in project.VarietyPairs)
			{
				wordPairGenerator.Process(vp);
				foreach (WordPair wp in vp.WordPairs)
					wp.AreCognatePredicted = true;
				vp.SoundChangeFrequencyDistribution = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
				vp.SoundChangeProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(vp.SoundChangeFrequencyDistribution, (sc, fd) => new MaxLikelihoodProbabilityDistribution<Ngram<Segment>>(fd));
				globalCorrIdentifier.Process(vp);
			}
			projectService.AreAllVarietiesCompared.Returns(true);
			projectService.ProjectOpened += Raise.Event();

			var observedWordPairs = globalCorrespondences.ObservedWordPairs;
			observedWordPairs.WordPairsView = new ListCollectionView(observedWordPairs.WordPairs);

			Assert.That(observedWordPairs.WordPairsView, Is.Empty);

			globalCorrespondences.SelectedCorrespondence = globalCorrespondences.Graph.Edges.First(e => ((GlobalSegmentVertex) e.Source).StrRep == "g" && ((GlobalSegmentVertex) e.Target).StrRep == "k");
			WordPairViewModel[] wordPairsArray = observedWordPairs.WordPairsView.Cast<WordPairViewModel>().ToArray();
			Assert.That(wordPairsArray.Length, Is.EqualTo(2));
			Assert.That(wordPairsArray[0].Sense.Gloss, Is.EqualTo("sense2"));
			Assert.That(wordPairsArray[0].AlignedNodes[2].IsSelected, Is.True);
			Assert.That(wordPairsArray[1].Sense.Gloss, Is.EqualTo("sense2"));
			Assert.That(wordPairsArray[1].AlignedNodes[0].IsSelected, Is.True);

			globalCorrespondences.SyllablePosition = SyllablePosition.Nucleus;
			globalCorrespondences.SelectedCorrespondence = globalCorrespondences.Graph.Edges.First(e => ((GlobalSegmentVertex) e.Source).StrRep == "ʊ" && ((GlobalSegmentVertex) e.Target).StrRep == "u");
			wordPairsArray = observedWordPairs.WordPairsView.Cast<WordPairViewModel>().ToArray();
			Assert.That(wordPairsArray.Length, Is.EqualTo(2));
			Assert.That(wordPairsArray[0].Sense.Gloss, Is.EqualTo("sense2"));
			Assert.That(wordPairsArray[0].AlignedNodes[1].IsSelected, Is.True);
			Assert.That(wordPairsArray[1].Sense.Gloss, Is.EqualTo("sense3"));
			Assert.That(wordPairsArray[1].AlignedNodes[1].IsSelected, Is.True);

			globalCorrespondences.SelectedCorrespondence = null;
			Assert.That(observedWordPairs.WordPairsView, Is.Empty);
		}

		[Test]
		public void FindCommand()
		{
			DispatcherHelper.Initialize();
			var segmentPool = new SegmentPool();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			var graphService = new GraphService(projectService);
			var imageExportService = Substitute.For<IImageExportService>();
			var analysisService = new AnalysisService(_spanFactory, segmentPool, projectService, dialogService, busyService);

			WordPairsViewModel.Factory wordPairsFactory = () => new WordPairsViewModel(busyService);

			var globalCorrespondences = new GlobalCorrespondencesViewModel(projectService, busyService, dialogService, imageExportService, graphService, wordPairsFactory);

			CogProject project = TestHelpers.GetTestProject(_spanFactory, segmentPool);
			project.Senses.AddRange(new[] {new Sense("sense1", "cat1"), new Sense("sense2", "cat2"), new Sense("sense3", "cat3")});
			project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2"), new Variety("variety3")});
			project.Varieties[0].Words.AddRange(new[] {new Word("hɛ.loʊ", project.Senses[0]), new Word("gʊd", project.Senses[1]), new Word("kæd", project.Senses[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("hɛlp", project.Senses[0]), new Word("gu.gəl", project.Senses[1]), new Word("gu.fi", project.Senses[2])});
			project.Varieties[2].Words.AddRange(new[] {new Word("wɜrd", project.Senses[0]), new Word("kɑr", project.Senses[1]), new Word("fʊt.bɔl", project.Senses[2])});
			projectService.Project.Returns(project);
			analysisService.SegmentAll();

			var varietyPairGenerator = new VarietyPairGenerator();
			varietyPairGenerator.Process(project);
			var wordPairGenerator = new WordPairGenerator(project, "primary");
			var globalCorrIdentifier = new SoundCorrespondenceIdentifier(segmentPool, project, "primary");
			foreach (VarietyPair vp in project.VarietyPairs)
			{
				wordPairGenerator.Process(vp);
				foreach (WordPair wp in vp.WordPairs)
					wp.AreCognatePredicted = true;
				vp.SoundChangeFrequencyDistribution = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
				vp.SoundChangeProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(vp.SoundChangeFrequencyDistribution, (sc, fd) => new MaxLikelihoodProbabilityDistribution<Ngram<Segment>>(fd));
				globalCorrIdentifier.Process(vp);
			}
			projectService.AreAllVarietiesCompared.Returns(true);
			projectService.ProjectOpened += Raise.Event();

			WordPairsViewModel observedWordPairs = globalCorrespondences.ObservedWordPairs;
			observedWordPairs.WordPairsView = new ListCollectionView(observedWordPairs.WordPairs);

			FindViewModel findViewModel = null;
			Action closeCallback = null;
			dialogService.ShowModelessDialog(globalCorrespondences, Arg.Do<FindViewModel>(vm => findViewModel = vm), Arg.Do<Action>(callback => closeCallback = callback));
			globalCorrespondences.FindCommand.Execute(null);
			Assert.That(findViewModel, Is.Not.Null);
			Assert.That(closeCallback, Is.Not.Null);

			// already open, shouldn't get opened twice
			dialogService.ClearReceivedCalls();
			globalCorrespondences.FindCommand.Execute(null);
			dialogService.DidNotReceive().ShowModelessDialog(globalCorrespondences, Arg.Any<FindViewModel>(), Arg.Any<Action>());

			// form searches
			findViewModel.Field = FindField.Form;

			// no word pairs, no match
			findViewModel.String = "nothing";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.Empty);

			globalCorrespondences.SelectedCorrespondence = globalCorrespondences.Graph.Edges.First(e => ((GlobalSegmentVertex) e.Source).StrRep == "k" && ((GlobalSegmentVertex) e.Target).StrRep == "g");
			WordPairViewModel[] wordPairsArray = observedWordPairs.WordPairsView.Cast<WordPairViewModel>().ToArray();

			// nothing selected, no match
			findViewModel.String = "nothing";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.Empty);

			// nothing selected, matches
			findViewModel.String = "d";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[1].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[2].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[2].ToEnumerable()));

			// first word selected, matches
			observedWordPairs.SelectedWordPairs.Clear();
			observedWordPairs.SelectedWordPairs.Add(wordPairsArray[0]);
			findViewModel.String = "g";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[1].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[2].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[0].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[0].ToEnumerable()));
			// start search over
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[1].ToEnumerable()));

			// last word selected, matches
			observedWordPairs.SelectedWordPairs.Clear();
			observedWordPairs.SelectedWordPairs.Add(wordPairsArray[2]);
			findViewModel.String = "u";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[0].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[2].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[2].ToEnumerable()));

			// nothing selected, matches, change selected word
			observedWordPairs.SelectedWordPairs.Clear();
			findViewModel.String = ".";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[0].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[2].ToEnumerable()));
			observedWordPairs.SelectedWordPairs.Clear();
			observedWordPairs.SelectedWordPairs.Add(wordPairsArray[1]);
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[2].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[0].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[0].ToEnumerable()));

			// sense searches
			findViewModel.Field = FindField.Gloss;
			findViewModel.String = "sense2";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[1].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[0].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWordPairs.SelectedWordPairs, Is.EquivalentTo(wordPairsArray[0].ToEnumerable()));
		}
	}
}
