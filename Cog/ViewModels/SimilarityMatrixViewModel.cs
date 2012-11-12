using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Clusterers;
using SIL.Cog.Processors;
using SIL.Cog.Services;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class SimilarityMatrixViewModel : WorkspaceViewModelBase
	{
		private readonly IProgressService _progressService;
		private readonly IExportService _exportService;
		private CogProject _project;
		private ReadOnlyCollection<VarietySimilarityMatrixViewModel> _varieties;
		private readonly List<Variety> _modelVarieties;
		private bool _isEmpty;
		private SimilarityMetric _similarityMetric;

		public SimilarityMatrixViewModel(IProgressService progressService, IExportService exportService)
			: base("Similarity Matrix")
		{
			_progressService = progressService;
			_exportService = exportService;
			_modelVarieties = new List<Variety>();
			TaskAreas.Add(new TaskAreaGroupViewModel("Similarity metric",
				new CommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new CommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaViewModel("Common tasks",
				new CommandViewModel("Perform comparison", new RelayCommand(PerformComparison))));
			TaskAreas.Add(new TaskAreaViewModel("Other tasks",
				new CommandViewModel("Export this matrix", new RelayCommand(Export))));
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			ResetVarieties();
			_project.Varieties.CollectionChanged += VarietiesChanged;
			_project.Senses.CollectionChanged += SensesChanged;
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
			Set("Varieties", ref _varieties, new ReadOnlyCollection<VarietySimilarityMatrixViewModel>(new VarietySimilarityMatrixViewModel[0]));
			IsEmpty = true;
		}

		private void PerformComparison()
		{
			if (_project.Varieties.Count == 0 || _project.Senses.Count == 0)
				return;

			ResetVarieties();
			var generator = new VarietyPairGenerator();
			generator.Process(_project);
			var processors = new []
				{
					new WordPairGenerator(_project, "primary"),
					_project.VarietyPairProcessors["soundChangeInducer"],
					_project.VarietyPairProcessors["similarSegmentIdentifier"],
					_project.VarietyPairProcessors["cognateIdentifier"]
				};
			var pipeline = new MultiThreadedPipeline<VarietyPair>(processors);

			var progressVM = new ProgressViewModel(() => pipeline.Process(_project.VarietyPairs)) {Text = "Comparing all variety pairs..."};
			pipeline.ProgressUpdated += (sender, e) => progressVM.Value = e.PercentCompleted;

			if (_progressService.ShowProgress(this, progressVM))
			{
				CreateSimilarityMatrix();
				Messenger.Default.Send(new NotificationMessage(Notifications.ComparisonPerformed));
			}
			else
			{
				pipeline.Cancel();
				pipeline.WaitForComplete();
			}
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
			VarietySimilarityMatrixViewModel[] vms = _modelVarieties.Select(v => new VarietySimilarityMatrixViewModel(_similarityMetric, _modelVarieties, v)).ToArray();
			Set("Varieties", ref _varieties, new ReadOnlyCollection<VarietySimilarityMatrixViewModel>(vms));
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

		public ReadOnlyCollection<VarietySimilarityMatrixViewModel> Varieties
		{
			get { return _varieties; }
		}
	}
}
