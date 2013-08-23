using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;

namespace SIL.Cog.Applications.ViewModels
{
	public class MainWindowViewModel : MasterViewModelBase
	{
		private readonly ICommand _newCommand;
		private readonly ICommand _openCommand;
		private readonly ICommand _saveCommand;
		private readonly ICommand _saveAsCommand;
		private readonly ICommand _importWordListsCommand;
		private readonly ICommand _importGeographicRegionsCommand; 
		private readonly ICommand _exportWordListsCommand;
		private readonly ICommand _exportSimilarityMatrixCommand;
		private readonly ICommand _exportCognateSetsCommand;
		private readonly ICommand _exportSegmentFrequenciesCommand;
		private readonly ICommand _exportHierarchicalGraphCommand;
		private readonly ICommand _exportNetworkGraphCommand;
		private readonly ICommand _exportGlobalCorrespondencesChartCommand;

		private readonly IDialogService _dialogService;
		private readonly IImportService _importService;
		private readonly IExportService _exportService;
		private readonly IImageExportService _imageExportService;
		private readonly IProjectService _projectService;

		public MainWindowViewModel(IProjectService projectService, IDialogService dialogService, IImportService importService, IExportService exportService,
			IImageExportService imageExportService, InputMasterViewModel inputMasterViewModel, CompareMasterViewModel compareMasterViewModel, AnalyzeMasterViewModel analyzeMasterViewModel)
			: base("Cog", inputMasterViewModel, compareMasterViewModel, analyzeMasterViewModel)
		{
			_dialogService = dialogService;
			_importService = importService;
			_exportService = exportService;
			_imageExportService = imageExportService;
			_projectService = projectService;

			_newCommand = new RelayCommand(New);
			_openCommand = new RelayCommand(Open);
			_saveCommand = new RelayCommand(Save, CanSave);
			_saveAsCommand = new RelayCommand(SaveAs);
			_importWordListsCommand = new RelayCommand(ImportWordLists);
			_importGeographicRegionsCommand = new RelayCommand(ImportGeographicRegions);
			_exportWordListsCommand = new RelayCommand(ExportWordLists, CanExportWordLists);
			_exportSimilarityMatrixCommand = new RelayCommand(ExportSimilarityMatrix, CanExportSimilarityMatrix);
			_exportCognateSetsCommand = new RelayCommand(ExportCognateSets, CanExportCognateSets);
			_exportSegmentFrequenciesCommand = new RelayCommand(ExportSegmentFrequencies);
			_exportHierarchicalGraphCommand = new RelayCommand(ExportHierarchicalGraph, CanExportHierarchicalGraph);
			_exportNetworkGraphCommand = new RelayCommand(ExportNetworkGraph, CanExportNetworkGraph);
			_exportGlobalCorrespondencesChartCommand = new RelayCommand(ExportGlobalCorrespondencesChart, CanExportGlobalCorrespondencesChart);

			foreach (MasterViewModelBase childView in Views.OfType<MasterViewModelBase>())
				childView.PropertyChanging += childView_PropertyChanging;

			PropertyChanging += OnPropertyChanging;

			Messenger.Default.Register<SwitchViewMessage>(this, HandleSwitchView);

			_projectService.Init();
			DisplayName = string.Format("{0} - Cog", _projectService.ProjectName);
		}

		private void HandleSwitchView(SwitchViewMessage msg)
		{
			SwitchView(msg.ViewModelType);
		}

