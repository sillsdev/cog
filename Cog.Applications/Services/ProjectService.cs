using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using GalaSoft.MvvmLight.Messaging;
using ProtoBuf;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Config;
using SIL.Machine;

namespace SIL.Cog.Applications.Services
{
	public class ProjectService : IProjectService
	{
		private static readonly FileType CogProjectFileType = new FileType("Cog Project", ".cogx");

		public event EventHandler<EventArgs> ProjectOpened;

		private readonly SpanFactory<ShapeNode> _spanFactory;
		private readonly SegmentPool _segmentPool;
		private readonly IDialogService _dialogService;
		private readonly IBusyService _busyService;
		private readonly ISettingsService _settingsService;
		private readonly Lazy<IAnalysisService> _analysisService;

		private CogProject _project;
		private string _projectName;
		private bool _isChanged;

		public ProjectService(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, IDialogService dialogService, IBusyService busyService,
			ISettingsService settingsService, Lazy<IAnalysisService> analysisService)
		{
			_spanFactory = spanFactory;
			_segmentPool = segmentPool;
			_dialogService = dialogService;
			_busyService = busyService;
			_settingsService = settingsService;
			_analysisService = analysisService;

			Messenger.Default.Register<DomainModelChangedMessage>(this, HandleDomainModelChanged);
			Messenger.Default.Register<ComparisonPerformedMessage>(this, HandleComparisonPerformed);
		}

		private void HandleDomainModelChanged(DomainModelChangedMessage msg)
		{
			_isChanged = true;
			_project.VarietyPairs.Clear();
		}

		private void HandleComparisonPerformed(ComparisonPerformedMessage msg)
		{
			if (_settingsService.LastProject != null && !_isChanged)
				SaveComparisonCache();
		}

		public void Init()
		{
			string projectPath = _settingsService.LastProject;
			string[] args = Environment.GetCommandLineArgs();
			if (args.Length > 1)
				projectPath = args[1];

			if (string.IsNullOrEmpty(projectPath) || !File.Exists(projectPath))
			{
				NewProject();
			}
			else
			{
				try
				{
					OpenProject(projectPath);
				}
				catch (ConfigException)
				{
					NewProject();
				}
			}
		}

		public bool New()
		{
			if (Close())
			{
				NewProject();
				return true;
			}
			return false;
		}

		private void NewProject()
		{
			_busyService.ShowBusyIndicatorUntilUpdated();
			Stream stream = Assembly.GetAssembly(GetType()).GetManifestResourceStream("SIL.Cog.Applications.NewProject.cogx");
			CogProject project = ConfigManager.Load(_spanFactory, _segmentPool, stream);
			SetupProject(null, "New Project", project);
		}

		public bool Open()
		{
			if (Close())
			{
				FileDialogResult result = _dialogService.ShowOpenFileDialog(this, CogProjectFileType);
				if (result.IsValid)
				{
					try
					{
						OpenProject(result.FileName);
						return true;
					}
					catch (ConfigException)
					{
						_dialogService.ShowError(this, "The specified file is not a valid Cog configuration file.", "Cog");
					}
					catch (IOException ioe)
					{
						_dialogService.ShowError(this, ioe.Message, "Cog");
					}
				}
			}

			return false;
		}

		public bool Close()
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

		private void OpenProject(string path)
		{
			_busyService.ShowBusyIndicatorUntilUpdated();
			CogProject project = ConfigManager.Load(_spanFactory, _segmentPool, path);
			SetupProject(path, Path.GetFileNameWithoutExtension(path), project);
		}

		public bool Save()
		{
			if (string.IsNullOrEmpty(_settingsService.LastProject))
			{
				FileDialogResult result = _dialogService.ShowSaveFileDialog(this, CogProjectFileType);
				if (result.IsValid)
				{
					SaveProject(result.FileName);
					return true;
				}

				return false;
			}

			SaveProject(_settingsService.LastProject);
			return true;
		}

		public bool SaveAs()
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog(this, CogProjectFileType);
			if (result.IsValid)
			{
				SaveProject(result.FileName);
				return true;
			}

			return false;
		}

		private void SaveProject(string path)
		{
			ConfigManager.Save(_project, path);
			_settingsService.LastProject = path;
			_projectName = Path.GetFileNameWithoutExtension(path);
			SaveComparisonCache();
			_isChanged = false;
		}

		private void SetupProject(string path, string name, CogProject project)
		{
			_settingsService.LastProject = path;
			_isChanged = false;
			_project = project;
			_projectName = name;

			_analysisService.Value.SegmentAll();

			if (path != null)
			{
				string cogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SIL", "Cog");
				string cacheFileName = Path.Combine(cogPath, name + ".cache");
				if (File.Exists(cacheFileName))
				{
					bool delete = false;
					using (FileStream fs = File.OpenRead(cacheFileName))
					{
						var hash = Serializer.DeserializeWithLengthPrefix<string>(fs, PrefixStyle.Base128, 1);
						if (hash == CalcProjectHash())
						{
							var cache = Serializer.DeserializeWithLengthPrefix<ComparisonCache>(fs, PrefixStyle.Base128, 1);
							cache.Load(_segmentPool, _project);
						}
						else
						{
							delete = true;
						}
					}
					if (delete)
						File.Delete(cacheFileName);
				}
			}

			OnProjectOpened(new EventArgs());
		}

		private void SaveComparisonCache()
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SIL", "Cog");
			string name = Path.GetFileNameWithoutExtension(_settingsService.LastProject);
			string cacheFileName = Path.Combine(path, name + ".cache");
			if (_project.VarietyPairs.Count == 0)
			{
				if (File.Exists(cacheFileName))
					File.Delete(cacheFileName);
			}
			else
			{
				Directory.CreateDirectory(path);
				using (FileStream fs = File.Create(cacheFileName))
				{
					Serializer.SerializeWithLengthPrefix(fs, CalcProjectHash(), PrefixStyle.Base128, 1);
					Serializer.SerializeWithLengthPrefix(fs, new ComparisonCache(_project), PrefixStyle.Base128, 1);
				}
			}
		}

		private string CalcProjectHash()
		{
			using (var md5 = MD5.Create())
			{
				using (FileStream fs = File.OpenRead(_settingsService.LastProject))
				{
					return BitConverter.ToString(md5.ComputeHash(fs)).Replace("-","").ToLower();
				}
			}
		}

		private void OnProjectOpened(EventArgs e)
		{
			if (ProjectOpened != null)
				ProjectOpened(this, e);
		}

		public bool IsChanged
		{
			get { return _isChanged; }
		}

		public CogProject Project
		{
			get { return _project; }
		}

		public string ProjectName
		{
			get { return _projectName; }
		}
	}
}
