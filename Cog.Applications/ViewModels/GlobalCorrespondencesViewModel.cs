using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using QuickGraph;
using SIL.Cog.Applications.GraphAlgorithms;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class GlobalCorrespondencesViewModel : WorkspaceViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly IImageExportService _imageExportService;
		private readonly WordPairsViewModel _wordPairs;
		private GlobalCorrespondenceEdge _selectedCorrespondence;
		private ViewModelSyllablePosition _syllablePosition;
		private readonly TaskAreaIntegerViewModel _correspondenceFilter;
		private readonly IDialogService _dialogService;
		private readonly IBusyService _busyService;
		private readonly IGraphService _graphService;
		private readonly ICommand _findCommand;
		private IBidirectionalGraph<GridVertex, GlobalCorrespondenceEdge> _graph; 

		private FindViewModel _findViewModel;
		private WordPairViewModel _startWordPair;
		private readonly SimpleMonitor _selectedWordPairsMonitor;

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
			Messenger.Default.Register<DomainModelChangedMessage>(this, msg => ClearGraph());
			Messenger.Default.Register<ViewChangedMessage>(this, HandleViewChanged);

			_selectedWordPairsMonitor = new SimpleMonitor();

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
					new TaskAreaCommandViewModel("Sense", new RelayCommand(() => _wordPairs.UpdateSort("Sense.Gloss", ListSortDirection.Ascending))),
					new TaskAreaCommandViewModel("Similarity", new RelayCommand(() => _wordPairs.UpdateSort("PhoneticSimilarityScore", ListSortDirection.Descending)))))
				));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export current chart", new RelayCommand(ExportChart))));
			_wordPairs = wordPairsFactory();
			_wordPairs.IncludeVarietyNamesInSelectedText = true;
			_wordPairs.UpdateSort("Sense.Gloss", ListSortDirection.Ascending);
			_wordPairs.SelectedWordPairs.CollectionChanged += SelectedWordPairs_CollectionChanged;
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			if (_projectService.Project.VarietyPairs.Count > 0)
				GenerateGraph();
			else
				ClearGraph();
		}

		private void SelectedWordPairs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!_selectedWordPairsMonitor.Busy)
				_startWordPair = null;
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
			_findViewModel.PropertyChanged += (sender, args) => _startWordPair = null;
			_dialogService.ShowModelessDialog(this, _findViewModel, () => _findViewModel = null);
		}

		private void FindNext()
		{
			if (_wordPairs.WordPairs.Count == 0)
			{
				SearchEnded();
				return;
			}
			if (_wordPairs.SelectedWordPairs.Count == 0)
			{
				_startWordPair = _wordPairs.WordPairsView.Cast<WordPairViewModel>().Last();
			}
			else if (_startWordPair == null)
			{
				_startWordPair = _wordPairs.SelectedWordPairs[0];
			}
			else if (_wordPairs.SelectedWordPairs.Contains(_startWordPair))
			{
				SearchEnded();
				return;
			}

			List<WordPairViewModel> wordPairs = _wordPairs.WordPairsView.Cast<WordPairViewModel>().ToList();
			WordPairViewModel curWordPair = _wordPairs.SelectedWordPairs.Count == 0 ? _startWordPair : _wordPairs.SelectedWordPairs[0];
			int wordPairIndex = wordPairs.IndexOf(curWordPair);
			do
			{
				wordPairIndex = (wordPairIndex + 1) % wordPairs.Count;
				curWordPair = wordPairs[wordPairIndex];
				bool match = false;
				switch (_findViewModel.Field)
				{
					case FindField.Form:
						match = curWordPair.DomainWordPair.Word1.StrRep.Contains(_findViewModel.String)
							|| curWordPair.DomainWordPair.Word2.StrRep.Contains(_findViewModel.String);
						break;

					case FindField.Sense:
						match = curWordPair.Sense.Gloss.Contains(_findViewModel.String);
						break;
				}
				if (match)
				{
					using (_selectedWordPairsMonitor.Enter())
					{
						_wordPairs.SelectedWordPairs.Clear();
						_wordPairs.SelectedWordPairs.Add(curWordPair);
					}
					return;
				}
			}
			while (_startWordPair != curWordPair);
			SearchEnded();
		}

		private void SearchEnded()
		{
			_findViewModel.ShowSearchEndedMessage();
			_startWordPair = null;
		}

		private void HandleViewChanged(ViewChangedMessage msg)
		{
			if (msg.OldViewModel == this && _findViewModel != null)
			{
				_dialogService.CloseDialog(_findViewModel);
				_findViewModel = null;
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
					_wordPairs.WordPairs.Clear();
					_wordPairs.SelectedWordPairs.Clear();
					if (_selectedCorrespondence != null)
					{
						_selectedCorrespondence.IsSelected = true;

						IWordAligner aligner = _projectService.Project.WordAligners["primary"];
						foreach (WordPair wp in _selectedCorrespondence.DomainWordPairs)
						{
							var vm = new WordPairViewModel(aligner, wp, true);
							switch (_syllablePosition)
							{
								case ViewModelSyllablePosition.Onset:
									foreach (AlignedNodeViewModel an in vm.AlignedNodes)
									{
										if ((!an.DomainCell1.IsNull && an.DomainCell1.First.Annotation.Parent.Children.First == an.DomainCell1.First.Annotation)
										    || (!an.DomainCell2.IsNull && an.DomainCell2.First.Annotation.Parent.Children.First == an.DomainCell2.First.Annotation))
										{
											CheckAlignedNodeSelected(an);
										}
									}
									break;
								case ViewModelSyllablePosition.Nucleus:
									foreach (AlignedNodeViewModel an in vm.AlignedNodes)
										CheckAlignedNodeSelected(an);
									break;
								case ViewModelSyllablePosition.Coda:
									foreach (AlignedNodeViewModel an in vm.AlignedNodes)
									{
										if ((!an.DomainCell1.IsNull && an.DomainCell1.Last.Annotation.Parent.Children.Last == an.DomainCell1.Last.Annotation)
										    || (!an.DomainCell2.IsNull && an.DomainCell2.Last.Annotation.Parent.Children.Last == an.DomainCell2.Last.Annotation))
										{
											CheckAlignedNodeSelected(an);
										}
									}
									break;
							}
							_wordPairs.WordPairs.Add(vm);
						}
					}
				}
			}
		}

		private void CheckAlignedNodeSelected(AlignedNodeViewModel an)
		{
			var seg1 = (GlobalSegmentVertex) _selectedCorrespondence.Source;
			var seg2 = (GlobalSegmentVertex) _selectedCorrespondence.Target;

			an.IsSelected = (seg1.StrReps.Contains(an.StrRep1) && seg2.StrReps.Contains(an.StrRep2))
				|| (seg1.StrReps.Contains(an.StrRep2) && seg2.StrReps.Contains(an.StrRep1));
		}

		public WordPairsViewModel WordPairs
		{
			get { return _wordPairs; }
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
