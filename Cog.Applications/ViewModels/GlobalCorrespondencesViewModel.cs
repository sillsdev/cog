using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using QuickGraph;
using SIL.Cog.Applications.GraphAlgorithms;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Applications.ViewModels
{
	public enum SoundCorrespondenceType
	{
		InitialConsonants,
		MedialConsonants,
		FinalConsonants,
		Vowels
	}

	public class GlobalCorrespondencesViewModel : WorkspaceViewModelBase
	{
		private static readonly Dictionary<string, int> VowelHeightLookup = new Dictionary<string, int>
			{
				{"close", 1},
				{"near-close", 2},
				{"close-mid", 3},
				{"mid", 4},
				{"open-mid", 5},
				{"near-open", 6},
				{"open", 7}
			};

		private static readonly Dictionary<string, int> VowelBacknessLookup = new Dictionary<string, int>
			{
				{"front", 1},
				{"near-front", 4},
				{"central", 7},
				{"near-back", 10},
				{"back", 13}
			};

		private static readonly Dictionary<string, int> ConsonantPlaceLookup = new Dictionary<string, int>
			{
				{"bilabial", 1},
				{"labiodental", 4},
				{"dental", 7},
				{"alveolar", 10},
				{"palato-alveolar", 13},
				{"retroflex", 16},
				{"palatal", 19},
				{"velar", 22},
				{"uvular", 25},
				{"pharyngeal", 28},
				{"glottal", 31}
			};

		private static readonly Dictionary<string, int> ConsonantMannerLookup = new Dictionary<string, int>
			{
				{"stop", 1},
				{"trill", 3},
				{"flap", 4},
				{"fricative", 5},
				{"approximant", 7}
			};

		private readonly IProjectService _projectService;
		private readonly WordPairsViewModel _wordPairs;
		private GlobalCorrespondenceEdge _selectedCorrespondence;
		private readonly BackgroundWorker _generateCorrespondencesWorker;
		private bool _restartWork;
		private readonly ManualResetEvent _workCompleteEvent;
		private bool _generatingCorrespondences;
		private SoundCorrespondenceType _correspondenceType;
		private readonly TaskAreaIntegerViewModel _correspondenceFilter;
		private readonly IDialogService _dialogService;
		private readonly IBusyService _busyService;
		private readonly ICommand _findCommand;
		private IBidirectionalGraph<GridVertex, GlobalCorrespondenceEdge> _graph; 

		private FindViewModel _findViewModel;
		private WordPairViewModel _startWordPair;
		private readonly SimpleMonitor _selectedWordPairsMonitor;

		public GlobalCorrespondencesViewModel(IProjectService projectService, IBusyService busyService, IDialogService dialogService)
			: base("Global Correspondences")
		{
			_projectService = projectService;
			_busyService = busyService;
			_dialogService = dialogService;

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => GenerateCorrespondences());
			Messenger.Default.Register<DomainModelChangingMessage>(this, msg => CancelWorker());
			Messenger.Default.Register<ViewChangedMessage>(this, HandleViewChanged);

			_selectedWordPairsMonitor = new SimpleMonitor();

			_findCommand = new RelayCommand(Find);

			_generateCorrespondencesWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
			_generateCorrespondencesWorker.DoWork += GenerateCorrespondencesAsync;
			_generateCorrespondencesWorker.RunWorkerCompleted += GenerateCorrespondencesAsyncFinished;
			_workCompleteEvent = new ManualResetEvent(true);
			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Correspondence type",
				new TaskAreaCommandViewModel("Initial consonants", new RelayCommand(() => CorrespondenceType = SoundCorrespondenceType.InitialConsonants)),
				new TaskAreaCommandViewModel("Medial consonants", new RelayCommand(() => CorrespondenceType = SoundCorrespondenceType.MedialConsonants)),
				new TaskAreaCommandViewModel("Final consonants", new RelayCommand(() => CorrespondenceType = SoundCorrespondenceType.FinalConsonants)),
				new TaskAreaCommandViewModel("Vowels", new RelayCommand(() => CorrespondenceType = SoundCorrespondenceType.Vowels))));
			_correspondenceFilter = new TaskAreaIntegerViewModel("Frequency threshold");
			_correspondenceFilter.PropertyChanging += _correspondenceFilter_PropertyChanging;
			_correspondenceFilter.PropertyChanged += _correspondenceFilter_PropertyChanged;
			TaskAreas.Add(_correspondenceFilter);
			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
				new TaskAreaCommandViewModel("Find words", _findCommand),
				new TaskAreaItemsViewModel("Sort word pairs by", new TaskAreaCommandGroupViewModel(
					new TaskAreaCommandViewModel("Sense", new RelayCommand(() => SortWordPairsBy("Sense.Gloss", ListSortDirection.Ascending))),
					new TaskAreaCommandViewModel("Similarity", new RelayCommand(() => SortWordPairsBy("PhoneticSimilarityScore", ListSortDirection.Descending)))))
				));
			_wordPairs = new WordPairsViewModel();
			SortWordPairsBy("Sense.Gloss", ListSortDirection.Ascending);
			_wordPairs.SelectedWordPairs.CollectionChanged += SelectedWordPairs_CollectionChanged;
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			CancelWorker();
			if (_projectService.Project.VarietyPairs.Count > 0)
				GenerateCorrespondences();
		}

		private void SortWordPairsBy(string propertyName, ListSortDirection sortDirection)
		{
			_busyService.ShowBusyIndicatorUntilUpdated();
			var sortDesc = new SortDescription(propertyName, sortDirection);
			if (_wordPairs.WordPairsView.SortDescriptions.Count == 0)
				_wordPairs.WordPairsView.SortDescriptions.Add(sortDesc);
			else
				_wordPairs.WordPairsView.SortDescriptions[0] = sortDesc;
		}

		private void SelectedWordPairs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!_selectedWordPairsMonitor.Busy)
				_startWordPair = null;
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
					case FindField.Word:
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

		private void GenerateCorrespondencesAsync(object sender, DoWorkEventArgs e)
		{
			_workCompleteEvent.Reset();

			var correspondenceType = (SoundCorrespondenceType) e.Argument;

			var tasks = new List<Task<VarietyPairResult>>();
			foreach (VarietyPair vp in _projectService.Project.VarietyPairs)
			{
				VarietyPair varietyPair = vp;
				tasks.Add(Task<VarietyPairResult>.Factory.StartNew(() =>
					{
						var segs = new Dictionary<Tuple<int, int>, GlobalSegmentVertex>();
						var corrs = new Dictionary<UnorderedTuple<Tuple<int, int>, Tuple<int, int>>, GlobalCorrespondenceEdge>();
						foreach (WordPair wp in varietyPair.WordPairs.Where(wp => wp.AreCognatePredicted))
						{
							if (_generateCorrespondencesWorker.CancellationPending)
								break;

							var vm = new WordPairViewModel(_projectService.Project.WordAligners["primary"], wp, true);

							switch (correspondenceType)
							{
								case SoundCorrespondenceType.InitialConsonants:
									AddConsonantCorrespondence(vm, segs, corrs, 0);
									break;

								case SoundCorrespondenceType.MedialConsonants:
									for (int column = 1; column < vm.DomainAlignment.ColumnCount - 1; column++)
									{
										if (_generateCorrespondencesWorker.CancellationPending)
											break;

										AddConsonantCorrespondence(vm, segs, corrs, column);
									}
									break;

								case SoundCorrespondenceType.FinalConsonants:
									AddConsonantCorrespondence(vm, segs, corrs, vm.DomainAlignment.ColumnCount - 1);
									break;

								case SoundCorrespondenceType.Vowels:
									for (int column = 0; column < vm.DomainAlignment.ColumnCount; column++)
									{
										if (_generateCorrespondencesWorker.CancellationPending)
											break;

										AddVowelCorrespondence(vm, segs, corrs, column);
									}
									break;
							}
						}

						return new VarietyPairResult(segs, corrs);
					}));
			}

			if (_generateCorrespondencesWorker.CancellationPending)
			{
				e.Cancel = true;
			}
			else
			{
				var globalSegs = new Dictionary<Tuple<int, int>, GlobalSegmentVertex>();
				var globalCorrs = new Dictionary<UnorderedTuple<Tuple<int, int>, Tuple<int, int>>, GlobalCorrespondenceEdge>();
				int maxFreq = 0;
				foreach (Task<VarietyPairResult> task in tasks)
				{
					if (_generateCorrespondencesWorker.CancellationPending)
						break;

					VarietyPairResult varietyPairResult = task.Result;
					foreach (KeyValuePair<Tuple<int, int>, GlobalSegmentVertex> segKvp in varietyPairResult.Segments)
					{
						GlobalSegmentVertex seg;
						if (globalSegs.TryGetValue(segKvp.Key, out seg))
							seg.StrReps.UnionWith(seg.StrReps);
						else
							globalSegs[segKvp.Key] = segKvp.Value;
					}

					foreach (KeyValuePair<UnorderedTuple<Tuple<int, int>, Tuple<int, int>>, GlobalCorrespondenceEdge> corrKvp in varietyPairResult.Correspondences)
					{
						GlobalCorrespondenceEdge corr = globalCorrs.GetValue(corrKvp.Key, () => new GlobalCorrespondenceEdge(globalSegs[corrKvp.Key.Item1], globalSegs[corrKvp.Key.Item2]));
						corr.WordPairs.AddRange(corrKvp.Value.WordPairs);
						corr.Frequency += corrKvp.Value.Frequency;
						if (corr.Frequency > maxFreq)
							maxFreq = corr.Frequency;
					}
				}

				var graph = new BidirectionalGraph<GridVertex, GlobalCorrespondenceEdge>();

				switch (_correspondenceType)
				{
					case SoundCorrespondenceType.InitialConsonants:
					case SoundCorrespondenceType.MedialConsonants:
					case SoundCorrespondenceType.FinalConsonants:
						graph.AddVertexRange(new[]
							{
								new HeaderGridVertex("Bilabial") {Row = 0, Column = 1, ColumnSpan = 3},
								new HeaderGridVertex("Labiodental") {Row = 0, Column = 4, ColumnSpan = 3},
								new HeaderGridVertex("Dental") {Row = 0, Column = 7, ColumnSpan = 3},
								new HeaderGridVertex("Alveolar") {Row = 0, Column = 10, ColumnSpan = 3},
								new HeaderGridVertex("Postalveolar") {Row = 0, Column = 13, ColumnSpan = 3},
								new HeaderGridVertex("Retroflex") {Row = 0, Column = 16, ColumnSpan = 3},
								new HeaderGridVertex("Palatal") {Row = 0, Column = 19, ColumnSpan = 3},
								new HeaderGridVertex("Velar") {Row = 0, Column = 22, ColumnSpan = 3},
								new HeaderGridVertex("Uvular") {Row = 0, Column = 25, ColumnSpan = 3},
								new HeaderGridVertex("Pharyngeal") {Row = 0, Column = 28, ColumnSpan = 3},
								new HeaderGridVertex("Glottal") {Row = 0, Column = 31, ColumnSpan = 3},

								new HeaderGridVertex("Plosive") {Row = 1, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
								new HeaderGridVertex("Nasal") {Row = 2, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
								new HeaderGridVertex("Trill") {Row = 3, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
								new HeaderGridVertex("Tap or Flap") {Row = 4, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
								new HeaderGridVertex("Fricative") {Row = 5, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
								new HeaderGridVertex("Lateral fricative") {Row = 6, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
								new HeaderGridVertex("Approximant") {Row = 7, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
								new HeaderGridVertex("Lateral approximant") {Row = 8, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left}
							});
						break;

					case SoundCorrespondenceType.Vowels:
						graph.AddVertexRange(new []
							{
								new HeaderGridVertex("Front") {Row = 0, Column = 1, ColumnSpan = 3},
								new HeaderGridVertex("Central") {Row = 0, Column = 7, ColumnSpan = 3},
								new HeaderGridVertex("Back") {Row = 0, Column = 13, ColumnSpan = 3},

								new HeaderGridVertex("Close") {Row = 1, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
								new HeaderGridVertex("Close-mid") {Row = 3, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
								new HeaderGridVertex("Open-mid") {Row = 5, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
								new HeaderGridVertex("Open") {Row = 7, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left}
							});
						break;
				}
				graph.AddVertexRange(globalSegs.Values);
				foreach (GlobalCorrespondenceEdge corr in globalCorrs.Values)
				{
					corr.NormalizedFrequency = (double) corr.Frequency / maxFreq;
					graph.AddEdge(corr);
				}

				if (_generateCorrespondencesWorker.CancellationPending)
					e.Cancel = true;
				else
					e.Result = graph;
			}
			_workCompleteEvent.Set();
		}

		private class VarietyPairResult
		{
			private readonly Dictionary<Tuple<int, int>, GlobalSegmentVertex> _segments;
			private readonly Dictionary<UnorderedTuple<Tuple<int, int>, Tuple<int, int>>, GlobalCorrespondenceEdge> _correspondences;

			public VarietyPairResult(Dictionary<Tuple<int, int>, GlobalSegmentVertex> segments, Dictionary<UnorderedTuple<Tuple<int, int>, Tuple<int, int>>, GlobalCorrespondenceEdge> correspondences)
			{
				_segments = segments;
				_correspondences = correspondences;
			}

			public Dictionary<Tuple<int, int>, GlobalSegmentVertex> Segments
			{
				get { return _segments; }
			}

			public Dictionary<UnorderedTuple<Tuple<int, int>, Tuple<int, int>>, GlobalCorrespondenceEdge> Correspondences
			{
				get { return _correspondences; }
			}
		}

		private static void AddConsonantCorrespondence(WordPairViewModel wp, Dictionary<Tuple<int, int>, GlobalSegmentVertex> segs,
			Dictionary<UnorderedTuple<Tuple<int, int>, Tuple<int, int>>, GlobalCorrespondenceEdge> corrs, int column)
		{
			AlignmentCell<ShapeNode> cell1 = wp.DomainAlignment[0, column];
			AlignmentCell<ShapeNode> cell2 = wp.DomainAlignment[1, column];
			if (!cell1.IsNull && cell1.First.Type() == CogFeatureSystem.ConsonantType && !cell2.IsNull && cell2.First.Type() == CogFeatureSystem.ConsonantType)
			{
				Ngram ngram1 = cell1.ToNgram(wp.DomainWordPair.VarietyPair.Variety1.SegmentPool);
				Ngram ngram2 = cell2.ToNgram(wp.DomainWordPair.VarietyPair.Variety2.SegmentPool);
				if (ngram1.Count == 1 && ngram2.Count == 1)
				{
					Segment seg1 = ngram1.First;
					Segment seg2 = ngram2.First;
					GlobalSegmentVertex vertex1, vertex2;
					if (GetConsonant(segs, seg1, out vertex1) && GetConsonant(segs, seg2, out vertex2) && vertex1 != vertex2)
					{
						Tuple<int, int> key1 = Tuple.Create(vertex1.Row, vertex1.Column);
						Tuple<int, int> key2 = Tuple.Create(vertex2.Row, vertex2.Column);
						GlobalCorrespondenceEdge corr = corrs.GetValue(UnorderedTuple.Create(key1, key2), () => new GlobalCorrespondenceEdge(vertex1, vertex2));
						corr.Frequency++;
						corr.WordPairs.Add(wp);
					}
				}
			}
		}

		private static bool GetConsonant(Dictionary<Tuple<int, int>, GlobalSegmentVertex> consonants, Segment consonant, out GlobalSegmentVertex vertex)
		{
			if (consonant.StrRep.DisplayLength() > 1)
			{
				vertex = null;
				return false;
			}

			var placeSymbol = (FeatureSymbol) consonant.FeatureStruct.GetValue<SymbolicFeatureValue>("place");
			var mannerSymbol = (FeatureSymbol) consonant.FeatureStruct.GetValue<SymbolicFeatureValue>("manner");
			var voiceSymbol = (FeatureSymbol) consonant.FeatureStruct.GetValue<SymbolicFeatureValue>("voice");
			var nasalSymbol = (FeatureSymbol) consonant.FeatureStruct.GetValue<SymbolicFeatureValue>("nasal");

			int column;
			if (!ConsonantPlaceLookup.TryGetValue(placeSymbol.ID, out column))
			{
				vertex = null;
				return false;
			}

			int row;
			if (nasalSymbol.ID == "nasal+")
			{
				row = 2;
			}
			else if (ConsonantMannerLookup.TryGetValue(mannerSymbol.ID, out row))
			{
				var lateralSymbol = (FeatureSymbol) consonant.FeatureStruct.GetValue<SymbolicFeatureValue>("lateral");
				if (lateralSymbol.ID == "lateral+")
					row++;
			}
			else
			{
				vertex = null;
				return false;
			}

			var alignment = GridHorizontalAlignment.Right;
			if (voiceSymbol.ID == "voice+")
			{
				column += 2;
				alignment = GridHorizontalAlignment.Left;
			}

			vertex = consonants.GetValue(Tuple.Create(row, column), () => new GlobalSegmentVertex {Row = row, Column = column, HorizontalAlignment = alignment} );
			vertex.StrReps.Add(consonant.StrRep);
			return true;
		}

		private static void AddVowelCorrespondence(WordPairViewModel wp, Dictionary<Tuple<int, int>, GlobalSegmentVertex> segs,
			Dictionary<UnorderedTuple<Tuple<int, int>, Tuple<int, int>>, GlobalCorrespondenceEdge> corrs, int column)
		{
			AlignmentCell<ShapeNode> cell1 = wp.DomainAlignment[0, column];
			AlignmentCell<ShapeNode> cell2 = wp.DomainAlignment[1, column];
			if (!cell1.IsNull && cell1.First.Type() == CogFeatureSystem.VowelType && !cell2.IsNull && cell2.First.Type() == CogFeatureSystem.VowelType)
			{
				Ngram ngram1 = cell1.ToNgram(wp.DomainWordPair.VarietyPair.Variety1.SegmentPool);
				Ngram ngram2 = cell2.ToNgram(wp.DomainWordPair.VarietyPair.Variety2.SegmentPool);
				if (ngram1.Count == 1 && ngram2.Count == 1)
				{
					Segment seg1 = ngram1.First;
					Segment seg2 = ngram2.First;
					GlobalSegmentVertex vertex1, vertex2;
					if (GetVowel(segs, seg1, out vertex1) && GetVowel(segs, seg2, out vertex2) && vertex1 != vertex2)
					{
						Tuple<int, int> key1 = Tuple.Create(vertex1.Row, vertex1.Column);
						Tuple<int, int> key2 = Tuple.Create(vertex2.Row, vertex2.Column);
						GlobalCorrespondenceEdge corr = corrs.GetValue(UnorderedTuple.Create(key1, key2), () => new GlobalCorrespondenceEdge(vertex1, vertex2));
						corr.Frequency++;
						corr.WordPairs.Add(wp);
					}
				}
			}
		}

		private static bool GetVowel(Dictionary<Tuple<int, int>, GlobalSegmentVertex> vowels, Segment vowel, out GlobalSegmentVertex vertex)
		{
			if (vowel.StrRep.DisplayLength() > 1)
			{
				vertex = null;
				return false;
			}

			var heightSymbol = (FeatureSymbol) vowel.FeatureStruct.GetValue<SymbolicFeatureValue>("height");
			var backnessSymbol = (FeatureSymbol) vowel.FeatureStruct.GetValue<SymbolicFeatureValue>("backness");
			var roundSymbol = (FeatureSymbol) vowel.FeatureStruct.GetValue<SymbolicFeatureValue>("round");

			int row = VowelHeightLookup[heightSymbol.ID];
			int column = VowelBacknessLookup[backnessSymbol.ID];

			var alignment = GridHorizontalAlignment.Right;
			if (roundSymbol.ID == "round+")
			{
				column += 2;
				alignment = GridHorizontalAlignment.Left;
			}

			vertex = vowels.GetValue(Tuple.Create(row, column), () => new GlobalSegmentVertex {Row = row, Column = column, HorizontalAlignment = alignment});
			vertex.StrReps.Add(vowel.StrRep);
			return true;
		}

		private void GenerateCorrespondencesAsyncFinished(object sender, RunWorkerCompletedEventArgs e)
		{
			if (_restartWork)
			{
				RunWorker();
			}
			else
			{
				GeneratingCorrespondences = false;
				if (!e.Cancelled)
				{
					_busyService.ShowBusyIndicatorUntilUpdated();
					Graph = (IBidirectionalGraph<GridVertex, GlobalCorrespondenceEdge>) e.Result;
				}
			}
		}

		private void HandleViewChanged(ViewChangedMessage msg)
		{
			if (msg.OldViewModel == this && _findViewModel != null)
			{
				_dialogService.CloseDialog(_findViewModel);
				_findViewModel = null;
			}
		}

		private void GenerateCorrespondences()
		{
			if (_generateCorrespondencesWorker.IsBusy)
			{
				_restartWork = true;
				_generateCorrespondencesWorker.CancelAsync();
			}
			else
			{
				RunWorker();
			}
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
					if (oldCorr != null)
						oldCorr.IsSelected = false;
					_wordPairs.WordPairs.Clear();
					_wordPairs.SelectedWordPairs.Clear();
					if (_selectedCorrespondence != null)
					{
						_selectedCorrespondence.IsSelected = true;

						foreach (WordPairViewModel wp in _selectedCorrespondence.WordPairs)
						{
							foreach (AlignedNodeViewModel an in wp.AlignedNodes)
							{
								var seg1 = (GlobalSegmentVertex) _selectedCorrespondence.Source;
								var seg2 = (GlobalSegmentVertex) _selectedCorrespondence.Target;

								an.IsSelected = (seg1.StrReps.Contains(an.StrRep1) && seg2.StrReps.Contains(an.StrRep2))
									|| (seg1.StrReps.Contains(an.StrRep2) && seg2.StrReps.Contains(an.StrRep1));
							}
							_wordPairs.WordPairs.Add(wp);
						}
					}
				}
			}
		}

		private void RunWorker()
		{
			GeneratingCorrespondences = true;
			_restartWork = false;
			_wordPairs.WordPairs.Clear();
			Graph = null;
			_generateCorrespondencesWorker.RunWorkerAsync(_correspondenceType);
		}

		private void CancelWorker()
		{
			if (_generateCorrespondencesWorker.IsBusy)
			{
				_generateCorrespondencesWorker.CancelAsync();
				_workCompleteEvent.WaitOne();
			}
			Graph = null;
		}

		public WordPairsViewModel WordPairs
		{
			get { return _wordPairs; }
		}

		public SoundCorrespondenceType CorrespondenceType
		{
			get { return _correspondenceType; }
			set
			{
				if (Set(() => CorrespondenceType, ref _correspondenceType, value) && _projectService.Project.VarietyPairs.Count > 0)
					GenerateCorrespondences();
			}
		}

		public IBidirectionalGraph<GridVertex, GlobalCorrespondenceEdge> Graph
		{
			get { return _graph; }
			set { Set(() => Graph, ref _graph, value); }
		}

		public bool GeneratingCorrespondences
		{
			get { return _generatingCorrespondences; }
			set { Set(() => GeneratingCorrespondences, ref _generatingCorrespondences, value); }
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
