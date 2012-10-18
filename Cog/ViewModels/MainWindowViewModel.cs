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
		private readonly ICommand _exitCommand;

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

			_newCommand = new RelayCommand(New);
			_openCommand = new RelayCommand(Open);
			_saveCommand = new RelayCommand(Save, CanSave);
			_saveAsCommand = new RelayCommand(SaveAs);
			_importCommand = new RelayCommand(Import);
			_exitCommand = new RelayCommand(Exit, CanExit);

			Messenger.Default.Register<SwitchViewMessage>(this, HandleSwitchView);

			NewProject();
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
			FileDialogResult result = _dialogService.ShowSaveFileDialog(this, CogProjectFileType);
			if (result.IsValid)
				SaveProject(result.FileName);
		}

		private void Import()
		{
			if (ViewModelUtilities.ImportWordLists(_dialogService, _project, this))
				IsChanged = true;
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
