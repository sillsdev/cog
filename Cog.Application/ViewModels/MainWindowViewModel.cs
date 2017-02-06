using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Application.Services;

namespace SIL.Cog.Application.ViewModels
{
	public class MainWindowViewModel : ContainerViewModelBase
	{
		private ICommand _findCommand;

		private readonly IDialogService _dialogService;
		private readonly IImportService _importService;
		private readonly IExportService _exportService;
		private readonly IImageExportService _imageExportService;
		private readonly IProjectService _projectService;
		private readonly IAnalysisService _analysisService;

		public MainWindowViewModel(IProjectService projectService, IDialogService dialogService, IImportService importService, IExportService exportService,
			IImageExportService imageExportService, IAnalysisService analysisService, InputViewModel input, CompareViewModel compare, AnalyzeViewModel analyze)
			: base("Cog", input, compare, analyze)
		{
			_dialogService = dialogService;
			_importService = importService;
			_exportService = exportService;
			_imageExportService = imageExportService;
			_projectService = projectService;
			_analysisService = analysisService;

			NewCommand = new RelayCommand(New);
			OpenCommand = new RelayCommand(Open);
			SaveCommand = new RelayCommand(Save, CanSave);
			SaveAsCommand = new RelayCommand(SaveAs);
			ImportWordListsFromFileCommand = new RelayCommand(ImportWordListsFromFile);
			ImportWordListsFromClipboardCommand = new RelayCommand(ImportWordListsFromClipboard, CanImportWordListsFromClipboard);
			ImportGeographicRegionsCommand = new RelayCommand(ImportGeographicRegions);
			ExportWordListsCommand = new RelayCommand(ExportWordLists, CanExportWordLists);
			ExportSimilarityMatrixCommand = new RelayCommand(ExportSimilarityMatrix, CanExportSimilarityMatrix);
			ExportCognateSetsCommand = new RelayCommand(ExportCognateSets, CanExportCognateSets);
			ExportSegmentFrequenciesCommand = new RelayCommand(ExportSegmentFrequencies, CanExportSegmentFrequencies);
			ExportHierarchicalGraphCommand = new RelayCommand(ExportHierarchicalGraph, CanExportHierarchicalGraph);
			ExportNetworkGraphCommand = new RelayCommand(ExportNetworkGraph, CanExportNetworkGraph);
			ExportGlobalCorrespondencesChartCommand = new RelayCommand(ExportGlobalCorrespondencesChart, CanExportGlobalCorrespondencesChart);
			PerformComparisonCommand = new RelayCommand(PerformComparison, CanPerformComparison);
			RunStemmerCommand = new RelayCommand(RunStemmer, CanRunStemmer);
			AboutCommand = new RelayCommand(ShowAbout);
			ShowTutorialCommand = new RelayCommand(() => Process.Start("https://github.com/sillsdev/cog/wiki/Cog-Tutorial"));
			ShowGettingStartedCommand = new RelayCommand(ShowGettingStarted);

			foreach (ContainerViewModelBase childView in Views.OfType<ContainerViewModelBase>())
				childView.PropertyChanging += childView_PropertyChanging;

			PropertyChanging += OnPropertyChanging;

			ICommand nullCommand = new RelayCommand(() => {}, () => false);
			_findCommand = nullCommand;
			Messenger.Default.Register<SwitchViewMessage>(this, HandleSwitchView);
			Messenger.Default.Register<HookFindMessage>(this, msg => FindCommand = msg.FindCommand ?? nullCommand);
		}

		public bool Init()
		{
			if (_projectService.Init())
			{
				DisplayName = $"{_projectService.ProjectName} - Cog";
				SelectedView = Views[0];
				return true;
			}

			return false;
		}

		private void HandleSwitchView(SwitchViewMessage msg)
		{
			SwitchView(msg.ViewModelType);
		}

