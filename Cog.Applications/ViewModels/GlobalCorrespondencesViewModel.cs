using System;
using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using QuickGraph;
using SIL.Cog.Applications.GraphAlgorithms;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.ViewModels
{
	public class GlobalCorrespondencesViewModel : WorkspaceViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly IImageExportService _imageExportService;
		private readonly WordPairsViewModel _observedWordPairs;
		private GlobalCorrespondenceEdge _selectedCorrespondence;
		private ViewModelSyllablePosition _syllablePosition;
		private readonly TaskAreaIntegerViewModel _correspondenceFilter;
		private readonly IDialogService _dialogService;
		private readonly IBusyService _busyService;
		private readonly IGraphService _graphService;
		private readonly ICommand _findCommand;
		private IBidirectionalGraph<GridVertex, GlobalCorrespondenceEdge> _graph; 

		private FindViewModel _findViewModel;

		public GlobalCorrespondencesViewModel(IProjectService projectService, IBusyService busyService, IDialogService dialogService, IImageExportService imageExportService, IGraphService graphService,
			WordPairsViewModel.Factory wordPairsFactory)
			: base("Global Correspondences")
		{
			_projectService = projectService;
			_busyService = busyService;
			_dialogService = dialogService;
			_imageExportService = imageExportService;
			_graphService = graphService;

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => GenerateGraph());
			Messenger.Default.Register<DomainModelChangedMessage>(this, msg =>
				{
					if (msg.AffectsComparison)
						ClearGraph();
				});
			Messenger.Default.Register<PerformingComparisonMessage>(this, msg => ClearGraph());

			_findCommand = new RelayCommand(Find);

			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Syllable position",
				new TaskAreaCommandViewModel("Onset", new RelayCommand(() => SyllablePosition = ViewModelSyllablePosition.Onset)),
				new TaskAreaCommandViewModel("Nucleus", new RelayCommand(() => SyllablePosition = ViewModelSyllablePosition.Nucleus)),
				new TaskAreaCommandViewModel("Coda", new RelayCommand(() => SyllablePosition = ViewModelSyllablePosition.Coda))));
			_correspondenceFilter = new TaskAreaIntegerViewModel("Frequency threshold");
			_correspondenceFilter.PropertyChanging += _correspondenceFilter_PropertyChanging;
			_correspondenceFilter.PropertyChanged += _correspondenceFilter_PropertyChanged;
			TaskAreas.Add(_correspondenceFilter);
			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
				new TaskAreaCommandViewModel("Find words", _findCommand),
				new TaskAreaItemsViewModel("Sort word pairs by", new TaskAreaCommandGroupViewModel(
					new TaskAreaCommandViewModel("Sense", new RelayCommand(() => _observedWordPairs.UpdateSort("Sense.Gloss", ListSortDirection.Ascending))),
					new TaskAreaCommandViewModel("Similarity", new RelayCommand(() => _observedWordPairs.UpdateSort("PhoneticSimilarityScore", ListSortDirection.Descending)))))
				));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export current chart", new RelayCommand(ExportChart))));
			_observedWordPairs = wordPairsFactory();
			_observedWordPairs.IncludeVarietyNamesInSelectedText = true;
			_observedWordPairs.UpdateSort("Sense.Gloss", ListSortDirection.Ascending);
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			if (_projectService.Project.VarietyPairs.Count > 0)
				GenerateGraph();
			else
				ClearGraph();
		}

		private void ExportChart()
		{
			_imageExportService.ExportCurrentGlobalCorrespondencesChart(this);
		}

		private void Find()
		{
			if ( _findViewModel != null)
				return;

			_findViewModel = new FindViewModel(_dialogService, FindNext);
			_findViewModel.PropertyChanged += (sender, args) => _observedWordPairs.ResetSearch();
			_dialogService.ShowModelessDialog(this, _findViewModel, () => _findViewModel = null);
		}

		private void FindNext()
		{
			if (!_observedWordPairs.FindNext(_findViewModel.Field, _findViewModel.String, true, false))
				_findViewModel.ShowSearchEndedMessage();
		}

		protected override void OnIsCurrentChanged()
		{
			if (IsCurrent)
			{
				Messenger.Default.Send(new HookFindMessage(_findCommand));
			}
			else
			{
				_dialogService.CloseDialog(_findViewModel);
				Messenger.Default.Send(new HookFindMessage(null));
			}
		}

		private void GenerateGraph()
		{
			SelectedCorrespondence = null;
			Graph = _graphService.GenerateGlobalCorrespondencesGraph(_syllablePosition);
		}

		private void ClearGraph()
		{
			SelectedCorrespondence = null;
			Graph = null;
		}

		public ICommand FindCommand
		{
			get { return _findCommand; }
		}

		public GlobalCorrespondenceEdge SelectedCorrespondence
		{
			get { return _selectedCorrespondence; }
			set
			{
				GlobalCorrespondenceEdge oldCorr = _selectedCorrespondence;
				if (Set(() => SelectedCorrespondence, ref _selectedCorrespondence, value))
				{
					_busyService.ShowBusyIndicatorUntilUpdated();
					if (oldCorr != null)
						oldCorr.IsSelected = false;
					_observedWordPairs.WordPairs.Clear();
					if (_selectedCorrespondence != null)
					{
						_selectedCorrespondence.IsSelected = true;

						var seg1 = (GlobalSegmentVertex) _selectedCorrespondence.Source;
						var seg2 = (GlobalSegmentVertex) _selectedCorrespondence.Target;
						IWordAligner aligner = _projectService.Project.WordAligners["primary"];
						foreach (WordPair wp in _selectedCorrespondence.DomainWordPairs)
						{
							var vm = new WordPairViewModel(aligner, wp, true);
							foreach (AlignedNodeViewModel an in vm.AlignedNodes)
							{
								if ((seg1.StrReps.Contains(an.StrRep1) && seg2.StrReps.Contains(an.StrRep2))
								    || (seg1.StrReps.Contains(an.StrRep2) && seg2.StrReps.Contains(an.StrRep1)))
								{
									bool correctPosition = false;
									switch (_syllablePosition)
									{
										case ViewModelSyllablePosition.Onset:
											correctPosition = (!an.DomainCell1.IsNull && an.DomainCell1.First.Annotation.Parent.Children.First == an.DomainCell1.First.Annotation)
												|| (!an.DomainCell2.IsNull && an.DomainCell2.First.Annotation.Parent.Children.First == an.DomainCell2.First.Annotation);
											break;
										case ViewModelSyllablePosition.Nucleus:
											correctPosition = true;
											break;
										case ViewModelSyllablePosition.Coda:
											correctPosition = (!an.DomainCell1.IsNull && an.DomainCell1.Last.Annotation.Parent.Children.Last == an.DomainCell1.Last.Annotation)
											    || (!an.DomainCell2.IsNull && an.DomainCell2.Last.Annotation.Parent.Children.Last == an.DomainCell2.Last.Annotation);
											break;
									}

									if (correctPosition)
										an.IsSelected = true;
								}
							}
							_observedWordPairs.WordPairs.Add(vm);
						}
					}
				}
			}
		}

		public WordPairsViewModel ObservedWordPairs
		{
			get { return _observedWordPairs; }
		}

		public ViewModelSyllablePosition SyllablePosition
		{
			get { return _syllablePosition; }
			set
			{
				if (Set(() => SyllablePosition, ref _syllablePosition, value) && _graph != null)
					GenerateGraph();
			}
		}

		public IBidirectionalGraph<GridVertex, GlobalCorrespondenceEdge> Graph
		{
			get { return _graph; }
			set { Set(() => Graph, ref _graph, value); }
		}

		public int CorrespondenceFilter
		{
			get { return _correspondenceFilter.Value; }
			set { _correspondenceFilter.Value = value; }
		}

		private void _correspondenceFilter_PropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Value":
					RaisePropertyChanging(() => CorrespondenceFilter);
					break;
			}
		}

		private void _correspondenceFilter_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Value":
					RaisePropertyChanged(() => CorrespondenceFilter);
					if (_selectedCorrespondence != null && _selectedCorrespondence.Frequency < CorrespondenceFilter)
						SelectedCorrespondence = null;
					break;
			}
		}
	}
}
