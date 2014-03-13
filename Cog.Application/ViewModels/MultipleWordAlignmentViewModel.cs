using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Application.Collections;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.Clusterers;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Application.ViewModels
{
	public class MultipleWordAlignmentViewModel : WorkspaceViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly BindableList<MultipleWordAlignmentWordViewModel> _words;
		private ICollectionView _wordsView;
		private MirroredBindableList<Meaning, MeaningViewModel> _meanings;
		private ICollectionView _meaningsView;
		private MeaningViewModel _selectedMeaning;
		private int _columnCount;
		private int _selectedColumn;
		private MultipleWordAlignmentWordViewModel _selectedWord;
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
						new TaskAreaCommandViewModel("Variety", new RelayCommand(() => SortBy("Variety", ListSortDirection.Ascending)))),
					showCognateSets)));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export all cognate sets", new RelayCommand(ExportCognateSets))));

			_words = new BindableList<MultipleWordAlignmentWordViewModel>();

			_groupByCognateSet = true;

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => AlignWords());
			Messenger.Default.Register<DomainModelChangedMessage>(this, msg =>
				{
					if (msg.AffectsComparison)
						ResetAlignment();
				});
			Messenger.Default.Register<PerformingComparisonMessage>(this, msg => ResetAlignment());
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			Set("Meanings", ref _meanings, new MirroredBindableList<Meaning, MeaningViewModel>(_projectService.Project.Meanings, meaning => new MeaningViewModel(meaning), vm => vm.DomainMeaning));
		}

		private void ExportCognateSets()
		{
			if (_projectService.AreAllVarietiesCompared)
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

		private void MeaningsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_selectedMeaning == null || !_meanings.Contains(_selectedMeaning))
				SelectedMeaning = _meanings.Count > 0 ? _meaningsView.Cast<MeaningViewModel>().First() : null;
		}

		public ReadOnlyObservableList<MeaningViewModel> Meanings
		{
			get { return _meanings; }
		}

		public ICollectionView MeaningsView
		{
			get { return _meaningsView; }
			set
			{
				if (Set(() => MeaningsView, ref _meaningsView, value))
				{
					_meaningsView.SortDescriptions.Add(new SortDescription("Gloss", ListSortDirection.Ascending));
					_meaningsView.CollectionChanged += MeaningsChanged;
					SelectedMeaning = _meanings.Count > 0 ? _meaningsView.Cast<MeaningViewModel>().First() : null;
				}
			}
		}

		public int ColumnCount
		{
			get { return _columnCount; }
			set { Set(() => ColumnCount, ref _columnCount, value); }
		}

		public MeaningViewModel SelectedMeaning
		{
			get { return _selectedMeaning; }
			set 
			{
				if (Set(() => SelectedMeaning, ref _selectedMeaning, value))
				{
					if (_selectedMeaning != null && _projectService.AreAllVarietiesCompared)
						AlignWords();
					else
						ResetAlignment();
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

		public int SelectedColumn
		{
			get { return _selectedColumn; }
			set { Set(() => SelectedColumn, ref _selectedColumn, value); }
		}

		public MultipleWordAlignmentWordViewModel SelectedWord
		{
			get { return _selectedWord; }
			set { Set(() => SelectedWord, ref _selectedWord, value); }
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
			SelectedColumn = 0;
			SelectedWord = null;
		}

		private void AlignWords()
		{
			if (_selectedMeaning == null)
				return;

			_busyService.ShowBusyIndicatorUntilFinishDrawing();

			List<Word> words = _projectService.Project.Varieties.SelectMany(v => v.Words[_selectedMeaning.DomainMeaning]).ToList();
			if (words.Count == 0)
			{
				_words.Clear();
				return;
			}

			IWordAligner aligner = _projectService.Project.WordAligners[ComponentIdentifiers.PrimaryWordAligner];
			Alignment<Word, ShapeNode> alignment;
			if (words.Count == 1)
			{
				Word word = words[0];
				Annotation<ShapeNode> prefixAnn = word.Prefix;
				var prefix = new AlignmentCell<ShapeNode>(prefixAnn != null ? word.Shape.GetNodes(prefixAnn.Span).Where(NodeFilter) : Enumerable.Empty<ShapeNode>());
				IEnumerable<AlignmentCell<ShapeNode>> columns = word.Shape.GetNodes(word.Stem.Span).Where(NodeFilter).Select(n => new AlignmentCell<ShapeNode>(n));
				Annotation<ShapeNode> suffixAnn = word.Suffix;
				var suffix = new AlignmentCell<ShapeNode>(suffixAnn != null ? word.Shape.GetNodes(suffixAnn.Span).Where(NodeFilter) : Enumerable.Empty<ShapeNode>());
				alignment = new Alignment<Word, ShapeNode>(0, 0, Tuple.Create(word, prefix, columns, suffix));
			}
			else
			{
				IWordAlignerResult result = aligner.Compute(words);
				alignment = result.GetAlignments().First();
			}

			List<Cluster<Word>> cognateSets = _projectService.Project.GenerateCognateSets(_selectedMeaning.DomainMeaning).ToList();
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
					int cognateSetIndex = cognateSets.FindIndex(set => set.DataObjects.Contains(word)) + 1;
					_words.Add(new MultipleWordAlignmentWordViewModel(word, prefix, columns, suffix, cognateSetIndex));
				}
			}
		}

		private bool NodeFilter(ShapeNode n)
		{
			return n.Type().IsOneOf(CogFeatureSystem.ConsonantType, CogFeatureSystem.VowelType, CogFeatureSystem.AnchorType);
		}

		public ObservableList<MultipleWordAlignmentWordViewModel> Words
		{
			get { return _words; }
		}
	}
}
