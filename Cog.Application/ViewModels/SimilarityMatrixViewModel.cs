using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine.Clusterers;

namespace SIL.Cog.Application.ViewModels
{
	public class SimilarityMatrixViewModel : WorkspaceViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly IExportService _exportService;
		private readonly IAnalysisService _analysisService;
		private ReadOnlyList<SimilarityMatrixVarietyViewModel> _varieties;
		private readonly List<Variety> _modelVarieties;
		private bool _isEmpty;
		private SimilarityMetric _similarityMetric;

		public SimilarityMatrixViewModel(IProjectService projectService, IExportService exportService, IAnalysisService analysisService)
			: base("Similarity Matrix")
		{
			_projectService = projectService;
			_exportService = exportService;
			_analysisService = analysisService;
			_modelVarieties = new List<Variety>();

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			Messenger.Default.Register<DomainModelChangedMessage>(this, msg =>
				{
					if (msg.AffectsComparison)
						ResetVarieties();
				});
			Messenger.Default.Register<PerformingComparisonMessage>(this, msg => ResetVarieties());
			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => CreateSimilarityMatrix());

			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Similarity metric",
				new TaskAreaCommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new TaskAreaCommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
				new TaskAreaCommandViewModel("Compare all variety pairs", new RelayCommand(PerformComparison))));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export this matrix", new RelayCommand(Export, CanExport))));
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			ResetVarieties();
			if (_projectService.AreAllVarietiesCompared)
				CreateSimilarityMatrix();
		}

		private void ResetVarieties()
		{
			if (IsEmpty)
				return;
			_modelVarieties.Clear();
			Varieties = new ReadOnlyList<SimilarityMatrixVarietyViewModel>(new SimilarityMatrixVarietyViewModel[0]);
			IsEmpty = true;
		}

		private void PerformComparison()
		{
			_analysisService.CompareAll(this);
		}

		private bool CanExport()
		{
			return !_isEmpty;
		}

		private void Export()
		{
			_exportService.ExportSimilarityMatrix(this, _similarityMetric);
		}

		private void CreateSimilarityMatrix()
		{
			var optics = new Optics<Variety>(variety => variety.VarietyPairs.Select(pair =>
				{
					double score = 0;
					switch (_similarityMetric)
					{
						case SimilarityMetric.Lexical:
							score = pair.LexicalSimilarityScore;
							break;
						case SimilarityMetric.Phonetic:
							score = pair.PhoneticSimilarityScore;
							break;
					}
					return Tuple.Create(pair.GetOtherVariety(variety), 1.0 - score);
				}).Concat(Tuple.Create(variety, 0.0)), 2);
			_modelVarieties.Clear();
			_modelVarieties.AddRange(optics.ClusterOrder(_projectService.Project.Varieties).Select(oe => oe.DataObject));
			SimilarityMatrixVarietyViewModel[] vms = _modelVarieties.Select(v => new SimilarityMatrixVarietyViewModel(_similarityMetric, _modelVarieties, v)).ToArray();
			Varieties = new ReadOnlyList<SimilarityMatrixVarietyViewModel>(vms);
			IsEmpty = false;
		}

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set
			{
				if (Set(() => SimilarityMetric, ref _similarityMetric, value))
				{
					ResetVarieties();
					CreateSimilarityMatrix();
				}
			}
		}

		public bool IsEmpty
		{
			get { return _isEmpty; }
			set { Set(() => IsEmpty, ref _isEmpty, value); }
		}

		public ReadOnlyList<SimilarityMatrixVarietyViewModel> Varieties
		{
			get { return _varieties; }
			private set { Set(() => Varieties, ref _varieties, value); }
		}
	}
}
