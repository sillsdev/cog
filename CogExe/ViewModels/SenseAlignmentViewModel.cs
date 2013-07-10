using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Services;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class SenseAlignmentViewModel : WorkspaceViewModelBase
	{
		private readonly BindableList<SenseAlignmentWordViewModel> _words; 
		private ReadOnlyMirroredList<Sense, SenseViewModel> _senses;
		private ListCollectionView _sensesView;
		private SenseViewModel _currentSense;
		private CogProject _project;
		private int _columnCount;
		private int _currentColumn;
		private SenseAlignmentWordViewModel _currentWord;
		private readonly IBusyService _busyService;

		public SenseAlignmentViewModel(IBusyService busyService)
			: base("Sense Alignment")
		{
			_busyService = busyService;
			_words = new BindableList<SenseAlignmentWordViewModel>();
			Messenger.Default.Register<Message>(this, HandleMessage);
		}

		private void HandleMessage(Message msg)
		{
			switch (msg.Type)
			{
				case MessageType.ComparisonPerformed:
					AlignWords();
					break;

				case MessageType.ComparisonInvalidated:
					ResetAlignment();
					break;
			}
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
			if (_currentSense == null && _senses.Count > 0)
				CurrentSense = (SenseViewModel) _sensesView.GetItemAt(0);
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

		public int CurrentColumn
		{
			get { return _currentColumn; }
			set { Set(() => CurrentColumn, ref _currentColumn, value); }
		}

		public SenseAlignmentWordViewModel CurrentWord
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
			IWordAligner aligner = _project.WordAligners["primary"];
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
					var wordCounts = new Dictionary<Word, int>();
					foreach (VarietyPair vp in variety.VarietyPairs)
					{
						WordPair wp;
						if (vp.WordPairs.TryGetValue(_currentSense.ModelSense, out wp))
							wordCounts.UpdateValue(wp.GetWord(variety), () => 0, c => c + 1);
					}
					if (wordCounts.Count > 0)
						words.Add(wordCounts.MaxBy(kvp => kvp.Value).Key);
				}
			}
			IWordAlignerResult result = aligner.Compute(words);
			Alignment<Word, ShapeNode> alignment = result.GetAlignments().First();

			ColumnCount = alignment.ColumnCount;
			using (_words.BulkUpdate())
			{
				_words.Clear();
				_words.AddRange(Enumerable.Range(0, alignment.SequenceCount)
					.Select(seq => new SenseAlignmentWordViewModel(alignment.Sequences[seq], alignment.Prefixes[seq], Enumerable.Range(0, alignment.ColumnCount).Select(col => alignment[seq, col]), alignment.Suffixes[seq])));
			}
		}

		public ObservableList<SenseAlignmentWordViewModel> Words
		{
			get { return _words; }
		}
	}
}
