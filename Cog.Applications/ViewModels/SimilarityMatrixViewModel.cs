using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Clusterers;
using SIL.Cog.Domain.Components;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class SimilarityMatrixViewModel : WorkspaceViewModelBase
	{
		private readonly IDialogService _dialogService;
		private readonly IExportService _exportService;
		private CogProject _project;
		private ReadOnlyList<SimilarityMatrixVarietyViewModel> _varieties;
		private readonly List<Variety> _modelVarieties;
		private bool _isEmpty;
		private SimilarityMetric _similarityMetric;

		public SimilarityMatrixViewModel(IDialogService dialogService, IExportService exportService)
			: base("Similarity Matrix")
		{
			_dialogService = dialogService;
			_exportService = exportService;
			_modelVarieties = new List<Variety>();

			Messenger.Default.Register<DomainModelChangingMessage>(this, msg => ResetVarieties());

			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Similarity metric",
				new TaskAreaCommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new TaskAreaCommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
				new TaskAreaCommandViewModel("Perform comparison", new RelayCommand(PerformComparison))));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export this matrix", new RelayCommand(Export))));
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			ResetVarieties();
			if (_project.VarietyPairs.Count > 0)
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
			if (_project.Varieties.Count == 0 || _project.Senses.Count == 0)
				return;

			ResetVarieties();
			var generator = new VarietyPairGenerator();
			generator.Process(_project);

			var pipeline = new MultiThreadedPipeline<VarietyPair>(_project.GetComparisonProcessors());

			var progressVM = new ProgressViewModel(vm =>
				{
					vm.Text = "Comparing all variety pairs...";
					pipeline.Process(_project.VarietyPairs);
					while (!pipeline.WaitForComplete(500))
					{
						if (vm.Canceled)
						{
							pipeline.Cancel();
							pipeline.WaitForComplete();
							break;
						}
					}
					if (vm.Canceled)
						return;
					vm.Text = "Analyzing results...";
					CreateSimilarityMatrix();
					Messenger.Default.Send(new ComparisonPerformedMessage());
				});
			pipeline.ProgressUpdated += (sender, e) => progressVM.Value = e.PercentCompleted;

			_dialogService.ShowModalDialog(this, progressVM);
		}

		private void Export()
		{
			if (!_isEmpty)
				_exportService.ExportSimilarityMatrix(this, _project, _similarityMetric);
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
			_modelVarieties.AddRange(optics.ClusterOrder(_project.Varieties).Select(oe => oe.DataObject));
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
