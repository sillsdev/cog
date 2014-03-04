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
		public void Senses()
		{
			DispatcherHelper.Initialize();
			var projectService = Substitute.For<IProjectService>();
			var busyService = Substitute.For<IBusyService>();
			var exportService = Substitute.For<IExportService>();

			var alignment = new MultipleWordAlignmentViewModel(projectService, busyService, exportService);

			var project = new CogProject(_spanFactory)
				{
					Senses = {new Sense("sense2", "cat2"), new Sense("sense1", "cat1"), new Sense("sense3", "cat3")}
				};

			projectService.Project.Returns(project);
			projectService.ProjectOpened += Raise.Event();

			Assert.That(alignment.SelectedSense, Is.Null);
			alignment.SensesView = new ListCollectionView(alignment.Senses);

			Assert.That(alignment.SelectedSense.Gloss, Is.EqualTo("sense1"));
			Assert.That(alignment.SensesView.Cast<SenseViewModel>().Select(s => s.Gloss), Is.EqualTo(new[] {"sense1", "sense2", "sense3"}));

			project.Senses.Insert(0, new Sense("sense4", "cat4"));
			Assert.That(alignment.SensesView.Cast<SenseViewModel>().Select(s => s.Gloss), Is.EqualTo(new[] {"sense1", "sense2", "sense3", "sense4"}));
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
			foreach (VarietyPair vp in project.VarietyPairs)
			{
				wordPairGenerator.Process(vp);
				foreach (WordPair wp in vp.WordPairs)
				{
					wp.AreCognatePredicted = true;
					wp.CognicityScore = 1.0;
				}
				vp.SoundChangeFrequencyDistribution = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
				vp.SoundChangeProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(vp.SoundChangeFrequencyDistribution, (sc, fd) => new MaxLikelihoodProbabilityDistribution<Ngram<Segment>>(fd));
			}
			projectService.AreAllVarietiesCompared.Returns(true);
			projectService.ProjectOpened += Raise.Event();

			Assert.That(alignment.SelectedSense, Is.Null);
			Assert.That(alignment.Words, Is.Empty);

			alignment.SensesView = new ListCollectionView(alignment.Senses);
			alignment.WordsView = new ListCollectionView(alignment.Words);

			Assert.That(alignment.WordsView.Cast<MultipleWordAlignmentWordViewModel>().Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.loʊ", "hɛlp", "wɜrd"}));
			Assert.That(alignment.ColumnCount, Is.EqualTo(4));

			alignment.SelectedSense = alignment.Senses[1];

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
					wp.AreCognatePredicted = true;
					wp.CognicityScore = 1.0;
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
					wp.AreCognatePredicted = true;
				vp.SoundChangeFrequencyDistribution = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
				vp.SoundChangeProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(vp.SoundChangeFrequencyDistribution, (sc, fd) => new MaxLikelihoodProbabilityDistribution<Ngram<Segment>>(fd));
			}
			Messenger.Default.Send(new ComparisonPerformedMessage());

			Assert.That(alignment.WordsView.Cast<MultipleWordAlignmentWordViewModel>().Select(w => w.StrRep), Is.EqualTo(new[] {"gʊd"}));
			Assert.That(alignment.ColumnCount, Is.EqualTo(3));
		}
	}
}
