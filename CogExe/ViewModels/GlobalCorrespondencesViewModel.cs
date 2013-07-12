using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Services;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
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
		private static readonly Dictionary<string, VowelHeight> VowelHeightLookup = new Dictionary<string, VowelHeight>
			{
				{"close", VowelHeight.Close},
				{"near-close", VowelHeight.NearClose},
				{"close-mid", VowelHeight.CloseMid},
				{"mid", VowelHeight.Mid},
				{"open-mid", VowelHeight.OpenMid},
				{"near-open", VowelHeight.NearOpen},
				{"open", VowelHeight.Open}
			};

		private static readonly Dictionary<string, VowelBackness> VowelBacknessLookup = new Dictionary<string, VowelBackness>
			{
				{"front", VowelBackness.Front},
				{"near-front", VowelBackness.NearFront},
				{"central", VowelBackness.Central},
				{"near-back", VowelBackness.NearBack},
				{"back", VowelBackness.Back}
			};

		private static readonly Dictionary<string, ConsonantPlace> ConsonantPlaceLookup = new Dictionary<string, ConsonantPlace>
			{
				{"bilabial", ConsonantPlace.Bilabial},
				{"labiodental", ConsonantPlace.Labiodental},
				{"dental", ConsonantPlace.Dental},
				{"alveolar", ConsonantPlace.Alveolar},
				{"palato-alveolar", ConsonantPlace.Postaveolar},
				{"retroflex", ConsonantPlace.Retroflex},
				{"palatal", ConsonantPlace.Palatal},
				{"velar", ConsonantPlace.Velar},
				{"uvular", ConsonantPlace.Uvular},
				{"pharyngeal", ConsonantPlace.Pharyngeal},
				{"glottal", ConsonantPlace.Glottal}
			};

		private static readonly Dictionary<string, ConsonantManner> ConsonantMannerLookup = new Dictionary<string, ConsonantManner>
			{
				{"stop", ConsonantManner.Plosive},
				{"fricative", ConsonantManner.Fricative},
				{"approximant", ConsonantManner.Approximant},
				{"trill", ConsonantManner.Trill},
				{"flap", ConsonantManner.TapOrFlap}
			};

		private CogProject _project;
		private readonly BindableList<GlobalSegmentViewModel> _globalSegments; 
		private readonly WordPairsViewModel _wordPairs;
		private GlobalCorrespondenceViewModel _selectedCorrespondence;
		private readonly BackgroundWorker _generateCorrespondencesWorker;
		private bool _restartWork;
		private readonly ManualResetEvent _workCompleteEvent;
		private bool _generatingCorrespondences;
		private SoundCorrespondenceType _correspondenceType;
		private readonly BindableList<GlobalCorrespondenceViewModel> _globalCorrespondences;
		private readonly TaskAreaIntegerViewModel _correspondenceFilter;
		private readonly IDialogService _dialogService;
		private readonly IBusyService _busyService;

		private FindViewModel _findViewModel;
		private WordPairViewModel _startWordPair;
		private readonly SimpleMonitor _selectedWordPairsMonitor;

		public GlobalCorrespondencesViewModel(IBusyService busyService, IDialogService dialogService)
			: base("Global Correspondences")
		{
			_busyService = busyService;
			_dialogService = dialogService;

			_globalSegments = new BindableList<GlobalSegmentViewModel>();
			_globalCorrespondences = new BindableList<GlobalCorrespondenceViewModel>();

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => GenerateCorrespondences());
			Messenger.Default.Register<ModelChangingMessage>(this, msg => CancelWorker());
			Messenger.Default.Register<ViewChangedMessage>(this, HandleViewChanged);

			_selectedWordPairsMonitor = new SimpleMonitor();

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
				new TaskAreaCommandViewModel("Find words", new RelayCommand(Find))));
			_wordPairs = new WordPairsViewModel();
			_wordPairs.SelectedWordPairs.CollectionChanged += SelectedWordPairs_CollectionChanged;
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
						match = curWordPair.ModelWordPair.Word1.StrRep.Contains(_findViewModel.String)
							|| curWordPair.ModelWordPair.Word2.StrRep.Contains(_findViewModel.String);
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

			var tasks = new List<Task<Result>>();
			foreach (VarietyPair vp in _project.VarietyPairs)
			{
				VarietyPair varietyPair = vp;
				tasks.Add(Task<Result>.Factory.StartNew(() =>
					{
						var segs = new Dictionary<object, GlobalSegmentViewModel>();
						var corrs = new Dictionary<UnorderedTuple<object, object>, CorrespondenceInfo>();
						foreach (WordPair wp in varietyPair.WordPairs.Where(wp => wp.AreCognatePredicted))
						{
							if (_generateCorrespondencesWorker.CancellationPending)
								break;

							var vm = new WordPairViewModel(_project, wp, true);

							switch (correspondenceType)
							{
								case SoundCorrespondenceType.InitialConsonants:
									AddConsonantCorrespondence(vm, segs, corrs, 0);
									break;

								case SoundCorrespondenceType.MedialConsonants:
									for (int column = 1; column < vm.ModelAlignment.ColumnCount - 1; column++)
									{
										if (_generateCorrespondencesWorker.CancellationPending)
											break;

										AddConsonantCorrespondence(vm, segs, corrs, column);
									}
									break;

								case SoundCorrespondenceType.FinalConsonants:
									AddConsonantCorrespondence(vm, segs, corrs, vm.ModelAlignment.ColumnCount - 1);
									break;

								case SoundCorrespondenceType.Vowels:
									for (int column = 0; column < vm.ModelAlignment.ColumnCount; column++)
									{
										if (_generateCorrespondencesWorker.CancellationPending)
											break;

										AddVowelCorrespondence(vm, segs, corrs, column);
									}
									break;
							}
						}

						return new Result(segs, corrs, 0);
					}));
			}

			if (_generateCorrespondencesWorker.CancellationPending)
			{
				e.Cancel = true;
			}
			else
			{
				var globalSegs = new Dictionary<object, GlobalSegmentViewModel>();
				var globalCorrs = new Dictionary<UnorderedTuple<object, object>, CorrespondenceInfo>();
				int maxFreq = 0;
				foreach (Task<Result> task in tasks)
				{
					if (_generateCorrespondencesWorker.CancellationPending)
						break;

					Result result = task.Result;
					foreach (KeyValuePair<object, GlobalSegmentViewModel> segKvp in result.Segments)
					{
						GlobalSegmentViewModel seg;
						if (globalSegs.TryGetValue(segKvp.Key, out seg))
							seg.StrReps.UnionWith(seg.StrReps);
						else
							globalSegs[segKvp.Key] = segKvp.Value;
					}

					foreach (KeyValuePair<UnorderedTuple<object, object>, CorrespondenceInfo> corrKvp in result.Correspondences)
					{
						CorrespondenceInfo corr;
						if (globalCorrs.TryGetValue(corrKvp.Key, out corr))
						{
							corr.WordPairs.AddRange(corrKvp.Value.WordPairs);
							corr.Frequency += corrKvp.Value.Frequency;
							if (corr.Frequency > maxFreq)
								maxFreq = corr.Frequency;
						}
						else
						{
							globalCorrs[corrKvp.Key] = corrKvp.Value;
						}
					}
				}

				if (_generateCorrespondencesWorker.CancellationPending)
					e.Cancel = true;
				else
					e.Result = new Result(globalSegs, globalCorrs, maxFreq);
			}
			_workCompleteEvent.Set();
		}

		private class Result
		{
			private readonly Dictionary<object, GlobalSegmentViewModel> _segments;
			private readonly Dictionary<UnorderedTuple<object, object>, CorrespondenceInfo> _correspondences;
			private readonly int _maxFrequency;

			public Result(Dictionary<object, GlobalSegmentViewModel> segments, Dictionary<UnorderedTuple<object, object>, CorrespondenceInfo> correspondences, int maxFrequency)
			{
				_segments = segments;
				_correspondences = correspondences;
				_maxFrequency = maxFrequency;
			}

			public Dictionary<object, GlobalSegmentViewModel> Segments
			{
				get { return _segments; }
			}

			public Dictionary<UnorderedTuple<object, object>, CorrespondenceInfo> Correspondences
			{
				get { return _correspondences; }
			}

			public int MaxFrequency
			{
				get { return _maxFrequency; }
			}
		}

		private static void AddConsonantCorrespondence(WordPairViewModel wp, Dictionary<object, GlobalSegmentViewModel> segs,
			Dictionary<UnorderedTuple<object, object>, CorrespondenceInfo> corrs, int column)
		{
			AlignmentCell<ShapeNode> cell1 = wp.ModelAlignment[0, column];
			AlignmentCell<ShapeNode> cell2 = wp.ModelAlignment[1, column];
			if (!cell1.IsNull && cell1.First.Type() == CogFeatureSystem.ConsonantType && !cell2.IsNull && cell2.First.Type() == CogFeatureSystem.ConsonantType)
			{
				Ngram ngram1 = cell1.ToNgram(wp.ModelWordPair.VarietyPair.Variety1.SegmentPool);
				Ngram ngram2 = cell2.ToNgram(wp.ModelWordPair.VarietyPair.Variety2.SegmentPool);
				if (ngram1.Count == 1 && ngram2.Count == 1)
				{
					Segment seg1 = ngram1.First;
					Segment seg2 = ngram2.First;
					object key1, key2;
					if (GetConsonant(segs, seg1, out key1) && GetConsonant(segs, seg2, out key2) && !key1.Equals(key2))
					{
						CorrespondenceInfo ci = corrs.GetValue(UnorderedTuple.Create(key1, key2), () => new CorrespondenceInfo());
						ci.Frequency++;
						ci.WordPairs.Add(wp);
					}
				}
			}
		}

		private static bool GetConsonant(Dictionary<object, GlobalSegmentViewModel> consonants, Segment consonant, out object key)
		{
			if (consonant.StrRep.DisplayLength() > 1)
			{
				key = null;
				return false;
			}

			var placeSymbol = (FeatureSymbol) consonant.FeatureStruct.GetValue<SymbolicFeatureValue>("place");
			var mannerSymbol = (FeatureSymbol) consonant.FeatureStruct.GetValue<SymbolicFeatureValue>("manner");
			var voiceSymbol = (FeatureSymbol) consonant.FeatureStruct.GetValue<SymbolicFeatureValue>("voice");
			var nasalSymbol = (FeatureSymbol) consonant.FeatureStruct.GetValue<SymbolicFeatureValue>("nasal");

			ConsonantPlace place;
			if (!ConsonantPlaceLookup.TryGetValue(placeSymbol.ID, out place))
			{
				key = null;
				return false;
			}

			ConsonantManner manner;
			if (nasalSymbol.ID == "nasal+")
			{
				manner = ConsonantManner.Nasal;
			}
			else if (ConsonantMannerLookup.TryGetValue(mannerSymbol.ID, out manner))
			{
				var lateralSymbol = (FeatureSymbol) consonant.FeatureStruct.GetValue<SymbolicFeatureValue>("lateral");
				if (lateralSymbol.ID == "lateral+")
				{
					switch (manner)
					{
						case ConsonantManner.Approximant:
							manner = ConsonantManner.LateralApproximant;
							break;
						case ConsonantManner.Fricative:
							manner = ConsonantManner.LateralFricative;
							break;
					}
				}
			}
			else
			{
				key = null;
				return false;
			}

			bool voice = voiceSymbol.ID == "voice+";

			key = Tuple.Create(place, manner, voice);
			GlobalSegmentViewModel seg = consonants.GetValue(key, () => new ConsonantGlobalSegmentViewModel(place, manner, voice));
			seg.StrReps.Add(consonant.StrRep);
			return true;
		}

		private static void AddVowelCorrespondence(WordPairViewModel wp, Dictionary<object, GlobalSegmentViewModel> segs,
			Dictionary<UnorderedTuple<object, object>, CorrespondenceInfo> corrs, int column)
		{
			AlignmentCell<ShapeNode> cell1 = wp.ModelAlignment[0, column];
			AlignmentCell<ShapeNode> cell2 = wp.ModelAlignment[1, column];
			if (!cell1.IsNull && cell1.First.Type() == CogFeatureSystem.VowelType && !cell2.IsNull && cell2.First.Type() == CogFeatureSystem.VowelType)
			{
				Ngram ngram1 = cell1.ToNgram(wp.ModelWordPair.VarietyPair.Variety1.SegmentPool);
				Ngram ngram2 = cell2.ToNgram(wp.ModelWordPair.VarietyPair.Variety2.SegmentPool);
				if (ngram1.Count == 1 && ngram2.Count == 1)
				{
					Segment seg1 = ngram1.First;
					Segment seg2 = ngram2.First;
					object key1, key2;
					if (GetVowel(segs, seg1, out key1) && GetVowel(segs, seg2, out key2) && !key1.Equals(key2))
					{
						CorrespondenceInfo ci = corrs.GetValue(UnorderedTuple.Create(key1, key2), () => new CorrespondenceInfo());
						ci.Frequency++;
						ci.WordPairs.Add(wp);
					}
				}
			}
		}

		private static bool GetVowel(Dictionary<object, GlobalSegmentViewModel> vowels, Segment vowel, out object key)
		{
			if (vowel.StrRep.DisplayLength() > 1)
			{
				key = null;
				return false;
			}

			var heightSymbol = (FeatureSymbol) vowel.FeatureStruct.GetValue<SymbolicFeatureValue>("height");
			var backnessSymbol = (FeatureSymbol) vowel.FeatureStruct.GetValue<SymbolicFeatureValue>("backness");
			var roundSymbol = (FeatureSymbol) vowel.FeatureStruct.GetValue<SymbolicFeatureValue>("round");

			VowelHeight height = VowelHeightLookup[heightSymbol.ID];
			VowelBackness backness = VowelBacknessLookup[backnessSymbol.ID];
			bool round = roundSymbol.ID == "round+";

			key = Tuple.Create(height, backness, round);
			GlobalSegmentViewModel seg = vowels.GetValue(key, () => new VowelGlobalSegmentViewModel(height, backness, round));
			seg.StrReps.Add(vowel.StrRep);
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
					var results = (Result) e.Result;
					_globalSegments.AddRange(results.Segments.Values);
					_globalCorrespondences.AddRange(results.Correspondences
						.Select(kvp => new GlobalCorrespondenceViewModel(results.Segments[kvp.Key.Item1], results.Segments[kvp.Key.Item2], kvp.Value.Frequency,
							(double) kvp.Value.Frequency / results.MaxFrequency, kvp.Value.WordPairs)));
				}
			}
		}

		public override void Initialize(CogProject project)
		{
			CancelWorker();
			_project = project;
			if (_project.VarietyPairs.Count > 0)
				GenerateCorrespondences();
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

		public GlobalCorrespondenceViewModel SelectedCorrespondence
		{
			get { return _selectedCorrespondence; }
			set
			{
				GlobalCorrespondenceViewModel oldCorr = _selectedCorrespondence;
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
								an.IsSelected = (_selectedCorrespondence.Segment1.StrReps.Contains(an.StrRep1) && _selectedCorrespondence.Segment2.StrReps.Contains(an.StrRep2))
									|| (_selectedCorrespondence.Segment1.StrReps.Contains(an.StrRep2) && _selectedCorrespondence.Segment2.StrReps.Contains(an.StrRep1));
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
			_globalSegments.Clear();
			_globalCorrespondences.Clear();
			_generateCorrespondencesWorker.RunWorkerAsync(_correspondenceType);
		}

		private void CancelWorker()
		{
			if (_generateCorrespondencesWorker.IsBusy)
			{
				_generateCorrespondencesWorker.CancelAsync();
				_workCompleteEvent.WaitOne();
			}
			_globalSegments.Clear();
			_globalCorrespondences.Clear();
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
				if (Set(() => CorrespondenceType, ref _correspondenceType, value) && _project.VarietyPairs.Count > 0)
					GenerateCorrespondences();
			}
		}

		public ObservableList<GlobalSegmentViewModel> GlobalSegments
		{
			get { return _globalSegments; }
		}

		public ObservableList<GlobalCorrespondenceViewModel> GlobalCorrespondences
		{
			get { return _globalCorrespondences; }
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

		private class CorrespondenceInfo
		{
			private readonly List<WordPairViewModel> _wordPairs; 

			public CorrespondenceInfo()
			{
				_wordPairs = new List<WordPairViewModel>();
			}

			public List<WordPairViewModel> WordPairs
			{
				get { return _wordPairs; }
			}

			public int Frequency { get; set; }
		}
	}
}
