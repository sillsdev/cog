using System;
using System.Windows.Data;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using NSubstitute;
using SIL.Cog.Application.Services;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Tests.ViewModels
{
	internal class WordListsViewModelTestEnvironment : IDisposable
	{
		private readonly WordListsViewModel _wordListsViewModel;
		private readonly IProjectService _projectService;
		private readonly IDialogService _dialogService;
		private readonly IAnalysisService _analysisService;
		private FindViewModel _findViewModel;

		public WordListsViewModelTestEnvironment()
		{
			DispatcherHelper.Initialize();
			_projectService = Substitute.For<IProjectService>();
			_dialogService = Substitute.For<IDialogService>();
			var busyService = Substitute.For<IBusyService>();
			_analysisService = Substitute.For<IAnalysisService>();
			var importService = Substitute.For<IImportService>();
			var exportService = Substitute.For<IExportService>();

			WordViewModel.Factory wordFactory = word => new WordViewModel(busyService, _analysisService, word);
			WordListsVarietyMeaningViewModel.Factory varietyMeaningFactory = (variety, meaning) => new WordListsVarietyMeaningViewModel(busyService, _analysisService, wordFactory, variety, meaning);
			WordListsVarietyViewModel.Factory varietyFactory = (parent, variety) => new WordListsVarietyViewModel(_projectService, varietyMeaningFactory, parent, variety);

			_wordListsViewModel = new WordListsViewModel(_projectService, _dialogService, importService, exportService, _analysisService, varietyFactory);
		}

		public WordListsViewModel WordListsViewModel
		{
			get { return _wordListsViewModel; }
		}

		public IDialogService DialogService
		{
			get { return _dialogService; }
		}

		public IAnalysisService AnalysisService
		{
			get { return _analysisService; }
		}

		public void OpenProject(CogProject project)
		{
			_projectService.Project.Returns(project);
			_projectService.ProjectOpened += Raise.Event();
			_wordListsViewModel.VarietiesView = new ListCollectionView(_wordListsViewModel.Varieties);
		}

		public void OpenFindDialog()
		{
			_dialogService.ShowModelessDialog(_wordListsViewModel, Arg.Do<FindViewModel>(vm => _findViewModel = vm), Arg.Any<Action>());
			_wordListsViewModel.FindCommand.Execute(null);
		}

		public FindViewModel FindViewModel
		{
			get { return _findViewModel; }
		}

		public void Dispose()
		{
			Messenger.Reset();
		}
	}
}