using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Clusterers;
using SIL.Cog.Components;
using SIL.Cog.Services;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class SimilarityMatrixViewModel : WorkspaceViewModelBase
	{
		private readonly IProgressService _progressService;
		private readonly IExportService _exportService;
		private CogProject _project;
		private System.Collections.ObjectModel.ReadOnlyCollection<SimilarityMatrixVarietyViewModel> _varieties;
		private readonly List<Variety> _modelVarieties;
		private bool _isEmpty;
		private SimilarityMetric _similarityMetric;

		public SimilarityMatrixViewModel(IProgressService progressService, IExportService exportService)
			: base("Similarity Matrix")
		{
			_progressService = progressService;
			_exportService = exportService;
			_modelVarieties = new List<Variety>();
			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Similarity metric",
				new CommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new CommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaCommandsViewModel("Common tasks",
				new CommandViewModel("Perform comparison", new RelayCommand(PerformComparison))));
			TaskAreas.Add(new TaskAreaCommandsViewModel("Other tasks",
				new CommandViewModel("Export this matrix", new RelayCommand(Export))));
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			ResetVarieties();
			_project.Varieties.CollectionChanged += VarietiesChanged;
			_project.Senses.CollectionChanged += SensesChanged;
			if (_project.VarietyPairs.Count > 0)
				CreateSimilarityMatrix();
		}

		private void SensesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			ResetVarieties();
			_project.VarietyPairs.Clear();
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			ResetVarieties();
			_project.VarietyPairs.Clear();
		}

		private void ResetVarieties()
		{
			if (IsEmpty)
				return;
			_modelVarieties.Clear();
			Varieties = new System.Collections.ObjectModel.ReadOnlyCollection<SimilarityMatrixVarietyViewModel>(new SimilarityMatrixVarietyViewModel[0]);
			IsEmpty = true;
		}

		private void PerformComparison()
		{
			if (_project.Varieties.Count == 0 || _project.Senses.Count == 0)
				return;

			Messenger.Default.Send(new NotificationMessage(Notifications.PerformingComparison));
			ResetVarieties();
			var generator = new VarietyPairGenerator();
			generator.Process(_project);

			var pipeline = new MultiThreadedPipeline<VarietyPair>(_project.GetVarietyPairProcessors());

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
					Messenger.Default.Send(new NotificationMessage(Notifications.ComparisonPerformed));
				});
			pipeline.ProgressUpdated += (sender, e) => progressVM.Value = e.PercentCompleted;

			_progressService.ShowProgress(this, progressVM);
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
			Varieties = new System.Collections.ObjectModel.ReadOnlyCollection<SimilarityMatrixVarietyViewModel>(vms);
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

		public System.Collections.ObjectModel.ReadOnlyCollection<SimilarityMatrixVarietyViewModel> Varieties
		{
			get { return _varieties; }
			private set { Set(() => Varieties, ref _varieties, value); }
		}
	}
}
