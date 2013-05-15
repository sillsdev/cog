using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Config;
using SIL.Cog.Properties;
using SIL.Cog.Services;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class MainWindowViewModel : MasterViewModelBase
	{
		private static readonly FileType CogProjectFileType = new FileType("Cog Project", ".cogx");

		private readonly ICommand _newCommand;
		private readonly ICommand _openCommand;
		private readonly ICommand _saveCommand;
		private readonly ICommand _saveAsCommand;
		private readonly ICommand _importWordListsCommand;
		private readonly ICommand _importGeographicRegionsCommand; 
		private readonly ICommand _exportWordListsCommand;
		private readonly ICommand _exportSimilarityMatrixCommand;
		private readonly ICommand _exportCognateSetsCommand;
		private readonly ICommand _exportHierarchicalGraphCommand;
		private readonly ICommand _exportNetworkGraphCommand;
		private readonly ICommand _exitCommand;

		private readonly IDialogService _dialogService;
		private readonly IImportService _importService;
		private readonly IExportService _exportService;
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private CogProject _project;

		public MainWindowViewModel(SpanFactory<ShapeNode> spanFactory, IDialogService dialogService, IImportService importService, IExportService exportService,
			DataMasterViewModel dataMasterViewModel, ComparisonMasterViewModel comparisonMasterViewModel, VisualizationMasterViewModel visualizationMasterViewModel)
			: base("Cog", dataMasterViewModel, comparisonMasterViewModel, visualizationMasterViewModel)
		{
			_dialogService = dialogService;
			_importService = importService;
			_exportService = exportService;

			_spanFactory = spanFactory;

			_newCommand = new RelayCommand(New);
			_openCommand = new RelayCommand(Open);
			_saveCommand = new RelayCommand(Save, CanSave);
			_saveAsCommand = new RelayCommand(SaveAs);
			_importWordListsCommand = new RelayCommand(ImportWordLists);
			_importGeographicRegionsCommand = new RelayCommand(ImportGeographicRegions);
			_exportWordListsCommand = new RelayCommand(ExportWordLists, CanExportWordLists);
			_exportSimilarityMatrixCommand = new RelayCommand(ExportSimilarityMatrix, CanExportSimilarityMatrix);
			_exportCognateSetsCommand = new RelayCommand(ExportCognateSets, CanExportCognateSets);
			_exportHierarchicalGraphCommand = new RelayCommand(ExportHierarchicalGraph, CanExportHierarchicalGraph);
			_exportNetworkGraphCommand = new RelayCommand(ExportNetworkGraph, CanExportNetworkGraph);
			_exitCommand = new RelayCommand(Exit, CanExit);

			foreach (MasterViewModelBase childView in Views.OfType<MasterViewModelBase>())
				childView.PropertyChanging += childView_PropertyChanging;

			PropertyChanging += MainWindowViewModel_PropertyChanging;

			Messenger.Default.Register<SwitchViewMessage>(this, HandleSwitchView);

			string[] args = Environment.GetCommandLineArgs();
			if (args.Length > 1)
				ProjectFilePath = args[1];

			if (string.IsNullOrEmpty(ProjectFilePath) || !File.Exists(ProjectFilePath))
			{
				NewProject();
			}
			else
			{
				try
				{
					OpenProject(ProjectFilePath);
				}
				catch (ConfigException)
				{
					NewProject();
				}
			}
		}

		private void MainWindowViewModel_PropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "CurrentView":
					var childView = CurrentView as MasterViewModelBase;
					if (childView != null)
						CheckSettingsWorkspace(childView);
					break;
			}
		}

		private void childView_PropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "CurrentView":
					var childView = (MasterViewModelBase) sender;
					CheckSettingsWorkspace(childView);
					break;
			}
		}

		private void CheckSettingsWorkspace(MasterViewModelBase childView)
		{
			var settingsWorkspace = childView.CurrentView as SettingsWorkspaceViewModelBase;
			if (settingsWorkspace != null && settingsWorkspace.IsDirty)
			{
				if (_dialogService.ShowYesNoQuestion(this, "Do you wish to apply the current settings?", "Cog"))
					settingsWorkspace.Apply();
				else
					settingsWorkspace.Reset();
			}
		}

		private void HandleSwitchView(SwitchViewMessage message)
		{
			SwitchView(message.ViewModelType, message.Model);
		}

		private void New()
		{
			if (CanCloseProject())
				NewProject();
		}

		private void Open()
		{
			if (CanCloseProject())
			{
				FileDialogResult result = _dialogService.ShowOpenFileDialog(this, CogProjectFileType);
				if (result.IsValid)
				{
					try
					{
						OpenProject(result.FileName);
					}
					catch (ConfigException)
					{
						_dialogService.ShowError(this, "The specified file is not a valid Cog configuration file.", "Cog");
					}
				}
			}
		}

		private bool CanSave()
		{
			return IsChanged;
		}

		private void Save()
		{
			var childView = CurrentView as MasterViewModelBase;
			if (childView != null)
				CheckSettingsWorkspace(childView);
			if (string.IsNullOrEmpty(ProjectFilePath))
			{
				FileDialogResult result = _dialogService.ShowSaveFileDialog(this, CogProjectFileType);
				if (result.IsValid)
					SaveProject(result.FileName);
			}
			else
			{
				SaveProject(ProjectFilePath);
			}
		}

		private void SaveAs()
		{
			var childView = CurrentView as MasterViewModelBase;
			if (childView != null)
				CheckSettingsWorkspace(childView);
			FileDialogResult result = _dialogService.ShowSaveFileDialog(this, CogProjectFileType);
			if (result.IsValid)
				SaveProject(result.FileName);
		}

		private void ImportWordLists()
		{
			if (_importService.ImportWordLists(this, _project))
				IsChanged = true;
		}

		private void ImportGeographicRegions()
		{
			_importService.ImportGeographicRegions(this, _project);
		}

		private bool CanExportWordLists()
		{
			return _project.Varieties.Count > 0 || _project.Senses.Count > 0;
		}

		private void ExportWordLists()
		{
			_exportService.ExportWordLists(this, _project);
		}

		private bool CanExportSimilarityMatrix()
		{
			return _project.VarietyPairs.Count > 0;
		}

		private void ExportSimilarityMatrix()
		{
			var vm = new ExportSimilarityMatrixViewModel();
			if (_dialogService.ShowDialog(this, vm) == true)
				_exportService.ExportSimilarityMatrix(this, _project, vm.SimilarityMetric);
		}

		private bool CanExportCognateSets()
		{
			return _project.VarietyPairs.Count > 0;
		}

		private void ExportCognateSets()
		{
			_exportService.ExportCognateSets(this, _project);
		}

		private bool CanExportHierarchicalGraph()
		{
			return _project.VarietyPairs.Count > 0;
		}

		private void ExportHierarchicalGraph()
		{
			var vm = new ExportHierarchicalGraphViewModel();
			if (_dialogService.ShowDialog(this, vm) == true)
				_exportService.ExportHierarchicalGraph(this, _project.GenerateHierarchicalGraph(vm.ClusteringMethod, vm.SimilarityMetric), vm.GraphType);
		}

		private bool CanExportNetworkGraph()
		{
			return _project.VarietyPairs.Count > 0;
		}

		private void ExportNetworkGraph()
		{
			var vm = new ExportNetworkGraphViewModel();
			if (_dialogService.ShowDialog(this, vm) == true)
				_exportService.ExportNetworkGraph(this, _project.GenerateNetworkGraph(vm.SimilarityMetric), vm.SimilarityScoreFilter);
		}

		private bool CanExit()
		{
			if (CanCloseProject())
			{
				if (IsChanged)
					AcceptChanges();
				return true;
			}
			return false;
		}

		private bool CanCloseProject()
		{
			var childView = CurrentView as MasterViewModelBase;
			if (childView != null)
				CheckSettingsWorkspace(childView);
			if (IsChanged)
			{
				bool? res = _dialogService.ShowQuestion(this, "Do you want to save the changes to this project?", "Cog");
				if (res == true)
					Save();
				else if (res == null)
					return false;
			}
			return true;
		}

		private void Exit()
		{
			Settings.Default.Save();
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

		public ICommand ExportHierarchicalGraphCommand
		{
			get { return _exportHierarchicalGraphCommand; }
		}

		public ICommand ExportNetworkGraphCommand
		{
			get { return _exportNetworkGraphCommand; }
		}

		public ICommand ExitCommand
		{
			get { return _exitCommand; }
		}

		private string ProjectFilePath
		{
			get { return Settings.Default.LastProject; }
			set { Settings.Default.LastProject = value; }
		}

		private void OpenProject(string path)
		{
			CogProject project = ConfigManager.Load(_spanFactory, path);
			SetupProject(path, Path.GetFileNameWithoutExtension(path), project);
		}

		private void NewProject()
		{
			Stream stream = Assembly.GetAssembly(GetType()).GetManifestResourceStream("SIL.Cog.NewProject.cogx");
			CogProject project = ConfigManager.Load(_spanFactory, stream);
			SetupProject(null, "New Project", project);
		}

		private void SetupProject(string path, string name, CogProject project)
		{
			if (IsChanged)
				AcceptChanges();
			ProjectFilePath = path;
			_project = project;
			DisplayName = string.Format("{0} - Cog", name);
			Initialize(project);
			SwitchView(typeof(WordListsViewModel), null);
		}

		private void SaveProject(string path)
		{
			ConfigManager.Save(_project, path);
			ProjectFilePath = path;
			AcceptChanges();
		}
	}
}