		private void OnPropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "CurrentView":
					CheckSettingsWorkspace(CurrentView);
					break;
			}
		}

		private void childView_PropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "CurrentView":
					CheckSettingsWorkspace(sender);
					break;
			}
		}

		private void CheckSettingsWorkspace(object view)
		{
			var childView = view as MasterViewModelBase;
			if (childView == null)
				return;

			var settingsWorkspace = childView.CurrentView as SettingsWorkspaceViewModelBase;
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
			CheckSettingsWorkspace(CurrentView);
			if (_projectService.New())
			{
				DisplayName = string.Format("{0} - Cog", _projectService.ProjectName);
				SwitchView(typeof(WordListsViewModel));
			}
		}

		private void Open()
		{
			CheckSettingsWorkspace(CurrentView);
			if (_projectService.Open())
			{
				DisplayName = string.Format("{0} - Cog", _projectService.ProjectName);
				SwitchView(typeof(WordListsViewModel));
			}
		}

		private bool CanSave()
		{
			return _projectService.IsChanged;
		}

		private void Save()
		{
			CheckSettingsWorkspace(CurrentView);
			if (_projectService.Save())
				DisplayName = string.Format("{0} - Cog", _projectService.ProjectName);
		}

		private void SaveAs()
		{
			CheckSettingsWorkspace(CurrentView);
			if (_projectService.SaveAs())
				DisplayName = string.Format("{0} - Cog", _projectService.ProjectName);
		}

		private void ImportWordLists()
		{
			_importService.ImportWordLists(this);
		}

		private void ImportGeographicRegions()
		{
			_importService.ImportGeographicRegions(this);
		}

		private bool CanExportWordLists()
		{
			return _projectService.Project.Varieties.Count > 0 || _projectService.Project.Senses.Count > 0;
		}

		private void ExportWordLists()
		{
			_exportService.ExportWordLists(this);
		}

		private bool CanExportSimilarityMatrix()
		{
			return _projectService.Project.VarietyPairs.Count > 0;
		}

		private void ExportSimilarityMatrix()
		{
			var vm = new ExportSimilarityMatrixViewModel();
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_exportService.ExportSimilarityMatrix(this, vm.SimilarityMetric);
		}

		private bool CanExportCognateSets()
		{
			return _projectService.Project.VarietyPairs.Count > 0;
		}

		private void ExportCognateSets()
		{
			_exportService.ExportCognateSets(this);
		}

		private void ExportSegmentFrequencies()
		{
			var vm = new ExportSegmentFrequenciesViewModel();
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_exportService.ExportSegmentFrequencies(this, vm.SyllablePosition);
		}

		private bool CanExportHierarchicalGraph()
		{
			return _projectService.Project.VarietyPairs.Count > 0;
		}

		private void ExportHierarchicalGraph()
		{
			var vm = new ExportHierarchicalGraphViewModel();
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_imageExportService.ExportHierarchicalGraph(this, vm.GraphType, vm.ClusteringMethod, vm.SimilarityMetric);
		}

		private bool CanExportNetworkGraph()
		{
			return _projectService.Project.VarietyPairs.Count > 0;
		}

		private void ExportNetworkGraph()
		{
			var vm = new ExportNetworkGraphViewModel();
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_imageExportService.ExportNetworkGraph(this, vm.SimilarityMetric, vm.SimilarityScoreFilter);
		}

		private bool CanExportGlobalCorrespondencesChart()
		{
			return _projectService.Project.VarietyPairs.Count > 0;
		}

		private void ExportGlobalCorrespondencesChart()
		{
			var vm = new ExportGlobalCorrespondencesChartViewModel();
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_imageExportService.ExportGlobalCorrespondencesChart(this, vm.SyllablePosition, vm.FrequencyFilter);
		}

		public bool CanExit()
		{
			CheckSettingsWorkspace(CurrentView);
			return _projectService.Close();
		}

		public ICommand NewCommand
		{
			get { return _newCommand; }
		}

		public ICommand OpenCommand
		{
			get { return _openCommand; }
		}

		public ICommand SaveCommand
		{
			get { return _saveCommand; }
		}

		public ICommand SaveAsCommand
		{
			get { return _saveAsCommand; }
		}

		public ICommand ImportWordListsCommand
		{
			get { return _importWordListsCommand; }
		}

		public ICommand ImportGeographicRegionsCommand
		{
			get { return _importGeographicRegionsCommand; }
		}

		public ICommand ExportWordListsCommand
		{
			get { return _exportWordListsCommand; }
		}

		public ICommand ExportSimilarityMatrixCommand
		{
			get { return _exportSimilarityMatrixCommand; }
		}

		public ICommand ExportCognateSetsCommand
		{
			get { return _exportCognateSetsCommand; }
		}

		public ICommand ExportSegmentFrequenciesCommand
		{
			get { return _exportSegmentFrequenciesCommand; }
		}

		public ICommand ExportHierarchicalGraphCommand
		{
			get { return _exportHierarchicalGraphCommand; }
		}

		public ICommand ExportNetworkGraphCommand
		{
			get { return _exportNetworkGraphCommand; }
		}

		public ICommand ExportGlobalCorrespondencesChartCommand
		{
			get { return _exportGlobalCorrespondencesChartCommand; }
		}
	}
}
