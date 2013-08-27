using System;
using System.Collections.Generic;
using System.IO;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Import;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Services
{
	public class ImportService : IImportService
	{
		private static readonly Dictionary<FileType, IWordListsImporter> WordListsImporters;
		private static readonly Dictionary<FileType, ISegmentMappingsImporter> SegmentMappingsImporters;
		private static readonly Dictionary<FileType, IGeographicRegionsImporter> GeographicRegionsImporters;
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

		public ImportService(IProjectService projectService, IDialogService dialogService, IBusyService busyService, IAnalysisService analysisService)
		{
			_projectService = projectService;
			_dialogService = dialogService;
			_busyService = busyService;
			_analysisService = analysisService;
		}

		public bool ImportWordLists(object ownerViewModel)
		{
			FileDialogResult result = _dialogService.ShowOpenFileDialog(ownerViewModel, "Import Word Lists", WordListsImporters.Keys);
			if (result.IsValid)
			{
				IWordListsImporter importer = WordListsImporters[result.SelectedFileType];
				CogProject project = _projectService.Project;
				if (Import(importer, ownerViewModel, importSettingsViewModel => importer.Import(importSettingsViewModel, result.FileName, project)))
				{
					_analysisService.SegmentAll();
					Messenger.Default.Send(new DomainModelChangedMessage(true));
					return true;
				}
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
				if (Import(importer, ownerViewModel, importSettingsViewModel => importedMappings = importer.Import(importSettingsViewModel, result.FileName)))
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
				if (Import(importer, ownerViewModel, importSettingsViewModel => importer.Import(importSettingsViewModel, result.FileName, _projectService.Project)))
				{
					Messenger.Default.Send(new DomainModelChangedMessage(false));
					return true;
				}
			}
			return false;
		}

		private bool Import(IImporter importer, object ownerViewModel, Action<object> importAction)
		{
			object importSettingsViewModel;
			if (GetImportSettings(ownerViewModel, importer, out importSettingsViewModel))
			{
				_busyService.ShowBusyIndicatorUntilUpdated();
				try
				{
					importAction(importSettingsViewModel);
					return true;
				}
				catch (ImportException ie)
				{
					_dialogService.ShowError(ownerViewModel, ie.Message, "Cog");
				}
				catch (IOException ioe)
				{
					_dialogService.ShowError(ownerViewModel, ioe.Message, "Cog");
				}
			}
			return false;
		}

		private bool GetImportSettings(object ownerViewModel, IImporter importer, out object importSettingsViewModel)
		{
			importSettingsViewModel = importer.CreateImportSettingsViewModel();
			if (importSettingsViewModel == null)
				return true;

			return _dialogService.ShowModalDialog(ownerViewModel, importSettingsViewModel) == true;
		}
	}
}
