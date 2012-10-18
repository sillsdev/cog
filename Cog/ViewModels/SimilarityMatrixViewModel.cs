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
		private CogProject _project;
		private ReadOnlyCollection<VarietySimilarityMatrixViewModel> _varieties;
		private readonly ObservableCollection<Variety> _modelVarieties;
		private bool _isEmpty;

		public SimilarityMatrixViewModel(IProgressService progressService)
			: base("Similarity Matrix")
		{
			_progressService = progressService;
			_modelVarieties = new ObservableCollection<Variety>();
			TaskAreas.Add(new TaskAreaViewModel("Common tasks", new []
				{
					new CommandViewModel("Perform comparison", new RelayCommand(PerformComparison))
				}));
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
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			ResetVarieties();
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
				var optics = new Optics<Variety>(variety => variety.VarietyPairs.Select(pair =>
					Tuple.Create(pair.GetOtherVariety(variety), 1.0 - pair.LexicalSimilarityScore)).Concat(Tuple.Create(variety, 0.0)), 2);
				var vms = new List<VarietySimilarityMatrixViewModel>();
				foreach (ClusterOrderEntry<Variety> orderEntry in optics.ClusterOrder(_project.Varieties))
				{
					_modelVarieties.Add(orderEntry.DataObject);
					vms.Add(new VarietySimilarityMatrixViewModel(_modelVarieties, orderEntry.DataObject));
				}
				Set("Varieties", ref _varieties, new ReadOnlyCollection<VarietySimilarityMatrixViewModel>(vms));
				IsEmpty = false;
				Messenger.Default.Send(new NotificationMessage(Notifications.ComparisonPerformed));
			}
			else
			{
				pipeline.Cancel();
				pipeline.WaitForComplete();
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
