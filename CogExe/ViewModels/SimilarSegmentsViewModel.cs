using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Threading;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Collections;
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

	public class SimilarSegmentsViewModel : WorkspaceViewModelBase
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
		private readonly BulkObservableCollection<GlobalSegmentViewModel> _globalSegments; 
		private readonly WordPairsViewModel _wordPairs;
		private GlobalCorrespondenceViewModel _selectedCorrespondence;
		private readonly BackgroundWorker _generateCorrespondencesWorker;
		private bool _restartWork;
		private readonly ManualResetEvent _workCompleteEvent;
		private bool _generatingCorrespondences;
		private SoundCorrespondenceType _correspondenceType;
		private readonly BulkObservableCollection<GlobalCorrespondenceViewModel> _globalCorrespondences;
		private readonly TaskAreaIntegerViewModel _correspondenceFilter;

		public SimilarSegmentsViewModel()
			: base("Similar Segments")
		{
			_globalSegments = new BulkObservableCollection<GlobalSegmentViewModel>();
			_globalCorrespondences = new BulkObservableCollection<GlobalCorrespondenceViewModel>();
			Messenger.Default.Register<NotificationMessage>(this, HandleNotificationMessage);
			_generateCorrespondencesWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
			_generateCorrespondencesWorker.DoWork += GenerateCorrespondencesAsync;
			_generateCorrespondencesWorker.RunWorkerCompleted += GenerateCorrespondencesAsyncFinished;
			_workCompleteEvent = new ManualResetEvent(true);
			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Correspondence type",
				new CommandViewModel("Initial consonants", new RelayCommand(() => CorrespondenceType = SoundCorrespondenceType.InitialConsonants)),
				new CommandViewModel("Medial consonants", new RelayCommand(() => CorrespondenceType = SoundCorrespondenceType.MedialConsonants)),
				new CommandViewModel("Final consonants", new RelayCommand(() => CorrespondenceType = SoundCorrespondenceType.FinalConsonants)),
				new CommandViewModel("Vowels", new RelayCommand(() => CorrespondenceType = SoundCorrespondenceType.Vowels))));
			_correspondenceFilter = new TaskAreaIntegerViewModel("Correspondence filter");
			_correspondenceFilter.PropertyChanging += _correspondenceFilter_PropertyChanging;
			_correspondenceFilter.PropertyChanged += _correspondenceFilter_PropertyChanged;
			TaskAreas.Add(_correspondenceFilter);
			_wordPairs = new WordPairsViewModel();
		}

		private void GenerateCorrespondencesAsync(object sender, DoWorkEventArgs e)
		{
			_workCompleteEvent.Reset();

			var correspondenceType = (SoundCorrespondenceType) e.Argument;

			var segs = new Dictionary<object, Dictionary<string, SegmentInfo>>();
			var corrs = new Dictionary<UnorderedTuple<object, object>, CorrespondenceInfo>();
			int maxFreq = 0;
			foreach (WordPair wp in _project.VarietyPairs.SelectMany(vp => vp.WordPairs).Where(wp => wp.AreCognatePredicted))
			{
				if (_generateCorrespondencesWorker.CancellationPending)
					break;

				var vm = new WordPairViewModel(_project, wp, true);

				switch (correspondenceType)
				{
					case SoundCorrespondenceType.InitialConsonants:
						AlignedNodeViewModel initialNode = vm.AlignedNodes[0];
						maxFreq = Math.Max(AddConsonantCorrespondence(vm, segs, corrs, initialNode), maxFreq);
						break;

					case SoundCorrespondenceType.MedialConsonants:
						foreach (AlignedNodeViewModel node in vm.AlignedNodes.Skip(1).Take(vm.AlignedNodes.Count - 2))
						{
							if (_generateCorrespondencesWorker.CancellationPending)
								break;

							maxFreq = Math.Max(AddConsonantCorrespondence(vm, segs, corrs, node), maxFreq);
						}
						break;

					case SoundCorrespondenceType.FinalConsonants:
						AlignedNodeViewModel finalNode = vm.AlignedNodes[vm.AlignedNodes.Count - 1];
						maxFreq = Math.Max(AddConsonantCorrespondence(vm, segs, corrs, finalNode), maxFreq);
						break;

					case SoundCorrespondenceType.Vowels:
						foreach (AlignedNodeViewModel node in vm.AlignedNodes)
						{
							if (_generateCorrespondencesWorker.CancellationPending)
								break;

							maxFreq = Math.Max(AddVowelCorrespondence(vm, segs, corrs, node), maxFreq);
						}
						break;
				}
			}

			if (_generateCorrespondencesWorker.CancellationPending)
				e.Cancel = true;
			else
				e.Result = Tuple.Create(segs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Values.MaxBy(si => si.Frequency).GlobalSegment), corrs, maxFreq);
			_workCompleteEvent.Set();
		}

		private static int AddConsonantCorrespondence(WordPairViewModel wp, Dictionary<object, Dictionary<string, SegmentInfo>> segs,
			Dictionary<UnorderedTuple<object, object>, CorrespondenceInfo> corrs, AlignedNodeViewModel node)
		{
			int freq = 0;
			if (node.Annotation1.Type() == CogFeatureSystem.ConsonantType && node.Annotation2.Type() == CogFeatureSystem.ConsonantType)
			{
				Ngram ngram1 = node.Annotation1.Ngram(wp.ModelWordPair.VarietyPair.Variety1);
				Ngram ngram2 = node.Annotation2.Ngram(wp.ModelWordPair.VarietyPair.Variety2);
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
						freq = ci.Frequency;
					}
				}
			}

			return freq;
		}

		private static bool GetConsonant(Dictionary<object, Dictionary<string, SegmentInfo>> consonants, Segment consonant, out object key)
		{
			string strRep = RemoveDiacritics(consonant.StrRep);
			if (strRep.Length > 1)
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
			Dictionary<string, SegmentInfo> segInfos = consonants.GetValue(key, () => new Dictionary<string, SegmentInfo>());
			SegmentInfo si = segInfos.GetValue(strRep, () => new SegmentInfo(new ConsonantGlobalSegmentViewModel(strRep, place, manner, voice)));
			si.Frequency++;
			return true;
		}

		private static int AddVowelCorrespondence(WordPairViewModel wp, Dictionary<object, Dictionary<string, SegmentInfo>> segs,
			Dictionary<UnorderedTuple<object, object>, CorrespondenceInfo> corrs, AlignedNodeViewModel node)
		{
			int freq = 0;
			if (node.Annotation1.Type() == CogFeatureSystem.VowelType && node.Annotation2.Type() == CogFeatureSystem.VowelType)
			{
				Ngram ngram1 = node.Annotation1.Ngram(wp.ModelWordPair.VarietyPair.Variety1);
				Ngram ngram2 = node.Annotation2.Ngram(wp.ModelWordPair.VarietyPair.Variety2);
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
						freq = ci.Frequency;
					}
				}
			}

			return freq;
		}

		private static bool GetVowel(Dictionary<object, Dictionary<string, SegmentInfo>> vowels, Segment vowel, out object key)
		{
			string strRep = RemoveDiacritics(vowel.StrRep);
			if (strRep.Length > 1)
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
			Dictionary<string, SegmentInfo> segInfos = vowels.GetValue(key, () => new Dictionary<string, SegmentInfo>());
			SegmentInfo si = segInfos.GetValue(strRep, () => new SegmentInfo(new VowelGlobalSegmentViewModel(strRep, height, backness, round)));
			si.Frequency++;
			return true;
		}

		private static string RemoveDiacritics(string str)
		{
			var sb = new StringBuilder();
			foreach (char c in str)
			{
				switch (CharUnicodeInfo.GetUnicodeCategory(c))
				{
					case UnicodeCategory.NonSpacingMark:
					case UnicodeCategory.SpacingCombiningMark:
					case UnicodeCategory.EnclosingMark:
						break;

					default:
						sb.Append(c);
						break;
				}
			}
			return sb.ToString();
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
					var results = (Tuple<Dictionary<object, GlobalSegmentViewModel>, Dictionary<UnorderedTuple<object, object>, CorrespondenceInfo>, int>) e.Result;
					_globalSegments.AddRange(results.Item1.Values);
					_globalCorrespondences.AddRange(results.Item2
						.Select(kvp => new GlobalCorrespondenceViewModel(results.Item1[kvp.Key.Item1], results.Item1[kvp.Key.Item2], kvp.Value.Frequency,
							(double) kvp.Value.Frequency / results.Item3, kvp.Value.WordPairs)));
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

		private void HandleNotificationMessage(NotificationMessage msg)
		{
			switch (msg.Notification)
			{
				case Notifications.PerformingComparison:
					CancelWorker();
					break;
				case Notifications.ComparisonPerformed:
					GenerateCorrespondences();
					break;
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
					if (_selectedCorrespondence != null)
					{
						_selectedCorrespondence.IsSelected = true;

						foreach (WordPairViewModel wp in _selectedCorrespondence.WordPairs)
						{
							foreach (AlignedNodeViewModel an in wp.AlignedNodes)
							{
								string strRep1 = RemoveDiacritics(an.StrRep1);
								string strRep2 = RemoveDiacritics(an.StrRep2);
								an.IsSelected = strRep1 != strRep2
									&& (strRep1 == _selectedCorrespondence.Segment1.StrRep || strRep1 == _selectedCorrespondence.Segment2.StrRep)
								    && (strRep2 == _selectedCorrespondence.Segment1.StrRep || strRep2 == _selectedCorrespondence.Segment2.StrRep);
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

		public ObservableCollection<GlobalSegmentViewModel> GlobalSegments
		{
			get { return _globalSegments; }
		}

		public ObservableCollection<GlobalCorrespondenceViewModel> GlobalCorrespondences
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

		private class SegmentInfo
		{
			private readonly GlobalSegmentViewModel _globalSegment;

			public SegmentInfo(GlobalSegmentViewModel globalSegment)
			{
				_globalSegment = globalSegment;
			}

			public GlobalSegmentViewModel GlobalSegment
			{
				get { return _globalSegment; }
			}

			public int Frequency { get; set; }
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
