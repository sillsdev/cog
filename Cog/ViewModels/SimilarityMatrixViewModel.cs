using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private readonly IDialogService _dialogService; 
		private CogProject _project;
		private ViewModelCollection<VarietySimilarityMatrixViewModel, Variety> _varieties;
		private readonly ObservableCollection<Variety> _modelVarieties; 

		public SimilarityMatrixViewModel(IDialogService dialogService)
			: base("Similarity Matrix")
		{
			_dialogService = dialogService;
			_modelVarieties = new ObservableCollection<Variety>();
			TaskAreas.Add(new TaskAreaViewModel("Common tasks", new []
				{
					new CommandViewModel("Perform comparison", new RelayCommand(PerformComparison))
				}));
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			Set("Varieties", ref _varieties, new ViewModelCollection<VarietySimilarityMatrixViewModel, Variety>(_modelVarieties, variety => new VarietySimilarityMatrixViewModel(_modelVarieties, variety)));
		}

		private void PerformComparison()
		{
			_modelVarieties.Clear();
			var processors = new []
				{
					new WordPairGenerator(_project, "primary"),
					_project.VarietyPairProcessors["soundChangeInducer"],
					_project.VarietyPairProcessors["similarSegmentIdentifier"],
					_project.VarietyPairProcessors["cognateIdentifier"]
				};
			var pipeline = new MultiThreadedPipeline<VarietyPair>(processors);

			var progressVM = new ProgressViewModel(pvm =>
				{
					pvm.Text = "Comparing all variety pairs...";
					pipeline.ProgressUpdated += (sender, e) => pvm.Value = e.PercentCompleted;
					pipeline.Process(_project.VarietyPairs);
				});

			if (_dialogService.ShowDialog(this, progressVM) == false)
			{
				pipeline.Cancel();
				pipeline.WaitForComplete();
				_modelVarieties.Clear();
			}
			else
			{
				var optics = new Optics<Variety>(variety => variety.VarietyPairs.Select(pair =>
					Tuple.Create(pair.GetOtherVariety(variety), 1.0 - pair.LexicalSimilarityScore)).Concat(Tuple.Create(variety, 0.0)), 2);
				IList<ClusterOrderEntry<Variety>> orderEntries = optics.ClusterOrder(_project.Varieties);
				_modelVarieties.AddRange(orderEntries.Select(entry => entry.DataObject));
				Messenger.Default.Send(new NotificationMessage(Notifications.ComparisonPerformed));
			}
		}

		public ObservableCollection<VarietySimilarityMatrixViewModel> Varieties
		{
			get { return _varieties; }
		}
	}
}
