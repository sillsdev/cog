using System;
using System.Collections.Generic;
using SIL.Cog.Import;

namespace SIL.Cog.Services
{
	public class ImportService : IImportService
	{
		private static readonly Dictionary<FileType, IWordListsImporter> WordListsImporters;
		private static readonly Dictionary<FileType, ISimilarSegmentsImporter> SimilarSegmentsImporters; 
		static ImportService()
		{
			WordListsImporters = new Dictionary<FileType, IWordListsImporter>
				{
					{new FileType("Tab-delimited Text", ".txt"), new TextWordListsImporter()},
					{new FileType("WordSurv XML", ".xml"), new WordSurvWordListsImporter()}
				};

			SimilarSegmentsImporters = new Dictionary<FileType, ISimilarSegmentsImporter>
				{
					{new FileType("Tab-delimited Text", ".txt"), new TextSimilarSegmentsImporter()}
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

		public bool ImportSimilarSegments(object ownerViewModel, out IEnumerable<Tuple<string, string>> mappings)
		{
			FileDialogResult result = _dialogService.ShowOpenFileDialog(ownerViewModel, "Import similar segments", SimilarSegmentsImporters.Keys);
			if (result.IsValid)
			{
				mappings = SimilarSegmentsImporters[result.SelectedFileType].Import(result.FileName);
				return true;
			}
			mappings = null;
			return false;
		}
	}
}
