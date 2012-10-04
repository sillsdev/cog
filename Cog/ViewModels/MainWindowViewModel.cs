using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Config;
using SIL.Cog.Processors;
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

		private readonly IDialogService _dialogService;
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private CogProject _project;
		private string _projectFilePath;

		public MainWindowViewModel(SpanFactory<ShapeNode> spanFactory, IDialogService dialogService, DataMasterViewModel dataMasterViewModel, ComparisonMasterViewModel comparisonMasterViewModel,
			VisualizationMasterViewModel visualizationMasterViewModel)
			: base("Cog", dataMasterViewModel, comparisonMasterViewModel, visualizationMasterViewModel)
		{
			_dialogService = dialogService;

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

			Messenger.Default.Register<SwitchViewMessage>(this, HandleSwitchView);

			NewProject();
		}

		private void HandleSwitchView(SwitchViewMessage message)
		{
			SwitchView(message.ViewModelType, message.Model);
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

			CogProject project = ConfigManager.Load(_spanFactory, path);
			var generator = new VarietyPairGenerator();
			generator.Process(project);
			_project = project;
			Initialize(project);
			CurrentView = Views[0];
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
	}
}
