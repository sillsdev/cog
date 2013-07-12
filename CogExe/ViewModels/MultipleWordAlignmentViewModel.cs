using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Clusterers;
using SIL.Cog.Services;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class MultipleWordAlignmentViewModel : WorkspaceViewModelBase
	{
		private readonly BindableList<MultipleWordAlignmentWordViewModel> _words;
		private readonly ListCollectionView _wordsView;
		private ReadOnlyMirroredList<Sense, SenseViewModel> _senses;
		private ListCollectionView _sensesView;
		private SenseViewModel _currentSense;
		private CogProject _project;
		private int _columnCount;
		private int _currentColumn;
		private MultipleWordAlignmentWordViewModel _currentWord;
		private readonly IBusyService _busyService;
		private readonly IExportService _exportService;

		public MultipleWordAlignmentViewModel(IBusyService busyService, IExportService exportService)
			: base("Multiple Word Alignment")
		{
			var showCognateSets = new TaskAreaBooleanViewModel("Show cognate sets") {Value = true};
			showCognateSets.PropertyChanged += _showCognateSets_PropertyChanged;
			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
				new TaskAreaItemsViewModel("Sort words by",
					new TaskAreaCommandGroupViewModel(
						new TaskAreaCommandViewModel("Variety", new RelayCommand(() => SortBy("Variety.Name"))),
						new TaskAreaCommandViewModel("Word", new RelayCommand(() => SortBy("StrRep")))),
					showCognateSets)));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export all cognate sets", new RelayCommand(ExportCognateSets))));

			_busyService = busyService;
			_exportService = exportService;

			_words = new BindableList<MultipleWordAlignmentWordViewModel>();
			_wordsView = new ListCollectionView(_words);
			Debug.Assert(_wordsView.GroupDescriptions != null);
			_wordsView.GroupDescriptions.Add(new PropertyGroupDescription("CognateSetIndex"));
			_wordsView.SortDescriptions.Add(new SortDescription("CognateSetIndex", ListSortDirection.Ascending));
			_wordsView.SortDescriptions.Add(new SortDescription("Variety.Name", ListSortDirection.Ascending));

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => AlignWords());
			Messenger.Default.Register<ModelChangingMessage>(this, msg => ResetAlignment());
		}

		private void ExportCognateSets()
		{
			if (_project.VarietyPairs.Count == 0)
				return;

			_exportService.ExportCognateSets(this, _project);
		}

		private void _showCognateSets_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Value":
					Debug.Assert(_wordsView.GroupDescriptions != null);
					if (_wordsView.GroupDescriptions.Count > 0)
					{
						_wordsView.GroupDescriptions.Clear();
						_wordsView.SortDescriptions.RemoveAt(0);
					}
					else
					{
						_wordsView.GroupDescriptions.Add(new PropertyGroupDescription("CognateSetIndex"));
						_wordsView.SortDescriptions.Insert(0, new SortDescription("CognateSetIndex", ListSortDirection.Ascending));
					}
					break;
			}
		}

		private void SortBy(string property)
		{
			Debug.Assert(_wordsView.GroupDescriptions != null);
			_wordsView.SortDescriptions[_wordsView.GroupDescriptions.Count > 0 ? 1 : 0] = new SortDescription(property, ListSortDirection.Ascending);
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			Set("Senses", ref _senses, new ReadOnlyMirroredList<Sense, SenseViewModel>(_project.Senses, sense => new SenseViewModel(sense), vm => vm.ModelSense));
			Set("SensesView", ref _sensesView, new ListCollectionView(_senses) {SortDescriptions = {new SortDescription("Gloss", ListSortDirection.Ascending)}});
			((INotifyCollectionChanged) _sensesView).CollectionChanged += SensesChanged;
			CurrentSense = _senses.Count > 0 ? (SenseViewModel) _sensesView.GetItemAt(0) : null;
		}

		private void SensesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_currentSense == null || !_senses.Contains(_currentSense))
				CurrentSense = _senses.Count > 0 ? (SenseViewModel) _sensesView.GetItemAt(0) : null;
		}

		public ReadOnlyObservableList<SenseViewModel> Senses
		{
			get { return _senses; }
		}

		public ICollectionView SensesView
		{
			get { return _sensesView; }
		}

		public int ColumnCount
		{
			get { return _columnCount; }
			set { Set(() => ColumnCount, ref _columnCount, value); }
		}

		public SenseViewModel CurrentSense
		{
			get { return _currentSense; }
			set 
			{
				if (Set(() => CurrentSense, ref _currentSense, value))
				{
					if (_currentSense == null)
						ResetAlignment();
					else
						AlignWords();
				}
			}
		}

		public ICollectionView WordsView
		{
			get { return _wordsView; }
		}

		public int CurrentColumn
		{
			get { return _currentColumn; }
			set { Set(() => CurrentColumn, ref _currentColumn, value); }
		}

		public MultipleWordAlignmentWordViewModel CurrentWord
		{
			get { return _currentWord; }
			set { Set(() => CurrentWord, ref _currentWord, value); }
		}

		private void ResetAlignment()
		{
			_words.Clear();
			ColumnCount = 0;
			CurrentColumn = 0;
			CurrentWord = null;
		}

		private void AlignWords()
		{
			if (_project.VarietyPairs.Count == 0)
				return;

			_busyService.ShowBusyIndicatorUntilUpdated();

			var clusterer = new CognateSetsClusterer(_currentSense.ModelSense, 0.5);
			List<Cluster<Variety>> cognateSets = clusterer.GenerateClusters(_project.Varieties).ToList();

			var words = new List<Word>();
			foreach (Variety variety in _project.Varieties)
			{
				IReadOnlyCollection<Word> varietyWords = variety.Words[_currentSense.ModelSense];
				if (varietyWords.Count == 0)
					continue;

				if (varietyWords.Count == 1)
				{
					Word word = varietyWords.First();
					if (word.Shape.Count > 0)
						words.Add(word);
				}
				else
				{
					Cluster<Variety> cognateSet = cognateSets.Single(set => set.DataObjects.Contains(variety));
					var wordCounts = new Dictionary<Word, int>();
					foreach (Variety otherVariety in cognateSet.DataObjects.Where(v => v != variety))
					{
						VarietyPair vp = variety.VarietyPairs[otherVariety];
						WordPair wp;
						if (vp.WordPairs.TryGetValue(_currentSense.ModelSense, out wp))
							wordCounts.UpdateValue(wp.GetWord(variety), () => 0, c => c + 1);
					}
					if (wordCounts.Count > 0)
						words.Add(wordCounts.MaxBy(kvp => kvp.Value).Key);
				}
			}

			IWordAligner aligner = _project.WordAligners["primary"];
			IWordAlignerResult result = aligner.Compute(words);
			Alignment<Word, ShapeNode> alignment = result.GetAlignments().First();

			ColumnCount = alignment.ColumnCount;
			using (_words.BulkUpdate())
			{
				_words.Clear();
				for (int i = 0; i < alignment.SequenceCount; i++)
				{
					AlignmentCell<ShapeNode> prefix = alignment.Prefixes[i];
					Word word = alignment.Sequences[i];
					IEnumerable<AlignmentCell<ShapeNode>> columns = Enumerable.Range(0, alignment.ColumnCount).Select(col => alignment[i, col]);
					AlignmentCell<ShapeNode> suffix = alignment.Suffixes[i];
					int cognateSetIndex = cognateSets.FindIndex(set => set.DataObjects.Contains(word.Variety)) + 1;
					_words.Add(new MultipleWordAlignmentWordViewModel(word, prefix, columns, suffix, cognateSetIndex));
				}
			}
		}

		public ObservableList<MultipleWordAlignmentWordViewModel> Words
		{
			get { return _words; }
		}
	}
}
