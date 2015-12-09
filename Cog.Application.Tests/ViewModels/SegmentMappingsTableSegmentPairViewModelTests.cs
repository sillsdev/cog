using System;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Application.Services;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Application.Tests.ViewModels
{
	[TestFixture]
	public class SegmentMappingsTableSegmentPairViewModelTests
	{
		[Test]
		public void ToggleMappingCommand_MeetsThreshold_DefiniteListMapped()
		{
			using (var env = new TestEnvironment())
			{
				env.SegmentPair.MeetsThreshold = true;
				Assert.That(env.SegmentPair.MappingState, Is.EqualTo(SegmentMappingState.ThresholdMapped));
				env.SegmentPair.ToggleMappingCommand.Execute(null);
				Assert.That(env.SegmentPair.MappingState, Is.EqualTo(SegmentMappingState.DefiniteListMapped));
				Assert.That(env.SegmentPair.Mappings.Mappings.Count, Is.EqualTo(1));
				Assert.That(env.SegmentPair.Mappings.Mappings[0].Segment1, Is.EqualTo("b"));
				Assert.That(env.SegmentPair.Mappings.Mappings[0].Segment2, Is.EqualTo("c"));
			}
		}

		[Test]
		public void ToggleMappingCommand_MeetsThresholdHasMapping_ThresholdMapped()
		{
			using (var env = new TestEnvironment())
			{
				env.SegmentPair.MeetsThreshold = true;
				env.SegmentPair.Mappings.Mappings.Add(new SegmentMappingViewModel(env.ProjectService, "b", "c"));
				Assert.That(env.SegmentPair.MappingState, Is.EqualTo(SegmentMappingState.DefiniteListMapped));
				env.SegmentPair.ToggleMappingCommand.Execute(null);
				Assert.That(env.SegmentPair.MappingState, Is.EqualTo(SegmentMappingState.ThresholdMapped));
				Assert.That(env.SegmentPair.Mappings.Mappings, Is.Empty);
			}
		}

		[Test]
		public void ToggleMappingCommand_DoesNotMeetThresholdHasMapping_Unmapped()
		{
			using (var env = new TestEnvironment())
			{
				env.SegmentPair.MeetsThreshold = false;
				env.SegmentPair.Mappings.Mappings.Add(new SegmentMappingViewModel(env.ProjectService, "b", "c"));
				Assert.That(env.SegmentPair.MappingState, Is.EqualTo(SegmentMappingState.DefiniteListMapped));
				env.SegmentPair.ToggleMappingCommand.Execute(null);
				Assert.That(env.SegmentPair.MappingState, Is.EqualTo(SegmentMappingState.Unmapped));
				Assert.That(env.SegmentPair.Mappings.Mappings, Is.Empty);
			}
		}

		[Test]
		public void ToggleMappingCommand_DoesNotMeetThresholdNoMappings_DefiniteListMapped()
		{
			using (var env = new TestEnvironment())
			{
				env.SegmentPair.MeetsThreshold = false;
				Assert.That(env.SegmentPair.MappingState, Is.EqualTo(SegmentMappingState.Unmapped));
				env.SegmentPair.ToggleMappingCommand.Execute(null);
				Assert.That(env.SegmentPair.MappingState, Is.EqualTo(SegmentMappingState.DefiniteListMapped));
				Assert.That(env.SegmentPair.Mappings.Mappings.Count, Is.EqualTo(1));
				Assert.That(env.SegmentPair.Mappings.Mappings[0].Segment1, Is.EqualTo("b"));
				Assert.That(env.SegmentPair.Mappings.Mappings[0].Segment2, Is.EqualTo("c"));
			}
		}

		[Test]
		public void ToggleMappingCommand_DoesNotMeetThresholdHasIndefiniteMapping_Unmapped()
		{
			using (var env = new TestEnvironment())
			{
				env.SegmentPair.MeetsThreshold = false;
				env.SegmentPair.Mappings.Mappings.Add(new SegmentMappingViewModel(env.ProjectService, "#b", "c"));
				Assert.That(env.SegmentPair.MappingState, Is.EqualTo(SegmentMappingState.IndefiniteListMapped));
				env.SegmentPair.ToggleMappingCommand.Execute(null);
				Assert.That(env.SegmentPair.MappingState, Is.EqualTo(SegmentMappingState.Unmapped));
				Assert.That(env.SegmentPair.Mappings.Mappings, Is.Empty);
			}
		}

		[Test]
		public void ToggleMappingCommand_MeetsThresholdHasIndefiniteMapping_ThresholdMapped()
		{
			using (var env = new TestEnvironment())
			{
				env.SegmentPair.MeetsThreshold = true;
				env.SegmentPair.Mappings.Mappings.Add(new SegmentMappingViewModel(env.ProjectService, "#b", "c"));
				Assert.That(env.SegmentPair.MappingState, Is.EqualTo(SegmentMappingState.IndefiniteListMapped));
				env.SegmentPair.ToggleMappingCommand.Execute(null);
				Assert.That(env.SegmentPair.MappingState, Is.EqualTo(SegmentMappingState.ThresholdMapped));
				Assert.That(env.SegmentPair.Mappings.Mappings, Is.Empty);
			}
		}

		private class TestEnvironment : IDisposable
		{
			private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();
			private readonly SegmentMappingsTableSegmentPairViewModel _segmentPair;
			private readonly IProjectService _projectService;

			public TestEnvironment()
			{
				DispatcherHelper.Initialize();
				_projectService = Substitute.For<IProjectService>();
				var dialogService = Substitute.For<IDialogService>();
				var importService = Substitute.For<IImportService>();

				SegmentMappingViewModel.Factory mappingFactory = (segment1, segment2) => new SegmentMappingViewModel(_projectService, segment1, segment2);
				NewSegmentMappingViewModel.Factory newMappingFactory = () => new NewSegmentMappingViewModel(_projectService);

				var segmentMappings = new SegmentMappingsViewModel(dialogService, importService, mappingFactory, newMappingFactory);
				_segmentPair = new SegmentMappingsTableSegmentPairViewModel(segmentMappings, mappingFactory,
					new SegmentMappingsTableSegmentViewModel(new Segment(FeatureStruct.New().Symbol(CogFeatureSystem.ConsonantType).Feature(CogFeatureSystem.StrRep).EqualTo("b").Value), SoundType.Consonant),
					new SegmentMappingsTableSegmentViewModel(new Segment(FeatureStruct.New().Symbol(CogFeatureSystem.ConsonantType).Feature(CogFeatureSystem.StrRep).EqualTo("c").Value), SoundType.Consonant),
					100, true);

				var project = new CogProject(_spanFactory);
				_projectService.Project.Returns(project);
			}

			public IProjectService ProjectService
			{
				get { return _projectService; }
			}

			public SegmentMappingsTableSegmentPairViewModel SegmentPair
			{
				get { return _segmentPair; }
			}

			public void Dispose()
			{
				Messenger.Reset();
			}
		}
	}
}
