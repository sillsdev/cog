using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine.NgramModeling;

namespace SIL.Cog.Application.ViewModels
{
	public class VarietyPairViewModel : WrapperViewModel
	{
		public delegate VarietyPairViewModel Factory(VarietyPair varietyPair, bool areVarietiesInOrder);

		private readonly SegmentPool _segmentPool;
		private readonly IProjectService _projectService;
		private readonly VarietyPair _varietyPair;
		private ReadOnlyList<SoundChangeViewModel> _soundChanges;
		private SoundChangeViewModel _selectedSoundChange;
		private readonly bool _areVarietiesInOrder;
		private WordPairsViewModel _cognates;
		private WordPairsViewModel _noncognates;
		private readonly WordPairsViewModel.Factory _wordPairsFactory;
		private readonly WordPairViewModel.Factory _wordPairFactory;

		public VarietyPairViewModel(SegmentPool segmentPool, IProjectService projectService, WordPairsViewModel.Factory wordPairsFactory,
			WordPairViewModel.Factory wordPairFactory, VarietyPair varietyPair, bool areVarietiesInOrder)
			: base(varietyPair)
		{
			_segmentPool = segmentPool;
			_projectService = projectService;
			_varietyPair = varietyPair;
			_areVarietiesInOrder = areVarietiesInOrder;
			_wordPairsFactory = wordPairsFactory;
			_wordPairFactory = wordPairFactory;

			Messenger.Default.Register<ComparisonPerformedMessage>(this, HandleComparisonPerformed);

			UpdateVarietyPair();
		}

		private void HandleComparisonPerformed(ComparisonPerformedMessage msg)
		{
			if (msg.VarietyPair == _varietyPair)
			{
				var selectedMeanings = new HashSet<Meaning>();
				selectedMeanings.UnionWith(_cognates.SelectedWordPairs.Select(wp => wp.DomainWordPair.Meaning));
				selectedMeanings.UnionWith(_noncognates.SelectedWordPairs.Select(wp => wp.DomainWordPair.Meaning));

				SortDescription[] cognateSortDescriptions = _cognates.WordPairsView.SortDescriptions.ToArray();
				SortDescription[] noncognateSortDescriptions = _noncognates.WordPairsView.SortDescriptions.ToArray();

				UpdateVarietyPair();

				_cognates.WordPairsView.SortDescriptions.AddRange(cognateSortDescriptions);
				_noncognates.WordPairsView.SortDescriptions.AddRange(noncognateSortDescriptions);

				_cognates.SelectedWordPairs.AddRange(_cognates.WordPairs.Where(wp => selectedMeanings.Contains(wp.DomainWordPair.Meaning)));
				_noncognates.SelectedWordPairs.AddRange(_noncognates.WordPairs.Where(wp => selectedMeanings.Contains(wp.DomainWordPair.Meaning)));
			}
		}

		private void UpdateVarietyPair()
		{
			WordPairsViewModel cognates = _wordPairsFactory();
			foreach (WordPair wp in _varietyPair.WordPairs.Where(wp => wp.Cognacy))
				cognates.WordPairs.Add(_wordPairFactory(wp, _areVarietiesInOrder));
			Cognates = cognates;

			WordPairsViewModel noncognates = _wordPairsFactory();
			foreach (WordPair wp in _varietyPair.WordPairs.Where(wp => !wp.Cognacy))
				noncognates.WordPairs.Add(_wordPairFactory(wp, _areVarietiesInOrder));
			Noncognates = noncognates;

			SoundChanges = new ReadOnlyList<SoundChangeViewModel>(_varietyPair.SoundChangeProbabilityDistribution.Conditions.SelectMany(lhs => _varietyPair.SoundChangeProbabilityDistribution[lhs].Samples,
				(lhs, segment) => new SoundChangeViewModel(lhs, segment, _varietyPair.SoundChangeProbabilityDistribution[lhs][segment], _varietyPair.SoundChangeFrequencyDistribution[lhs][segment])).ToList());
		}

		public SoundChangeViewModel SelectedSoundChange
		{
			get { return _selectedSoundChange; }
			set
			{
				Set(() => SelectedSoundChange, ref _selectedSoundChange, value);
				UpdateSelectedChangeWordPairs(_cognates);
				UpdateSelectedChangeWordPairs(_noncognates);
			}
		}

		private void UpdateSelectedChangeWordPairs(WordPairsViewModel wordPairs)
		{
			IWordAligner aligner = _projectService.Project.WordAligners[ComponentIdentifiers.PrimaryWordAligner];
			wordPairs.SelectedCorrespondenceWordPairs.Clear();
			foreach (WordPairViewModel wordPair in wordPairs.WordPairs)
			{
				bool selected = false;
				foreach (AlignedNodeViewModel node in wordPair.AlignedNodes)
				{
					if (_selectedSoundChange == null)
					{
						node.IsSelected = false;
					}
					else
					{
						SoundContext lhs = wordPair.DomainAlignment.ToSoundContext(_segmentPool, 0, node.Column, aligner.ContextualSoundClasses);
						Ngram<Segment> corr = wordPair.DomainAlignment[1, node.Column].ToNgram(_segmentPool);
						node.IsSelected = lhs.Equals(_selectedSoundChange.DomainSoundChangeLhs) && corr.Equals(_selectedSoundChange.DomainCorrespondence);
						if (node.IsSelected)
							selected = true;
					}
				}

				if (selected)
					wordPairs.SelectedCorrespondenceWordPairs.Add(wordPair);
			}
		}

		public bool AreVarietiesInOrder
		{
			get { return _areVarietiesInOrder; }
		}

		public double LexicalSimilarityScore
		{
			get { return _varietyPair.LexicalSimilarityScore; }
		}

		public double PhoneticSimilarityScore
		{
			get { return _varietyPair.PhoneticSimilarityScore; }
		}

		public ReadOnlyList<SoundChangeViewModel> SoundChanges
		{
			get { return _soundChanges; }
			private set { Set(() => SoundChanges, ref _soundChanges, value); }
		}

		public WordPairsViewModel Cognates
		{
			get { return _cognates; }
			private set { Set(() => Cognates, ref _cognates, value); }
		}

		public WordPairsViewModel Noncognates
		{
			get { return _noncognates; }
			set { Set(() => Noncognates, ref _noncognates, value); }
		}

		internal VarietyPair DomainVarietyPair
		{
			get { return _varietyPair; }
		}
	}
}
