using System;
using System.Collections.Generic;
using SIL.Cog.Import;

namespace SIL.Cog.Services
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
					{new FileType("Tab-delimited Text", ".txt"), new TextWordListsImporter()},
					{new FileType("WordSurv XML", ".xml"), new WordSurvWordListsImporter()}
				};

			SegmentMappingsImporters = new Dictionary<FileType, ISegmentMappingsImporter>
				{
					{new FileType("Tab-delimited Text", ".txt"), new TextSegmentMappingsImporter()}
				};

			GeographicRegionsImporters = new Dictionary<FileType, IGeographicRegionsImporter>
				{
					{new FileType("Google Earth", ".kml", ".kmz"), new KmlGeographicRegionsImporter()}
				};
		}

		private readonly IDialogService _dialogService;

		public ImportService(IDialogService dialogService)
		{
			_dialogService = dialogService;
		}

		public bool ImportWordLists(object ownerViewModel, CogProject project)
		{
			FileDialogResult result = _dialogService.ShowOpenFileDialog(ownerViewModel, "Import word lists", WordListsImporters.Keys);
			if (result.IsValid)
			{
				project.Senses.Clear();
				project.Varieties.Clear();
				WordListsImporters[result.SelectedFileType].Import(result.FileName, project);
				return true;
			}
			return false;
		}

		public bool ImportSegmentMappings(object ownerViewModel, out IEnumerable<Tuple<string, string>> mappings)
		{
			FileDialogResult result = _dialogService.ShowOpenFileDialog(ownerViewModel, "Import similar segments", SegmentMappingsImporters.Keys);
			if (result.IsValid)
			{
				mappings = SegmentMappingsImporters[result.SelectedFileType].Import(result.FileName);
				return true;
			}
			mappings = null;
			return false;
		}

		public bool ImportGeographicRegions(object ownerViewModel, CogProject project)
		{
			FileDialogResult result = _dialogService.ShowOpenFileDialog(ownerViewModel, "Import regions", GeographicRegionsImporters.Keys);
			if (result.IsValid)
			{
				GeographicRegionsImporters[result.SelectedFileType].Import(result.FileName, project);
				return true;
			}
			return false;
		}
	}
}
