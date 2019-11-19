using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Application.Import;
using SIL.Cog.Application.ViewModels;
using SIL.Extensions;

namespace SIL.Cog.Application.Services
{
	public class ImportService : IImportService
	{
		private static readonly Dictionary<FileType, IWordListsImporter> WordListsImporters;
		private static readonly Dictionary<FileType, ISegmentMappingsImporter> SegmentMappingsImporters;
		private static readonly Dictionary<FileType, IGeographicRegionsImporter> GeographicRegionsImporters;
		private static readonly IWordListsImporter ClipboardWordListsImporter = new TextWordListsImporter('\t');
		static ImportService()
		{
			WordListsImporters = new Dictionary<FileType, IWordListsImporter>
				{
					{new FileType("Tab-delimited Text", ".txt"), new TextWordListsImporter('\t')},
					{new FileType("Comma-delimited Text", ".csv"), new TextWordListsImporter(',')},
					{new FileType("WordSurv 6 XML", ".xml"), new WordSurv6WordListsImporter()},
					{new FileType("WordSurv 7 CSV", ".csv"), new WordSurv7WordListsImporter()}
				};

			SegmentMappingsImporters = new Dictionary<FileType, ISegmentMappingsImporter>
				{
					{new FileType("Tab-delimited Text", ".txt"), new TextSegmentMappingsImporter('\t')},
					{new FileType("Comma-delimited Text", ".csv"), new TextSegmentMappingsImporter(',')}
				};

			GeographicRegionsImporters = new Dictionary<FileType, IGeographicRegionsImporter>
				{
					{new FileType("Google Earth", ".kml", ".kmz"), new KmlGeographicRegionsImporter()}
				};
		}

		private readonly IProjectService _projectService;
		private readonly IDialogService _dialogService;
		private readonly IBusyService _busyService;
		private readonly IAnalysisService _analysisService;
		private readonly Dictionary<IImporter, object> _importerSettingsViewModels;

		public ImportService(IProjectService projectService, IDialogService dialogService, IBusyService busyService, IAnalysisService analysisService)
		{
			_projectService = projectService;
			_dialogService = dialogService;
			_busyService = busyService;
			_analysisService = analysisService;
			_importerSettingsViewModels = new Dictionary<IImporter, object>();
		}

		public bool ImportWordListsFromFile(object ownerViewModel)
		{
			FileDialogResult result = _dialogService.ShowOpenFileDialog(ownerViewModel, "Import Word Lists", WordListsImporters.Keys);
			if (result.IsValid)
			{
				IWordListsImporter importer = WordListsImporters[result.SelectedFileType];
				if (Import(importer, ownerViewModel, result.FileName, (importSettingsViewModel, stream) => importer.Import(importSettingsViewModel, stream, _projectService.Project)))
				{
					_analysisService.SegmentAll();
					Messenger.Default.Send(new DomainModelChangedMessage(true));
					return true;
				}
			}
			return false;
		}

		public bool CanImportWordListsFromClipboard()
		{
			return Clipboard.ContainsData(DataFormats.UnicodeText);
		}

		public bool ImportWordListsFromClipboard(object ownerViewModel)
		{
			if (!Clipboard.ContainsData(DataFormats.UnicodeText))
				return false;

			try
			{
				object importSettingsViewModel;
				if (GetImportSettings(ownerViewModel, ClipboardWordListsImporter, out importSettingsViewModel))
				{
					var data = (string) Clipboard.GetData(DataFormats.UnicodeText);
					using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
						ClipboardWordListsImporter.Import(importSettingsViewModel, stream, _projectService.Project);
					_analysisService.SegmentAll();
					Messenger.Default.Send(new DomainModelChangedMessage(true));
					return true;
				}
			}
			catch (ImportException ie)
			{
				_dialogService.ShowError(ownerViewModel, $"Error importing from clipboard:\n{ie.Message}", "Cog");
			}

			return false;
		}

		public bool ImportSegmentMappings(object ownerViewModel, out IEnumerable<Tuple<string, string>> mappings)
		{
			FileDialogResult result = _dialogService.ShowOpenFileDialog(ownerViewModel, "Import Correspondences", SegmentMappingsImporters.Keys);
			if (result.IsValid)
			{
				ISegmentMappingsImporter importer = SegmentMappingsImporters[result.SelectedFileType];
				IEnumerable<Tuple<string, string>> importedMappings = null;
				if (Import(importer, ownerViewModel, result.FileName, (importSettingsViewModel, stream) => importedMappings = importer.Import(importSettingsViewModel, stream)))
				{
					mappings = importedMappings;
					return true;
				}
			}
			mappings = null;
			return false;
		}

		public bool ImportGeographicRegions(object ownerViewModel)
		{
			FileDialogResult result = _dialogService.ShowOpenFileDialog(ownerViewModel, "Import Regions", GeographicRegionsImporters.Keys);
			if (result.IsValid)
			{
				IGeographicRegionsImporter importer = GeographicRegionsImporters[result.SelectedFileType];
				if (Import(importer, ownerViewModel, result.FileName, (importSettingsViewModel, stream) => importer.Import(importSettingsViewModel, stream, _projectService.Project)))
				{
					Messenger.Default.Send(new DomainModelChangedMessage(false));
					return true;
				}
			}
			return false;
		}

		private bool Import(IImporter importer, object ownerViewModel, string fileName, Action<object, Stream> importAction)
		{
			object importSettingsViewModel;
			if (GetImportSettings(ownerViewModel, importer, out importSettingsViewModel))
			{
				_busyService.ShowBusyIndicatorUntilFinishDrawing();
				try
				{
					using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
						importAction(importSettingsViewModel, stream);
					return true;
				}
				catch (IOException ioe)
				{
					_dialogService.ShowError(ownerViewModel, $"Error importing file:\n{ioe.Message}", "Cog");
				}
				catch (ImportException ie)
				{
					_dialogService.ShowError(ownerViewModel, $"Error importing file:\n{ie.Message}", "Cog");
				}
			}
			return false;
		}

		private bool GetImportSettings(object ownerViewModel, IImporter importer, out object importSettingsViewModel)
		{
			importSettingsViewModel = _importerSettingsViewModels.GetOrCreate(importer, importer.CreateImportSettingsViewModel);
			if (importSettingsViewModel == null)
				return true;

			return _dialogService.ShowModalDialog(ownerViewModel, importSettingsViewModel) == true;
		}
	}
}
