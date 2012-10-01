using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Config;
using SIL.Cog.Services;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class MainWindowViewModel : MasterViewModel
	{
		private static readonly FileType CogProjectFileType = new FileType("Cog Project", ".cogx");

		private readonly DataMasterViewModel _dataMasterViewModel;
		private readonly ComparisonMasterViewModel _comparisonMasterViewModel;
		private readonly VisualizationMasterViewModel _visualizationMasterViewModel;

		private readonly ICommand _newCommand;
		private readonly ICommand _openCommand;
		private readonly ICommand _saveCommand;
		private readonly ICommand _saveAsCommand;
		private readonly ICommand _importCommand;

		private readonly IDialogService _dialogService;
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private CogProject _project;
		private string _projectFilePath;

		public MainWindowViewModel(SpanFactory<ShapeNode> spanFactory, IDialogService dialogService, DataMasterViewModel dataMasterViewModel, ComparisonMasterViewModel comparisonMasterViewModel,
			VisualizationMasterViewModel visualizationMasterViewModel)
			: base("Cog", dataMasterViewModel, comparisonMasterViewModel, visualizationMasterViewModel)
		{
			_dialogService = dialogService;

			_dataMasterViewModel = dataMasterViewModel;
			_comparisonMasterViewModel = comparisonMasterViewModel;
			_visualizationMasterViewModel = visualizationMasterViewModel;

			_spanFactory = spanFactory;

			_newCommand = new RelayCommand(NewProject);
			_openCommand = new RelayCommand(() =>
				{
					FileDialogResult result = _dialogService.ShowOpenFileDialog(this, CogProjectFileType);
					if (result.IsValid)
						OpenProject(result.FileName);
				});
			_saveCommand = new RelayCommand(() =>
				{
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
				});
			_saveAsCommand = new RelayCommand(() =>
				{
					FileDialogResult result = _dialogService.ShowSaveFileDialog(this, CogProjectFileType);
					if (result.IsValid)
						SaveProject(result.FileName);
				});
			_importCommand = new RelayCommand(() => ViewModelUtilities.ImportWordLists(_dialogService, _project, this));

			NewProject();
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

		private void OpenProject(string path)
		{
			_projectFilePath = path;
			Initialize(ConfigManager.Load(_spanFactory, path));
			CurrentView = _dataMasterViewModel;
		}

		private void NewProject()
		{
			OpenProject("NewProject.cogx");
			_projectFilePath = null;
		}

		private void SaveProject(string path)
		{
			ConfigManager.Save(_project, path);
			_projectFilePath = path;
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			_dataMasterViewModel.Initialize(project);
			_comparisonMasterViewModel.Initialize(project);
			_visualizationMasterViewModel.Initialize(project);
		}
	}
}
