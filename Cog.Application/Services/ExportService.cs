using System;
using System.Collections.Generic;
using System.IO;
using SIL.Cog.Application.Export;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Services
{
	public class ExportService : IExportService
	{
		private static readonly Dictionary<FileType, IWordListsExporter> WordListsExporters = new Dictionary<FileType, IWordListsExporter>
			{
				{new FileType("Tab-delimited Text", ".txt"), new TextWordListsExporter()},
			};

		private static readonly Dictionary<FileType, ISimilarityMatrixExporter> SimilarityMatrixExporters = new Dictionary<FileType, ISimilarityMatrixExporter>
			{
				{new FileType("Tab-delimited Text", ".txt"), new TextSimilarityMatrixExporter()},
			};

		private static readonly Dictionary<FileType, ICognateSetsExporter> CognateSetsExporters = new Dictionary<FileType, ICognateSetsExporter>
			{
				{new FileType("Tab-delimited Text", ".txt"), new TextCognateSetsExporter()},
				{new FileType("NEXUS", ".nex"), new NexusCognateSetsExporter()}
			};

		private static readonly Dictionary<FileType, IVarietyPairExporter> VarietyPairExporters = new Dictionary<FileType, IVarietyPairExporter>
			{
				{new FileType("Unicode Text", ".txt"), new TextVarietyPairExporter()}
			};

		private static readonly Dictionary<FileType, ISegmentFrequenciesExporter> SegmentFrequenciesExporters = new Dictionary<FileType, ISegmentFrequenciesExporter>
			{
				{new FileType("Tab-delimited Text", ".txt"), new TextSegmentFrequenciesExporter()}
			};

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
				return Export(ownerViewModel, result.FileName, stream => SimilarityMatrixExporters[result.SelectedFileType].Export(stream, _projectService.Project, similarityMetric));
			return false;
		}

		public bool ExportWordLists(object ownerViewModel)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog(ownerViewModel, "Export Word Lists", WordListsExporters.Keys);
			if (result.IsValid)
				return Export(ownerViewModel, result.FileName, stream => WordListsExporters[result.SelectedFileType].Export(stream, _projectService.Project));
			return false;
		}

		public bool ExportCognateSets(object ownerViewModel)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog(ownerViewModel, "Export Cognate Sets", CognateSetsExporters.Keys);
			if (result.IsValid)
				return Export(ownerViewModel, result.FileName, stream => CognateSetsExporters[result.SelectedFileType].Export(stream, _projectService.Project));
			return false;
		}

		public bool ExportVarietyPair(object ownerViewModel, VarietyPair varietyPair)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog(ownerViewModel, "Export Variety Pair", VarietyPairExporters.Keys);
			if (result.IsValid)
				return Export(ownerViewModel, result.FileName, stream => VarietyPairExporters[result.SelectedFileType].Export(stream, _projectService.Project.WordAligners["primary"], varietyPair));
			return false;
		}

		public bool ExportSegmentFrequencies(object ownerViewModel, SyllablePosition syllablePosition)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog(ownerViewModel, "Export Segment Frequencies", SegmentFrequenciesExporters.Keys);
			if (result.IsValid)
				return Export(ownerViewModel, result.FileName, stream => SegmentFrequenciesExporters[result.SelectedFileType].Export(stream, _projectService.Project, syllablePosition));
			return false;
		}

		private bool Export(object ownerViewModel, string fileName, Action<Stream> exportAction)
		{
			try
			{
				_busyService.ShowBusyIndicator(() =>
					{
						using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
							exportAction(stream);
					});
				return true;
			}
			catch (Exception e)
			{
				_dialogService.ShowError(ownerViewModel, string.Format("Error exporting file:\n{0}", e.Message), "Cog");
				return false;
			}
		}
	}
}
