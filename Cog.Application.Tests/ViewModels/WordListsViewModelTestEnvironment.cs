using System;
using System.Collections.Generic;
using System.Windows.Data;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using NSubstitute;
using SIL.Cog.Application.Services;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.Tests.ViewModels
{
	internal class WordListsViewModelTestEnvironment : IDisposable
	{
		private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();
		private readonly WordListsViewModel _wordLists;
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

			_wordLists = new WordListsViewModel(_projectService, _dialogService, importService, exportService, _analysisService, varietyFactory);
		}

		public SpanFactory<ShapeNode> SpanFactory
		{
			get { return _spanFactory; }
		}

		public WordListsViewModel WordLists
		{
			get { return _wordLists; }
		}

		public IDialogService DialogService
		{
			get { return _dialogService; }
		}

		public IAnalysisService AnalysisService
		{
			get { return _analysisService; }
		}

		public CogProject OpenProject(IEnumerable<Meaning> meanings, IEnumerable<Variety> varieties)
		{
			var project = new CogProject(_spanFactory);
			project.Meanings.AddRange(meanings);
			project.Varieties.AddRange(varieties);
			_projectService.Project.Returns(project);
			_projectService.ProjectOpened += Raise.Event();
			_wordLists.VarietiesView = new ListCollectionView(_wordLists.Varieties);
			return project;
		}

		public void OpenFindDialog()
		{
			_dialogService.ShowModelessDialog(_wordLists, Arg.Do<FindViewModel>(vm => _findViewModel = vm), Arg.Do<Action>(callback => {}));
			_wordLists.FindCommand.Execute(null);
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