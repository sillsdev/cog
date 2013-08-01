using System.Collections.Generic;
using SIL.Cog.Applications.Export;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Services
{
	public class ExportService : IExportService
	{
		private static readonly Dictionary<FileType, IWordListsExporter> WordListsExporters;
		private static readonly Dictionary<FileType, ISimilarityMatrixExporter> SimilarityMatrixExporters;
		private static readonly Dictionary<FileType, ICognateSetsExporter> CognateSetsExporters;
		private static readonly Dictionary<FileType, IVarietyPairExporter> VarietyPairExporters; 
		static ExportService()
		{
			WordListsExporters = new Dictionary<FileType, IWordListsExporter>
				{
					{new FileType("Tab-delimited Text", ".txt"), new TextWordListsExporter()},
				};

			SimilarityMatrixExporters = new Dictionary<FileType, ISimilarityMatrixExporter>
				{
					{new FileType("Tab-delimited Text", ".txt"), new TextSimilarityMatrixExporter()},
				};

			CognateSetsExporters = new Dictionary<FileType, ICognateSetsExporter>
				{
					{new FileType("Tab-delimited Text", ".txt"), new TextCognateSetsExporter()},
					{new FileType("NEXUS", ".nex"), new NexusCognateSetsExporter()}
				};
			VarietyPairExporters = new Dictionary<FileType, IVarietyPairExporter>
				{
					{new FileType("Unicode Text", ".txt"), new TextVarietyPairExporter()}
				};
		}

		private readonly IProjectService _projectService;
		private readonly IDialogService _dialogService;
		private readonly IBusyService _busyService;

		public ExportService(IProjectService projectService, IDialogService dialogService, IBusyService busyService)
		{
			_projectService = projectService;
			_dialogService = dialogService;
			_busyService = busyService;
		}

		public bool ExportSimilarityMatrix(object ownerViewModel, SimilarityMetric similarityMetric)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog(ownerViewModel, "Export Similarity Matrix", SimilarityMatrixExporters.Keys);
			if (result.IsValid)
			{
				_busyService.ShowBusyIndicator(() => SimilarityMatrixExporters[result.SelectedFileType].Export(result.FileName, _projectService.Project, similarityMetric));
				return true;
			}
			return false;
		}

		public bool ExportWordLists(object ownerViewModel)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog(ownerViewModel, "Export Word Lists", WordListsExporters.Keys);
			if (result.IsValid)
			{
				_busyService.ShowBusyIndicator(() => WordListsExporters[result.SelectedFileType].Export(result.FileName, _projectService.Project));
				return true;
			}
			return false;
		}

		public bool ExportCognateSets(object ownerViewModel)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog(ownerViewModel, "Export Cognate Sets", CognateSetsExporters.Keys);
			if (result.IsValid)
			{
				_busyService.ShowBusyIndicator(() => CognateSetsExporters[result.SelectedFileType].Export(result.FileName, _projectService.Project));
				return true;
			}
			return false;
		}

		public bool ExportVarietyPair(object ownerViewModel, VarietyPair varietyPair)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog(ownerViewModel, "Export Variety Pair", VarietyPairExporters.Keys);
			if (result.IsValid)
			{
				_busyService.ShowBusyIndicator(() => VarietyPairExporters[result.SelectedFileType].Export(result.FileName, _projectService.Project.WordAligners["primary"], varietyPair));
				return true;
			}
			return false;
		}


	}
}
