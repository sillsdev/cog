using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class SenseAlignmentViewModel : WorkspaceViewModelBase
	{
		private readonly BulkObservableList<SenseAlignmentWordViewModel> _words; 
		private ReadOnlyMirroredList<Sense, SenseViewModel> _senses;
		private SenseViewModel _currentSense;
		private CogProject _project;
		private int _columnCount;
		private int _currentColumn;
		private SenseAlignmentWordViewModel _currentWord;

		public SenseAlignmentViewModel()
			: base("Sense Alignment")
		{
			_words = new BulkObservableList<SenseAlignmentWordViewModel>();
			Messenger.Default.Register<NotificationMessage>(this, HandleNotificationMessage);
		}

		private void HandleNotificationMessage(NotificationMessage msg)
		{
			switch (msg.Notification)
			{
				case Notifications.ComparisonPerformed:
					AlignWords();
					break;
			}
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			if (_senses != null)
				_senses.CollectionChanged -= SensesChanged;
			Set("Senses", ref _senses, new ReadOnlyMirroredList<Sense, SenseViewModel>(_project.Senses, sense => new SenseViewModel(sense), vm => vm.ModelSense));
			_senses.CollectionChanged += SensesChanged;
			CurrentSense = _senses.Count > 0 ? _senses[0] : null;
		}

		private void SensesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_currentSense == null && _senses.Count > 0)
				CurrentSense = _senses[0];
		}

		public ReadOnlyObservableList<SenseViewModel> Senses
		{
			get { return _senses; }
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
					AlignWords();
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

		private void AlignWords()
		{
			if (_project.VarietyPairs.Count == 0)
				return;

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
