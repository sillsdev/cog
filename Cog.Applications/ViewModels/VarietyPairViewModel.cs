using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class VarietyPairViewModel : ViewModelBase
	{
		public delegate VarietyPairViewModel Factory(VarietyPair varietyPair, bool areVarietiesInOrder);

		private readonly SegmentPool _segmentPool;
		private readonly IProjectService _projectService;
		private readonly VarietyPair _varietyPair;
		private readonly ReadOnlyList<SoundChangeViewModel> _soundChanges;
		private SoundChangeViewModel _selectedSoundChange;
		private readonly bool _areVarietiesInOrder;
		private readonly WordPairsViewModel _cognates;
		private readonly WordPairsViewModel _noncognates;

		public VarietyPairViewModel(SegmentPool segmentPool, IProjectService projectService, WordPairsViewModel.Factory wordPairsFactory, VarietyPair varietyPair, bool areVarietiesInOrder)
		{
			_segmentPool = segmentPool;
			_projectService = projectService;
			_varietyPair = varietyPair;
			_areVarietiesInOrder = areVarietiesInOrder;

			IWordAligner aligner = projectService.Project.WordAligners["primary"];
			_cognates = wordPairsFactory();
			foreach (WordPair wp in _varietyPair.WordPairs.Where(wp => wp.AreCognatePredicted))
				_cognates.WordPairs.Add(new WordPairViewModel(aligner, wp, _areVarietiesInOrder));
			_noncognates = wordPairsFactory();
			foreach (WordPair wp in _varietyPair.WordPairs.Where(wp => !wp.AreCognatePredicted))
				_noncognates.WordPairs.Add(new WordPairViewModel(aligner, wp, _areVarietiesInOrder));

			_soundChanges = new ReadOnlyList<SoundChangeViewModel>(_varietyPair.SoundChangeProbabilityDistribution.Conditions.SelectMany(lhs => _varietyPair.SoundChangeProbabilityDistribution[lhs].Samples,
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
			IWordAligner aligner = _projectService.Project.WordAligners["primary"];
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
						SoundContext lhs = wordPair.DomainAlignment.ToSoundContext(_segmentPool, 0, node.Column, wordPair.DomainWordPair.Word1, aligner.ContextualSoundClasses);
						Ngram corr = wordPair.DomainAlignment[1, node.Column].ToNgram(_segmentPool);
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
		}

		public WordPairsViewModel Cognates
		{
			get { return _cognates; }
		}

		public WordPairsViewModel Noncognates
		{
			get { return _noncognates; }
		}

		internal VarietyPair DomainVarietyPair
		{
			get { return _varietyPair; }
		}
	}
}
