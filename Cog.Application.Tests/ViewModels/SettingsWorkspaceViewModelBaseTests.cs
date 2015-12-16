using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Application.Services;
using SIL.Cog.Application.ViewModels;

namespace SIL.Cog.Application.Tests.ViewModels
{
	[TestFixture]
	public class SettingsWorkspaceViewModelBaseTests
	{
		private class TestComponentSettingsViewModel : ComponentSettingsViewModelBase
		{
			public TestComponentSettingsViewModel()
				: base("Test Component", "test")
			{
			}

			public override void Setup()
			{
				IsChanged = true;
			}

			public void ForceChanged()
			{
				IsChanged = true;
			}

			public override object UpdateComponent()
			{
				IsUpdated = true;
				return null;
			}

			public bool IsUpdated { get; private set; }
		}

		private class TestSettingsWorkspaceViewModel : SettingsWorkspaceViewModelBase
		{
			public TestSettingsWorkspaceViewModel(IProjectService projectService, IBusyService busyService)
				: base("Test Workspace", projectService, busyService, new TestComponentSettingsViewModel())
			{
			}

			public TestComponentSettingsViewModel TestComponent
			{
				get { return (TestComponentSettingsViewModel) Components[0]; }
			}
		}

		[Test]
		public void Apply_ComponentChanged_IsChangedSetToFalse()
		{
			IProjectService projectService = Substitute.For<IProjectService>();
			IBusyService busyService = Substitute.For<IBusyService>();
			var workspace = new TestSettingsWorkspaceViewModel(projectService, busyService);
			workspace.TestComponent.ForceChanged();
			Assert.That(workspace.IsDirty, Is.True);
			workspace.Apply();
			Assert.That(workspace.IsDirty, Is.False);
			Assert.That(workspace.TestComponent.IsUpdated, Is.True);
			Assert.That(workspace.TestComponent.IsChanged, Is.False);
		}

		[Test]
		public void Reset_ComponentChanged_IsChangedSetToFalse()
		{
			IProjectService projectService = Substitute.For<IProjectService>();
			IBusyService busyService = Substitute.For<IBusyService>();
			var workspace = new TestSettingsWorkspaceViewModel(projectService, busyService);
			workspace.TestComponent.ForceChanged();
			Assert.That(workspace.IsDirty, Is.True);
			workspace.Reset();
			Assert.That(workspace.IsDirty, Is.False);
			Assert.That(workspace.TestComponent.IsUpdated, Is.False);
			Assert.That(workspace.TestComponent.IsChanged, Is.False);
		}

		[Test]
		public void ProjectOpened_ComponentChanged_WorkspaceReset()
		{
			IProjectService projectService = Substitute.For<IProjectService>();
			IBusyService busyService = Substitute.For<IBusyService>();
			var workspace = new TestSettingsWorkspaceViewModel(projectService, busyService);
			workspace.TestComponent.ForceChanged();
			Assert.That(workspace.IsDirty, Is.True);
			projectService.ProjectOpened += Raise.Event();
			Assert.That(workspace.IsDirty, Is.False);
			Assert.That(workspace.TestComponent.IsUpdated, Is.False);
			Assert.That(workspace.TestComponent.IsChanged, Is.False);
		}
	}
}
