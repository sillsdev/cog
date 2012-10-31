using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Config;
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
		private readonly ICommand _importCommand;
		private readonly ICommand _exportWordListsCommand;
		private readonly ICommand _exportSimilarityMatrixCommand;
		private readonly ICommand _exportHierarchicalGraphCommand;
		private readonly ICommand _exportNetworkGraphCommand;
		private readonly ICommand _exitCommand;

		private readonly IDialogService _dialogService;
		private readonly IExportGraphService _exportGraphService;
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private CogProject _project;
		private string _projectFilePath;

		public MainWindowViewModel(SpanFactory<ShapeNode> spanFactory, IDialogService dialogService, IExportGraphService exportGraphService,
			DataMasterViewModel dataMasterViewModel, ComparisonMasterViewModel comparisonMasterViewModel, VisualizationMasterViewModel visualizationMasterViewModel)
			: base("Cog", dataMasterViewModel, comparisonMasterViewModel, visualizationMasterViewModel)
		{
			_dialogService = dialogService;
			_exportGraphService = exportGraphService;

			_spanFactory = spanFactory;

			_newCommand = new RelayCommand(New);
			_openCommand = new RelayCommand(Open);
			_saveCommand = new RelayCommand(Save, CanSave);
			_saveAsCommand = new RelayCommand(SaveAs);
			_importCommand = new RelayCommand(Import);
			_exportWordListsCommand = new RelayCommand(ExportWordLists, CanExportWordLists);
			_exportSimilarityMatrixCommand = new RelayCommand(ExportSimilarityMatrix, CanExportSimilarityMatrix);
			_exportHierarchicalGraphCommand = new RelayCommand(ExportHierarchicalGraph, CanExportHierarchicalGraph);
			_exportNetworkGraphCommand = new RelayCommand(ExportNetworkGraph, CanExportNetworkGraph);
			_exitCommand = new RelayCommand(Exit, CanExit);

			foreach (MasterViewModelBase childView in Views.OfType<MasterViewModelBase>())
				childView.PropertyChanging += childView_PropertyChanging;

			PropertyChanging += MainWindowViewModel_PropertyChanging;

			Messenger.Default.Register<SwitchViewMessage>(this, HandleSwitchView);

			NewProject();
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
					OpenProject(result.FileName);
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
			if (_projectFilePath == null)
			{
				FileDialogResult result = _dialogService.ShowSaveFileDialog(this, CogProjectFileType);
				if (result.IsValid)
					SaveProject(result.FileName);
			}
			else
			{
				SaveProject(_projectFilePath);
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

		private void Import()
		{
			if (ViewModelUtilities.ImportWordLists(_dialogService, _project, this))
				IsChanged = true;
		}

		private bool CanExportWordLists()
		{
			return _project.Varieties.Count > 0 || _project.Senses.Count > 0;
		}

		private void ExportWordLists()
		{
			ViewModelUtilities.ExportWordLists(_dialogService, _project, this);
		}

		private bool CanExportSimilarityMatrix()
		{
			return _project.VarietyPairs.Count > 0;
		}

		private void ExportSimilarityMatrix()
		{
			var vm = new ExportSimilarityMatrixViewModel();
			if (_dialogService.ShowDialog(this, vm) == true)
				ViewModelUtilities.ExportSimilarityMatrix(_dialogService, _project, this, vm.SimilarityMetric);
		}

		private bool CanExportHierarchicalGraph()
		{
			return _project.VarietyPairs.Count > 0;
		}

		private void ExportHierarchicalGraph()
		{
			var vm = new ExportHierarchicalGraphViewModel();
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				FileDialogResult result = _dialogService.ShowSaveFileDialog("Export hierarchical graph", this, new FileType("PNG image", ".png"));
				if (result.IsValid)
					_exportGraphService.ExportHierarchicalGraph(ViewModelUtilities.GenerateHierarchicalGraph(_project, vm.ClusteringMethod, vm.SimilarityMetric), vm.GraphType, result.FileName);
			}
		}

		private bool CanExportNetworkGraph()
		{
			return _project.VarietyPairs.Count > 0;
		}

		private void ExportNetworkGraph()
		{
			var vm = new ExportNetworkGraphViewModel();
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				FileDialogResult result = _dialogService.ShowSaveFileDialog("Export network graph", this, new FileType("PNG image", ".png"));
				if (result.IsValid)
					_exportGraphService.ExportNetworkGraph(ViewModelUtilities.GenerateNetworkGraph(_project, vm.SimilarityMetric), result.FileName);
			}
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

		public ICommand ImportCommand
		{
			get { return _importCommand; }
		}

		public ICommand ExportWordListsCommand
		{
			get { return _exportWordListsCommand; }
		}

		public ICommand ExportSimilarityMatrixCommand
		{
			get { return _exportSimilarityMatrixCommand; }
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

		private void OpenProject(string path)
		{
			if (IsChanged)
				AcceptChanges();
			_projectFilePath = path;

			CogProject project = ConfigManager.Load(_spanFactory, path);
			_project = project;
			Initialize(project);
			SwitchView(typeof(WordListsViewModel), null);
		}

		private void NewProject()
		{
			OpenProject("NewProject.cogx");
			_projectFilePath = null;
			SwitchView(typeof(WordListsViewModel), null);
		}

		private void SaveProject(string path)
		{
			ConfigManager.Save(_project, path);
			_projectFilePath = path;
			AcceptChanges();
		}
	}
}
