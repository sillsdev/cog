using System;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Application.Services;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Tests.ViewModels
{
	[TestFixture]
	public class MeaningsViewModelTests
	{
		private class TestEnvironment : IDisposable
		{
			private readonly IProjectService _projectService;
			private readonly IDialogService _dialogService;
			private readonly MeaningsViewModel _meaningsViewModel;

			public TestEnvironment()
			{
				DispatcherHelper.Initialize();
				_projectService = Substitute.For<IProjectService>();
				_dialogService = Substitute.For<IDialogService>();
				var busyService = Substitute.For<IBusyService>();

				_meaningsViewModel = new MeaningsViewModel(_projectService, _dialogService, busyService);
			}

			public void OpenProject(CogProject project)
			{
				_projectService.Project.Returns(project);
				_projectService.ProjectOpened += Raise.Event();
			}

			public IDialogService DialogService
			{
				get { return _dialogService; }
			}

			public MeaningsViewModel MeaningsViewModel
			{
				get { return _meaningsViewModel; }
			}

			public void Dispose()
			{
				Messenger.Reset();
			}
		}

		[Test]
		public void TaskAreas_EditMeaning_MeaningUpdated()
		{
			using (var env = new TestEnvironment())
			{
				var project = new CogProject()
				{
					Meanings = {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2")}
				};
				env.OpenProject(project);

				var commonTasks = (TaskAreaItemsViewModel) env.MeaningsViewModel.TaskAreas[0];
				var renameVariety = (TaskAreaCommandViewModel) commonTasks.Items[1];
				env.MeaningsViewModel.SelectedMeaning = env.MeaningsViewModel.Meanings.First(v => v.Gloss == "gloss2");
				env.DialogService.ShowModalDialog(env.MeaningsViewModel, Arg.Do<EditMeaningViewModel>(vm => vm.Gloss = "gloss3")).Returns(true);
				renameVariety.Command.Execute(null);
				Assert.That(env.MeaningsViewModel.SelectedMeaning.Gloss, Is.EqualTo("gloss3"));
				Assert.That(env.MeaningsViewModel.Meanings.Select(v => v.Gloss), Is.EqualTo(new[] {"gloss1", "gloss3"}));
				Assert.That(project.Meanings.Contains("gloss2"), Is.False);
			}
		}
	}
}
