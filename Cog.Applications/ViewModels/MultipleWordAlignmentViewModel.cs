using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Clusterers;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.Applications.ViewModels
{
	public class MultipleWordAlignmentViewModel : WorkspaceViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly BindableList<MultipleWordAlignmentWordViewModel> _words;
		private ICollectionView _wordsView;
		private ReadOnlyMirroredList<Sense, SenseViewModel> _senses;
		private ICollectionView _sensesView;
		private SenseViewModel _currentSense;
		private int _columnCount;
		private int _currentColumn;
		private MultipleWordAlignmentWordViewModel _currentWord;
		private readonly IBusyService _busyService;
		private readonly IExportService _exportService;
		private bool _groupByCognateSet;

		public MultipleWordAlignmentViewModel(IProjectService projectService, IBusyService busyService, IExportService exportService)
			: base("Multiple Word Alignment")
		{
			_projectService = projectService;
			_busyService = busyService;
			_exportService = exportService;

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			var showCognateSets = new TaskAreaBooleanViewModel("Show cognate sets") {Value = true};
			showCognateSets.PropertyChanged += _showCognateSets_PropertyChanged;
			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
				new TaskAreaItemsViewModel("Sort words by",
					new TaskAreaCommandGroupViewModel(
						new TaskAreaCommandViewModel("Form", new RelayCommand(() => SortBy("StrRep", ListSortDirection.Ascending))),
						new TaskAreaCommandViewModel("Variety", new RelayCommand(() => SortBy("Variety.Name", ListSortDirection.Ascending)))),
					showCognateSets)));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export all cognate sets", new RelayCommand(ExportCognateSets))));

			_words = new BindableList<MultipleWordAlignmentWordViewModel>();

			_groupByCognateSet = true;

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => AlignWords());
			Messenger.Default.Register<DomainModelChangingMessage>(this, msg => ResetAlignment());
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			Set("Senses", ref _senses, new ReadOnlyMirroredList<Sense, SenseViewModel>(_projectService.Project.Senses, sense => new SenseViewModel(sense), vm => vm.DomainSense));
		}

		private void ExportCognateSets()
		{
			if (_projectService.Project.VarietyPairs.Count == 0)
				return;

			_exportService.ExportCognateSets(this);
		}

		private void _showCognateSets_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Value":
					GroupByCognateSet = !GroupByCognateSet;
					break;
			}
		}

		private void SortBy(string property, ListSortDirection sortDirection)
		{
			_wordsView.SortDescriptions[_groupByCognateSet ? 1 : 0] = new SortDescription(property, sortDirection);
		}

		private void SensesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_currentSense == null || !_senses.Contains(_currentSense))
				CurrentSense = _senses.Count > 0 ? _sensesView.Cast<SenseViewModel>().First() : null;
		}

		public ReadOnlyObservableList<SenseViewModel> Senses
		{
			get { return _senses; }
		}

		public ICollectionView SensesView
		{
			get { return _sensesView; }
			set
			{
				if (Set(() => SensesView, ref _sensesView, value))
				{
					_sensesView.SortDescriptions.Add(new SortDescription("Gloss", ListSortDirection.Ascending));
					_sensesView.CollectionChanged += SensesChanged;
					CurrentSense = _senses.Count > 0 ? _sensesView.Cast<SenseViewModel>().First() : null;
				}
			}
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
					if (_currentSense == null || _projectService.Project.VarietyPairs.Count == 0)
						ResetAlignment();
					else
						AlignWords();
				}
			}
		}

		public ICollectionView WordsView
		{
			get { return _wordsView; }
			set
			{
				if (Set(() => WordsView, ref _wordsView, value))
				{
					if (_groupByCognateSet)
						_wordsView.SortDescriptions.Add(new SortDescription("CognateSetIndex", ListSortDirection.Ascending));
					_wordsView.SortDescriptions.Add(new SortDescription("StrRep", ListSortDirection.Ascending));
				}
			}
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

		public bool GroupByCognateSet
		{
			get { return _groupByCognateSet; }
			set
			{
				if (Set(() => GroupByCognateSet, ref _groupByCognateSet, value))
				{
					if (_groupByCognateSet)
						_wordsView.SortDescriptions.Insert(0, new SortDescription("CognateSetIndex", ListSortDirection.Ascending));
					else
						_wordsView.SortDescriptions.RemoveAt(0);
				}
			}
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
			_busyService.ShowBusyIndicatorUntilUpdated();

			var clusterer = new CognateSetsClusterer(_currentSense.DomainSense, 0.5);
			List<Cluster<Variety>> cognateSets = clusterer.GenerateClusters(_projectService.Project.Varieties).ToList();

			var words = new List<Word>();
			foreach (Variety variety in _projectService.Project.Varieties)
			{
				IReadOnlyCollection<Word> varietyWords = variety.Words[_currentSense.DomainSense];
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
						if (vp.WordPairs.TryGetValue(_currentSense.DomainSense, out wp))
							wordCounts.UpdateValue(wp.GetWord(variety), () => 0, c => c + 1);
					}
					if (wordCounts.Count > 0)
						words.Add(wordCounts.MaxBy(kvp => kvp.Value).Key);
				}
			}

			IWordAligner aligner = _projectService.Project.WordAligners["primary"];
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
