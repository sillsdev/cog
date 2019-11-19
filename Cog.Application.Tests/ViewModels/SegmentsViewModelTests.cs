using System;
using System.Windows.Data;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using NSubstitute;
using NUnit.Framework;
using System.Linq;
using SIL.Cog.Application.Services;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.Cog.TestUtils;
using SIL.Extensions;

namespace SIL.Cog.Application.Tests.ViewModels
{
	[TestFixture]
	public class SegmentsViewModelTests
	{
		[Test]
		public void Segments()
		{
			DispatcherHelper.Initialize();
			var segmentPool = new SegmentPool();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			var analysisService = new AnalysisService(segmentPool, projectService, dialogService, busyService);
			var exportService = Substitute.For<IExportService>();

			WordsViewModel.Factory wordsFactory = words => new WordsViewModel(busyService, words);
			WordViewModel.Factory wordFactory = word => new WordViewModel(busyService, analysisService, word);

			var segments = new SegmentsViewModel(projectService, dialogService, busyService, exportService, wordsFactory, wordFactory);

			CogProject project = TestHelpers.GetTestProject(segmentPool);
			project.Meanings.AddRange(new[] {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3")});
			project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2")});
			project.Varieties[0].Words.AddRange(new[] {new Word("hɛ.loʊ", project.Meanings[0]), new Word("gʊd", project.Meanings[1]), new Word("bæd", project.Meanings[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("hɛlp", project.Meanings[0]), new Word("gu.gəl", project.Meanings[1]), new Word("gu.fi", project.Meanings[2])});
			projectService.Project.Returns(project);
			analysisService.SegmentAll();
			projectService.ProjectOpened += Raise.Event();

			Assert.That(segments.HasSegments, Is.True);
			Assert.That(segments.Segments.Select(s => s.StrRep), Is.EqualTo(new[] {"b", "f", "l", "g", "h"}));
			Assert.That(segments.Categories.Select(c => c.Name), Is.EqualTo(new[] {"Labial", "Coronal", "Dorsal", "Guttural"}));
			Assert.That(segments.Categories[0].Segments, Is.EquivalentTo(new[] {segments.Segments[0], segments.Segments[1]}));
			Assert.That(segments.Categories[1].Segments, Is.EquivalentTo(new[] {segments.Segments[2]}));
			Assert.That(segments.Categories[2].Segments, Is.EquivalentTo(new[] {segments.Segments[3]}));
			Assert.That(segments.Categories[3].Segments, Is.EquivalentTo(new[] {segments.Segments[4]}));

			project.Varieties[0].Words.RemoveAll(project.Meanings[0]);
			analysisService.Segment(project.Varieties[0]);
			Messenger.Default.Send(new DomainModelChangedMessage(true));
			Assert.That(segments.HasSegments, Is.True);
			Assert.That(segments.Segments.Select(s => s.StrRep), Is.EqualTo(new[] {"b", "f", "g", "h"}));
			Assert.That(segments.Categories.Select(c => c.Name), Is.EqualTo(new[] {"Labial", "Dorsal", "Guttural"}));
			Assert.That(segments.Categories[0].Segments, Is.EquivalentTo(new[] {segments.Segments[0], segments.Segments[1]}));
			Assert.That(segments.Categories[1].Segments, Is.EquivalentTo(new[] {segments.Segments[2]}));
			Assert.That(segments.Categories[2].Segments, Is.EquivalentTo(new[] {segments.Segments[3]}));

			segments.SyllablePosition = SyllablePosition.Nucleus;
			Assert.That(segments.HasSegments, Is.True);
			Assert.That(segments.Segments.Select(s => s.StrRep), Is.EqualTo(new[] {"i", "u", "ʊ", "ɛ", "ə", "æ"}));
			Assert.That(segments.Categories.Select(c => c.Name), Is.EqualTo(new[] {"Close", "Mid", "Open"}));
			Assert.That(segments.Categories[0].Segments, Is.EquivalentTo(new[] {segments.Segments[0], segments.Segments[1], segments.Segments[2]}));
			Assert.That(segments.Categories[1].Segments, Is.EquivalentTo(new[] {segments.Segments[3], segments.Segments[4]}));
			Assert.That(segments.Categories[2].Segments, Is.EquivalentTo(new[] {segments.Segments[5]}));

			foreach (Variety variety in project.Varieties)
				variety.Words.Clear();
			analysisService.SegmentAll();
			Messenger.Default.Send(new DomainModelChangedMessage(true));
			Assert.That(segments.HasSegments, Is.False);
			Assert.That(segments.Segments, Is.Empty);
			Assert.That(segments.Categories, Is.Empty);
		}

		[Test]
		public void ObservedWords()
		{
			DispatcherHelper.Initialize();
			var segmentPool = new SegmentPool();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			var analysisService = new AnalysisService(segmentPool, projectService, dialogService, busyService);
			var exportService = Substitute.For<IExportService>();

			WordsViewModel.Factory wordsFactory = words => new WordsViewModel(busyService, words);
			WordViewModel.Factory wordFactory = word => new WordViewModel(busyService, analysisService, word);

			var segments = new SegmentsViewModel(projectService, dialogService, busyService, exportService, wordsFactory, wordFactory);

			CogProject project = TestHelpers.GetTestProject(segmentPool);
			project.Meanings.AddRange(new[] {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3")});
			project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2")});
			project.Varieties[0].Words.AddRange(new[] {new Word("hɛ.loʊ", project.Meanings[0]), new Word("gʊd", project.Meanings[1]), new Word("bæd", project.Meanings[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("hɛlp", project.Meanings[0]), new Word("gu.gəl", project.Meanings[1]), new Word("gu.fi", project.Meanings[2])});
			projectService.Project.Returns(project);
			analysisService.SegmentAll();
			projectService.ProjectOpened += Raise.Event();

			var observedWords = segments.ObservedWords;
			observedWords.WordsView = new ListCollectionView(segments.ObservedWords.Words);

			Assert.That(observedWords.WordsView, Is.Empty);

			segments.SelectedSegment = segments.Varieties[1].Segments[3];
			WordViewModel[] wordsViewArray = observedWords.WordsView.Cast<WordViewModel>().ToArray();
			Assert.That(wordsViewArray.Select(w => w.StrRep), Is.EqualTo(new[] {"gu.gəl", "gu.fi"}));
			Assert.That(wordsViewArray[0].Segments[1].IsSelected, Is.True);
			Assert.That(wordsViewArray[0].Segments[2].IsSelected, Is.False);
			Assert.That(wordsViewArray[0].Segments[4].IsSelected, Is.True);

			segments.SelectedSegment = segments.Varieties[0].Segments[1];
			Assert.That(observedWords.WordsView, Is.Empty);

			segments.SelectedSegment = null;
			Assert.That(observedWords.WordsView, Is.Empty);
		}

		[Test]
		public void FindCommand()
		{
			DispatcherHelper.Initialize();
			var segmentPool = new SegmentPool();
			var projectService = Substitute.For<IProjectService>();
			var dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			var analysisService = new AnalysisService(segmentPool, projectService, dialogService, busyService);
			var exportService = Substitute.For<IExportService>();

			WordsViewModel.Factory wordsFactory = words => new WordsViewModel(busyService, words);
			WordViewModel.Factory wordFactory = word => new WordViewModel(busyService, analysisService, word);

			var segments = new SegmentsViewModel(projectService, dialogService, busyService, exportService, wordsFactory, wordFactory);

			CogProject project = TestHelpers.GetTestProject(segmentPool);
			project.Meanings.AddRange(new[] {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3")});
			project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2")});
			project.Varieties[0].Words.AddRange(new[] {new Word("hɛ.loʊ", project.Meanings[0]), new Word("gʊd", project.Meanings[1]), new Word("bæd", project.Meanings[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("hɛlp", project.Meanings[0]), new Word("gu.gəl", project.Meanings[1]), new Word("gu.fi", project.Meanings[2])});
			projectService.Project.Returns(project);
			analysisService.SegmentAll();
			projectService.ProjectOpened += Raise.Event();

			WordsViewModel observedWords = segments.ObservedWords;
			observedWords.WordsView = new ListCollectionView(observedWords.Words);

			FindViewModel findViewModel = null;
			Action closeCallback = null;
			dialogService.ShowModelessDialog(segments, Arg.Do<FindViewModel>(vm => findViewModel = vm), Arg.Do<Action>(callback => closeCallback = callback));
			segments.FindCommand.Execute(null);
			Assert.That(findViewModel, Is.Not.Null);
			Assert.That(closeCallback, Is.Not.Null);

			// already open, shouldn't get opened twice
			dialogService.ClearReceivedCalls();
			segments.FindCommand.Execute(null);
			dialogService.DidNotReceive().ShowModelessDialog(segments, Arg.Any<FindViewModel>(), Arg.Any<Action>());

			// nothing selected, no match
			findViewModel.Field = FindField.Form;
			findViewModel.String = "nothing";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWords.SelectedWords, Is.Empty);

			// nothing selected, matches
			segments.SelectedSegment = segments.Varieties[1].Segments[3];
			WordViewModel[] wordsViewArray = observedWords.WordsView.Cast<WordViewModel>().ToArray();
			findViewModel.String = "fi";
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWords.SelectedWords, Is.EquivalentTo(wordsViewArray[1].ToEnumerable()));
			findViewModel.FindNextCommand.Execute(null);
			Assert.That(observedWords.SelectedWords, Is.EquivalentTo(wordsViewArray[1].ToEnumerable()));
		}
	}
}