		private void OnPropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "SelectedView":
					CheckSettingsWorkspace(SelectedView);
					break;
			}
		}

		private void childView_PropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "SelectedView":
					CheckSettingsWorkspace(sender);
					break;
			}
		}

		private void CheckSettingsWorkspace(object view)
		{
			var childView = view as ContainerViewModelBase;
			var settingsWorkspace = childView?.SelectedView as SettingsWorkspaceViewModelBase;
			if (settingsWorkspace != null && settingsWorkspace.IsDirty)
			{
				if (_dialogService.ShowYesNoQuestion(this, "Do you wish to apply the current settings?", "Cog"))
					settingsWorkspace.Apply();
				else
					settingsWorkspace.Reset();
			}
		}

		private void New()
		{
			CheckSettingsWorkspace(SelectedView);
			if (_projectService.New(this))
			{
				DisplayName = $"{_projectService.ProjectName} - Cog";
				SwitchView(typeof(WordListsViewModel));
			}
		}

		private void Open()
		{
			CheckSettingsWorkspace(SelectedView);
			if (_projectService.Open(this))
			{
				DisplayName = $"{_projectService.ProjectName} - Cog";
				SwitchView(typeof(WordListsViewModel));
			}
		}

		private bool CanSave()
		{
			return _projectService.IsChanged;
		}

		private void Save()
		{
			CheckSettingsWorkspace(SelectedView);
			if (_projectService.Save(this))
				DisplayName = $"{_projectService.ProjectName} - Cog";
		}

		private void SaveAs()
		{
			CheckSettingsWorkspace(SelectedView);
			if (_projectService.SaveAs(this))
				DisplayName = $"{_projectService.ProjectName} - Cog";
		}

		private void ImportWordListsFromFile()
		{
			_importService.ImportWordListsFromFile(this);
		}

		private bool CanImportWordListsFromClipboard()
		{
			return _importService.CanImportWordListsFromClipboard();
		}

		private void ImportWordListsFromClipboard()
		{
			_importService.ImportWordListsFromClipboard(this);
		}

		private void ImportGeographicRegions()
		{
			_importService.ImportGeographicRegions(this);
		}

		private bool CanExportWordLists()
		{
			return _projectService.Project.Varieties.Count > 0 || _projectService.Project.Meanings.Count > 0;
		}

		private void ExportWordLists()
		{
			_exportService.ExportWordLists(this);
		}

		private bool CanExportSimilarityMatrix()
		{
			return _projectService.AreAllVarietiesCompared;
		}

		private void ExportSimilarityMatrix()
		{
			var vm = new ExportSimilarityMatrixViewModel();
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_exportService.ExportSimilarityMatrix(this, vm.SimilarityMetric);
		}

		private bool CanExportCognateSets()
		{
			return _projectService.AreAllVarietiesCompared;
		}

		private void ExportCognateSets()
		{
			_exportService.ExportCognateSets(this);
		}

		private bool CanExportSegmentFrequencies()
		{
			return _projectService.Project.Varieties.Count > 0 && _projectService.Project.Meanings.Count > 0;
		}

		private void ExportSegmentFrequencies()
		{
			var vm = new ExportSegmentFrequenciesViewModel();
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_exportService.ExportSegmentFrequencies(this, vm.SyllablePosition);
		}

		private bool CanExportHierarchicalGraph()
		{
			return _projectService.AreAllVarietiesCompared;
		}

		private void ExportHierarchicalGraph()
		{
			var vm = new ExportHierarchicalGraphViewModel();
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_imageExportService.ExportHierarchicalGraph(this, vm.GraphType, vm.ClusteringMethod, vm.SimilarityMetric);
		}

		private bool CanExportNetworkGraph()
		{
			return _projectService.AreAllVarietiesCompared;
		}

		private void ExportNetworkGraph()
		{
			var vm = new ExportNetworkGraphViewModel();
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_imageExportService.ExportNetworkGraph(this, vm.SimilarityMetric, vm.SimilarityScoreFilter);
		}

		private bool CanExportGlobalCorrespondencesChart()
		{
			return _projectService.AreAllVarietiesCompared;
		}

		private void ExportGlobalCorrespondencesChart()
		{
			var vm = new ExportGlobalCorrespondencesChartViewModel();
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_imageExportService.ExportGlobalCorrespondencesChart(this, vm.SyllablePosition, vm.FrequencyThreshold);
		}

		public bool CanExit()
		{
			CheckSettingsWorkspace(SelectedView);
			return _projectService.Close(this);
		}

		private bool CanPerformComparison()
		{
			return _projectService.Project.Varieties.Count > 0 && _projectService.Project.Meanings.Count > 0;
		}

		private void PerformComparison()
		{
			_analysisService.CompareAll(this);
		}

		private bool CanRunStemmer()
		{
			return _projectService.Project.Varieties.Count > 0 && _projectService.Project.Meanings.Count > 0;
		}

		private void RunStemmer()
		{
			var vm = new RunStemmerViewModel(true);
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_analysisService.StemAll(this, vm.Method);
		}

		private void ShowAbout()
		{
			_dialogService.ShowModalDialog(this, new AboutViewModel());
		}

		private void ShowGettingStarted()
		{
			string exeDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			if (!string.IsNullOrEmpty(exeDir))
				Process.Start(Path.Combine(exeDir, "Help", "GettingStartedWithCog.pdf"));
		}

		public ICommand NewCommand { get; }
		public ICommand OpenCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand SaveAsCommand { get; }
		public ICommand ImportWordListsFromFileCommand { get; }
		public ICommand ImportWordListsFromClipboardCommand { get; }
		public ICommand ImportGeographicRegionsCommand { get; }
		public ICommand ExportWordListsCommand { get; }
		public ICommand ExportSimilarityMatrixCommand { get; }
		public ICommand ExportCognateSetsCommand { get; }
		public ICommand ExportSegmentFrequenciesCommand { get; }
		public ICommand ExportHierarchicalGraphCommand { get; }
		public ICommand ExportNetworkGraphCommand { get; }
		public ICommand ExportGlobalCorrespondencesChartCommand { get; }
		public ICommand PerformComparisonCommand { get; }
		public ICommand RunStemmerCommand { get; }
		public ICommand FindCommand
		{
			get { return _findCommand; }
			private set { Set(() => FindCommand, ref _findCommand, value); }
		}
		public ICommand AboutCommand { get; }
		public ICommand ShowTutorialCommand { get; }
		public ICommand ShowGettingStartedCommand { get; }
	}
}
