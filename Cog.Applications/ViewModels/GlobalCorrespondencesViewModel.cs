using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using QuickGraph;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Applications.ViewModels
{
	public class GlobalCorrespondencesViewModel : WorkspaceViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly IImageExportService _imageExportService;
		private readonly WordPairsViewModel _observedWordPairs;
		private GlobalCorrespondenceEdge _selectedCorrespondence;
		private SyllablePosition _syllablePosition;
		private readonly TaskAreaIntegerViewModel _correspondenceFilter;
		private readonly IDialogService _dialogService;
		private readonly IBusyService _busyService;
		private readonly IGraphService _graphService;
		private readonly ICommand _findCommand;
		private IBidirectionalGraph<GridVertex, GlobalCorrespondenceEdge> _graph;
		private readonly HashSet<Variety> _selectedVarieties;

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

			_selectedVarieties = new HashSet<Variety>();

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
				new TaskAreaCommandViewModel("Onset", new RelayCommand(() => SyllablePosition = SyllablePosition.Onset)),
				new TaskAreaCommandViewModel("Nucleus", new RelayCommand(() => SyllablePosition = SyllablePosition.Nucleus)),
				new TaskAreaCommandViewModel("Coda", new RelayCommand(() => SyllablePosition = SyllablePosition.Coda))));
			_correspondenceFilter = new TaskAreaIntegerViewModel("Frequency threshold");
			_correspondenceFilter.PropertyChanging += _correspondenceFilter_PropertyChanging;
			_correspondenceFilter.PropertyChanged += _correspondenceFilter_PropertyChanged;
			TaskAreas.Add(_correspondenceFilter);
			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
				new TaskAreaCommandViewModel("Find words", _findCommand),
				new TaskAreaItemsViewModel("Sort word pairs by", new TaskAreaCommandGroupViewModel(
					new TaskAreaCommandViewModel("Gloss", new RelayCommand(() => _observedWordPairs.UpdateSort("Sense.Gloss", ListSortDirection.Ascending))),
					new TaskAreaCommandViewModel("Similarity", new RelayCommand(() => _observedWordPairs.UpdateSort("PhoneticSimilarityScore", ListSortDirection.Descending))))),
				new TaskAreaCommandViewModel("Select varieties", new RelayCommand(SelectVarieties))
				));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export current chart", new RelayCommand(ExportChart))));
			_observedWordPairs = wordPairsFactory();
			_observedWordPairs.IncludeVarietyNamesInSelectedText = true;
			_observedWordPairs.UpdateSort("Sense.Gloss", ListSortDirection.Ascending);
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			_selectedVarieties.Clear();
			_selectedVarieties.UnionWith(_projectService.Project.Varieties);
			_projectService.Project.Varieties.CollectionChanged += VarietiesChanged;
			if (_projectService.AreAllVarietiesCompared)
				GenerateGraph();
			else
				ClearGraph();
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
				_selectedVarieties.ExceptWith(e.OldItems.Cast<Variety>());
			if (e.NewItems != null)
				_selectedVarieties.UnionWith(e.NewItems.Cast<Variety>());
		}

		private void SelectVarieties()
		{
			var vm = new SelectVarietiesViewModel(_projectService.Project.Varieties, _selectedVarieties);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				_selectedVarieties.Clear();
				_selectedVarieties.UnionWith(vm.Varieties.Where(v => v.IsSelected).Select(v => v.DomainVariety));
				GenerateGraph();
			}
		}

		private void ExportChart()
		{
			if (_projectService.AreAllVarietiesCompared)
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

		protected override void OnIsSelectedChanged()
		{
			if (IsSelected)
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
			_busyService.ShowBusyIndicatorUntilFinishDrawing();
			SelectedCorrespondence = null;
			Graph = _graphService.GenerateGlobalCorrespondencesGraph(_syllablePosition, _selectedVarieties);
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
					_busyService.ShowBusyIndicatorUntilFinishDrawing();
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
									FeatureSymbol pos = null;
									switch (_syllablePosition)
									{
										case SyllablePosition.Onset:
											pos = CogFeatureSystem.Onset;
											break;
										case SyllablePosition.Nucleus:
											pos = CogFeatureSystem.Nucleus;
											break;
										case SyllablePosition.Coda:
											pos = CogFeatureSystem.Coda;
											break;
									}

									SymbolicFeatureValue curPos1, curPos2;
									if (!an.DomainCell1.IsNull && !an.DomainCell2.IsNull
									    && an.DomainCell1.First.Annotation.FeatureStruct.TryGetValue(CogFeatureSystem.SyllablePosition, out curPos1) && (FeatureSymbol) curPos1 == pos
										&& an.DomainCell2.First.Annotation.FeatureStruct.TryGetValue(CogFeatureSystem.SyllablePosition, out curPos2) && (FeatureSymbol) curPos2 == pos)
									{
										an.IsSelected = true;
									}
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

		public SyllablePosition SyllablePosition
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
