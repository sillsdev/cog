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
using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;
using SIL.Machine.Statistics;

namespace SIL.Cog.Application.Tests.ViewModels
{
	[TestFixture]
	public class MultipleWordAlignmentViewModelTests
	{
		private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();

		[Test]
		public void Meanings()
		{
			DispatcherHelper.Initialize();
			var projectService = Substitute.For<IProjectService>();
			var busyService = Substitute.For<IBusyService>();
			var exportService = Substitute.For<IExportService>();

			var alignment = new MultipleWordAlignmentViewModel(projectService, busyService, exportService);

			var project = new CogProject(_spanFactory)
				{
					Meanings = {new Meaning("gloss2", "cat2"), new Meaning("gloss1", "cat1"), new Meaning("gloss3", "cat3")}
				};

			projectService.Project.Returns(project);
			projectService.ProjectOpened += Raise.Event();

			Assert.That(alignment.SelectedMeaning, Is.Null);
			alignment.MeaningsView = new ListCollectionView(alignment.Meanings);

			Assert.That(alignment.SelectedMeaning.Gloss, Is.EqualTo("gloss1"));
			Assert.That(alignment.MeaningsView.Cast<MeaningViewModel>().Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1", "gloss2", "gloss3"}));

			project.Meanings.Insert(0, new Meaning("gloss4", "cat4"));
			Assert.That(alignment.MeaningsView.Cast<MeaningViewModel>().Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1", "gloss2", "gloss3", "gloss4"}));
		}

		[Test]
		public void Words()
		{
			DispatcherHelper.Initialize();
			var segmentPool = new SegmentPool();
			var projectService = Substitute.For<IProjectService>();
			var busyService = Substitute.For<IBusyService>();
			var exportService = Substitute.For<IExportService>();
			var dialogService = Substitute.For<IDialogService>();
			var analysisService = new AnalysisService(_spanFactory, segmentPool, projectService, dialogService, busyService);

			var alignment = new MultipleWordAlignmentViewModel(projectService, busyService, exportService);

			var project = TestHelpers.GetTestProject(_spanFactory, segmentPool);
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
				foreach (WordPair wp in vp.WordPairs)
				{
					wp.PredictedCognacy = true;
					wp.PredictedCognacyScore = 1.0;
				}
				vp.SoundChangeFrequencyDistribution = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
				vp.SoundChangeProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(vp.SoundChangeFrequencyDistribution, (sc, fd) => new MaxLikelihoodProbabilityDistribution<Ngram<Segment>>(fd));
			}
			projectService.AreAllVarietiesCompared.Returns(true);
			projectService.ProjectOpened += Raise.Event();

			Assert.That(alignment.SelectedMeaning, Is.Null);
			Assert.That(alignment.Words, Is.Empty);

			alignment.MeaningsView = new ListCollectionView(alignment.Meanings);
			alignment.WordsView = new ListCollectionView(alignment.Words);

			Assert.That(alignment.WordsView.Cast<MultipleWordAlignmentWordViewModel>().Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.loʊ", "hɛlp", "wɜrd"}));
			Assert.That(alignment.ColumnCount, Is.EqualTo(4));

			alignment.SelectedMeaning = alignment.Meanings[1];

			Assert.That(alignment.WordsView.Cast<MultipleWordAlignmentWordViewModel>().Select(w => w.StrRep), Is.EqualTo(new[] {"gu.gəl", "gʊd", "kɑr"}));
			Assert.That(alignment.ColumnCount, Is.EqualTo(5));

			project.Varieties.RemoveAt(project.Varieties.Count - 1);
			Messenger.Default.Send(new DomainModelChangedMessage(true));

			Messenger.Default.Send(new PerformingComparisonMessage());
			foreach (VarietyPair vp in project.VarietyPairs)
			{
				wordPairGenerator.Process(vp);
				foreach (WordPair wp in vp.WordPairs)
				{
					wp.PredictedCognacy = true;
					wp.PredictedCognacyScore = 1.0;
				}
				vp.SoundChangeFrequencyDistribution = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
				vp.SoundChangeProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(vp.SoundChangeFrequencyDistribution, (sc, fd) => new MaxLikelihoodProbabilityDistribution<Ngram<Segment>>(fd));
			}
			Messenger.Default.Send(new ComparisonPerformedMessage());

			Assert.That(alignment.WordsView.Cast<MultipleWordAlignmentWordViewModel>().Select(w => w.StrRep), Is.EqualTo(new[] {"gu.gəl", "gʊd"}));
			Assert.That(alignment.ColumnCount, Is.EqualTo(5));

			project.Varieties.RemoveAt(project.Varieties.Count - 1);

			Messenger.Default.Send(new PerformingComparisonMessage());
			foreach (VarietyPair vp in project.VarietyPairs)
			{
				wordPairGenerator.Process(vp);
				foreach (WordPair wp in vp.WordPairs)
					wp.PredictedCognacy = true;
				vp.SoundChangeFrequencyDistribution = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
				vp.SoundChangeProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(vp.SoundChangeFrequencyDistribution, (sc, fd) => new MaxLikelihoodProbabilityDistribution<Ngram<Segment>>(fd));
			}
			Messenger.Default.Send(new ComparisonPerformedMessage());

			Assert.That(alignment.WordsView, Is.Empty);
		}
	}
}
