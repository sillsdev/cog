using System;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Application.Services;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.Cog.TestUtils;

namespace SIL.Cog.Application.Tests.ViewModels
{
	[TestFixture]
	public class WordPairViewModelTests
	{
		private class TestEnvironment : IDisposable
		{
			private readonly WordPairViewModel _wordPairViewModel;
			private readonly CogProject _project;

			public TestEnvironment(bool? actualCognacy, bool predictedCognacy)
			{
				DispatcherHelper.Initialize();

				var segmentPool = new SegmentPool();
				var projectService = Substitute.For<IProjectService>();
				var analysisService = Substitute.For<IAnalysisService>();
				_project = TestHelpers.GetTestProject(segmentPool);
				_project.Varieties.AddRange(new[] {new Variety("variety1"), new Variety("variety2")});
				_project.Meanings.Add(new Meaning("meaning1", null));
				var word1 = new Word("wɜrd", _project.Meanings[0]);
				_project.Varieties[0].Words.Add(word1);
				_project.Segmenter.Segment(word1);
				var word2 = new Word("kɑr", _project.Meanings[0]);
				_project.Varieties[1].Words.Add(word2);
				_project.Segmenter.Segment(word2);

				var vp = new VarietyPair(_project.Varieties[0], _project.Varieties[1]);
				if (actualCognacy != null)
					_project.CognacyDecisions.Add(new CognacyDecision(vp.Variety1, vp.Variety2, _project.Meanings[0], (bool) actualCognacy));

				_project.VarietyPairs.Add(vp);
				var wp = new WordPair(word1, word2) {PredictedCognacy = predictedCognacy, ActualCognacy = actualCognacy};
				_project.VarietyPairs[0].WordPairs.Add(wp);

				projectService.Project.Returns(_project);
				_wordPairViewModel = new WordPairViewModel(projectService, analysisService, wp, true);
			}

			public WordPairViewModel WordPairViewModel
			{
				get { return _wordPairViewModel; }
			}

			public CogProject Project
			{
				get { return _project; }
			}

			public void Dispose()
			{
				Messenger.Reset();
			}
		}

		[Test]
		public void PinUnpinCommand_UnpinnedCognate_PinnedNoncognate()
		{
			using (var env = new TestEnvironment(null, true))
			{
				Assert.That(env.Project.CognacyDecisions, Is.Empty);
				Assert.That(env.WordPairViewModel.PinUnpinText, Is.EqualTo("Pin to non-cognates"));
				env.WordPairViewModel.PinUnpinCommand.Execute(null);
				VarietyPair vp = env.WordPairViewModel.DomainWordPair.VarietyPair;
				Assert.That(env.Project.CognacyDecisions, Is.EquivalentTo(new[] {new CognacyDecision(vp.Variety1, vp.Variety2, env.WordPairViewModel.DomainWordPair.Meaning, false)}));
			}
		}

		[Test]
		public void PinUnpinCommand_UnpinnedNoncognate_PinnedCognate()
		{
			using (var env = new TestEnvironment(null, false))
			{
				Assert.That(env.Project.CognacyDecisions, Is.Empty);
				Assert.That(env.WordPairViewModel.PinUnpinText, Is.EqualTo("Pin to cognates"));
				env.WordPairViewModel.PinUnpinCommand.Execute(null);
				VarietyPair vp = env.WordPairViewModel.DomainWordPair.VarietyPair;
				Assert.That(env.Project.CognacyDecisions, Is.EquivalentTo(new[] {new CognacyDecision(vp.Variety1, vp.Variety2, env.WordPairViewModel.DomainWordPair.Meaning, true)}));
			}
		}

		[Test]
		public void PinUnpinCommand_PinnedNoncognate_UnpinnedCognate()
		{
			using (var env = new TestEnvironment(false, true))
			{
				VarietyPair vp = env.WordPairViewModel.DomainWordPair.VarietyPair;
				Assert.That(env.Project.CognacyDecisions, Is.EquivalentTo(new[] {new CognacyDecision(vp.Variety1, vp.Variety2, env.WordPairViewModel.DomainWordPair.Meaning, false)}));
				Assert.That(env.WordPairViewModel.PinUnpinText, Is.EqualTo("Unpin"));
				env.WordPairViewModel.PinUnpinCommand.Execute(null);
				Assert.That(env.Project.CognacyDecisions, Is.Empty);
			}
		}

		[Test]
		public void PinUnpinCommand_PinnedCognate_UnpinnedNoncognate()
		{
			using (var env = new TestEnvironment(true, false))
			{
				VarietyPair vp = env.WordPairViewModel.DomainWordPair.VarietyPair;
				Assert.That(env.Project.CognacyDecisions, Is.EquivalentTo(new[] {new CognacyDecision(vp.Variety1, vp.Variety2, env.WordPairViewModel.DomainWordPair.Meaning, true)}));
				Assert.That(env.WordPairViewModel.PinUnpinText, Is.EqualTo("Unpin"));
				env.WordPairViewModel.PinUnpinCommand.Execute(null);
				Assert.That(env.Project.CognacyDecisions, Is.Empty);
			}
		}
	}
}
