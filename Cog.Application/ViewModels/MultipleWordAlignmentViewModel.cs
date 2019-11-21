using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Application.Collections;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Extensions;
using SIL.Machine.Annotations;
using SIL.Machine.Clusterers;
using SIL.Machine.SequenceAlignment;
using SIL.ObjectModel;

namespace SIL.Cog.Application.ViewModels
{
	public class MultipleWordAlignmentViewModel : WorkspaceViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly IAnalysisService _analysisService;
		private readonly BindableList<MultipleWordAlignmentWordViewModel> _words;
		private readonly BindableList<MultipleWordAlignmentWordViewModel> _selectedWords; 
		private ICollectionView _wordsView;
		private MirroredBindableList<Meaning, MeaningViewModel> _meanings;
		private ICollectionView _meaningsView;
		private MeaningViewModel _selectedMeaning;
		private int _columnCount;
		private readonly IBusyService _busyService;
		private readonly IExportService _exportService;
		private bool _groupByCognateSet;
		private string _sortByProp;
		private readonly ICommand _showInVarietyPairsCommand;
		private readonly ICommand _performComparisonCommand;
		private bool _isEmpty;

		public MultipleWordAlignmentViewModel(IProjectService projectService, IBusyService busyService, IExportService exportService, IAnalysisService analysisService)
			: base("Multiple Word Alignment")
		{
			_projectService = projectService;
			_busyService = busyService;
			_exportService = exportService;
			_analysisService = analysisService;

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
				new TaskAreaCommandViewModel("Export all cognate sets", new RelayCommand(ExportCognateSets, CanExportCognateSets))));

			_words = new BindableList<MultipleWordAlignmentWordViewModel>();
			_selectedWords = new BindableList<MultipleWordAlignmentWordViewModel>();

			_showInVarietyPairsCommand = new RelayCommand(ShowInVarietyPairs, CanShowInVarietyPairs);
			_performComparisonCommand = new RelayCommand(PerformComparison);

			_groupByCognateSet = true;
			_sortByProp = "StrRep";

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => AlignWords());
			Messenger.Default.Register<DomainModelChangedMessage>(this, msg =>
			{
				if (msg.AffectsComparison)
					ResetAlignment();
			});
			Messenger.Default.Register<PerformingComparisonMessage>(this, msg => ResetAlignment());
			Messenger.Default.Register<SwitchViewMessage>(this, HandleSwitchView);
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			IsEmpty = true;
			Set("Meanings", ref _meanings, new MirroredBindableList<Meaning, MeaningViewModel>(_projectService.Project.Meanings, meaning => new MeaningViewModel(meaning), vm => vm.DomainMeaning));
			_selectedMeaning = null;
		}

		private void ShowInVarietyPairs()
		{
			VarietyPair vp = _selectedWords[0].Variety.DomainVariety.VarietyPairs[_selectedWords[1].Variety.DomainVariety];
			Messenger.Default.Send(new SwitchViewMessage(typeof(VarietyPairsViewModel), vp, _selectedMeaning.DomainMeaning));
		}

		private bool CanShowInVarietyPairs()
		{
			if (_selectedWords.Count != 2)
				return false;

			Word w1 = _selectedWords[0].DomainWord;
			Word w2 = _selectedWords[1].DomainWord;

			if (w1.Variety == w2.Variety)
				return false;

			VarietyPair vp = w1.Variety.VarietyPairs[w2.Variety];
			WordPair wp;
			if (vp.WordPairs.TryGet(_selectedMeaning.DomainMeaning, out wp))
				return wp.GetWord(w1.Variety) == w1 && wp.GetWord(w2.Variety) == w2;
			return false;
		}

		private void PerformComparison()
		{
			_analysisService.CompareAll(this);
		}

		private bool CanExportCognateSets()
		{
			return _projectService.AreAllVarietiesCompared;
		}

		private void ExportCognateSets()
		{
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
			_sortByProp = property;
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
					if (_selectedMeaning == null)
						SelectedMeaning = !_meaningsView.IsEmpty ? _meaningsView.Cast<MeaningViewModel>().First() : null;
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
					_wordsView.SortDescriptions.Add(new SortDescription(_sortByProp, ListSortDirection.Ascending));
				}
			}
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
			if (IsEmpty)
				return;

			_words.Clear();
			ColumnCount = 0;
			IsEmpty = true;
		}

		private void AlignWords()
		{
			if (_selectedMeaning == null || !_projectService.AreAllVarietiesCompared)
				return;

			_busyService.ShowBusyIndicatorUntilFinishDrawing();

			var words = new HashSet<Word>();
			foreach (VarietyPair vp in _projectService.Project.VarietyPairs)
			{
				WordPair wp;
				if (vp.WordPairs.TryGet(_selectedMeaning.DomainMeaning, out wp))
				{
					words.Add(wp.Word1);
					words.Add(wp.Word2);
				}
			}
			if (words.Count == 0)
			{
				_words.Clear();
				return;
			}

			IWordAligner aligner = _projectService.Project.WordAligners[ComponentIdentifiers.PrimaryWordAligner];
			Alignment<Word, ShapeNode> alignment;
			if (words.Count == 1)
			{
				Word word = words.First();
				Annotation<ShapeNode> prefixAnn = word.Prefix;
				var prefix = new AlignmentCell<ShapeNode>(prefixAnn != null ? word.Shape.GetNodes(prefixAnn.Range).Where(NodeFilter) : Enumerable.Empty<ShapeNode>());
				IEnumerable<AlignmentCell<ShapeNode>> columns = word.Shape.GetNodes(word.Stem.Range).Where(NodeFilter).Select(n => new AlignmentCell<ShapeNode>(n));
				Annotation<ShapeNode> suffixAnn = word.Suffix;
				var suffix = new AlignmentCell<ShapeNode>(suffixAnn != null ? word.Shape.GetNodes(suffixAnn.Range).Where(NodeFilter) : Enumerable.Empty<ShapeNode>());
				alignment = new Alignment<Word, ShapeNode>(0, 0, Tuple.Create(word, prefix, columns, suffix));
			}
			else
			{
				IWordAlignerResult result = aligner.Compute(words);
				alignment = result.GetAlignments().First();
			}

			List<Cluster<Word>> cognateSets = _projectService.Project.GenerateCognateSets(_selectedMeaning.DomainMeaning).OrderBy(c => c.Noise).ThenByDescending(c => c.DataObjects.Count).ToList();
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
					int cognateSetIndex = cognateSets.FindIndex(set => set.DataObjects.Contains(word));
					_words.Add(new MultipleWordAlignmentWordViewModel(this, word, prefix, columns, suffix, cognateSetIndex == cognateSets.Count - 1 ? int.MaxValue : cognateSetIndex + 1));
				}
			}
			IsEmpty = false;
		}

		private bool NodeFilter(ShapeNode n)
		{
			return n.Type().IsOneOf(CogFeatureSystem.ConsonantType, CogFeatureSystem.VowelType, CogFeatureSystem.AnchorType);
		}

		private void HandleSwitchView(SwitchViewMessage msg)
		{
			if (msg.ViewModelType == GetType())
			{
				_busyService.ShowBusyIndicatorUntilFinishDrawing();

				var meaning = (Meaning) msg.DomainModels[0];
				SelectedMeaning = _meanings[meaning];
				if (msg.DomainModels.Count > 1)
				{
					var wp = (WordPair) msg.DomainModels[1];
					_selectedWords.ReplaceAll(new[] { _words.First(w => w.DomainWord == wp.Word1), _words.First(w => w.DomainWord == wp.Word2)});
				}
			}
		}

		public bool IsEmpty
		{
			get { return _isEmpty; }
			private set { Set(() => IsEmpty, ref _isEmpty, value); }
		}

		public ObservableList<MultipleWordAlignmentWordViewModel> Words
		{
			get { return _words; }
		}

		public ObservableList<MultipleWordAlignmentWordViewModel> SelectedWords
		{
			get { return _selectedWords; }
		}

		public ICommand ShowInVarietyPairsCommand
		{
			get { return _showInVarietyPairsCommand; }
		}

		public ICommand PerformComparisonCommand
		{
			get { return _performComparisonCommand; }
		}
	}
}
