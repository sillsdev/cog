using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class VarietyPairViewModel : ViewModelBase
	{
		private readonly VarietyPair _varietyPair;
		private readonly ReadOnlyCollection<SoundChangeViewModel> _soundChanges;
		private SoundChangeViewModel _currentSoundChange;
		private readonly bool _areVarietiesInOrder;
		private readonly CogProject _project;
		private readonly WordPairsViewModel _cognates;
		private readonly WordPairsViewModel _noncognates;

		public VarietyPairViewModel(CogProject project, VarietyPair varietyPair, bool areVarietiesInOrder)
		{
			_project = project;
			_varietyPair = varietyPair;
			_areVarietiesInOrder = areVarietiesInOrder;

			_cognates = new WordPairsViewModel(project, _varietyPair.WordPairs.Where(wp => wp.AreCognatePredicted), areVarietiesInOrder);
			_noncognates = new WordPairsViewModel(project, _varietyPair.WordPairs.Where(wp => !wp.AreCognatePredicted), areVarietiesInOrder);

			_soundChanges = new ReadOnlyCollection<SoundChangeViewModel>(_varietyPair.SoundChangeProbabilityDistribution.Conditions.SelectMany(lhs => _varietyPair.SoundChangeProbabilityDistribution[lhs].Samples,
				(lhs, segment) => new SoundChangeViewModel(lhs, segment, _varietyPair.SoundChangeProbabilityDistribution[lhs][segment], _varietyPair.SoundChangeFrequencyDistribution[lhs][segment])).ToList());
		}

		public SoundChangeViewModel CurrentSoundChange
		{
			get { return _currentSoundChange; }
			set
			{
				Set(() => CurrentSoundChange, ref _currentSoundChange, value);
				UpdateSelectedChangeWordPairs(_cognates);
				UpdateSelectedChangeWordPairs(_noncognates);
			}
		}

		private void UpdateSelectedChangeWordPairs(WordPairsViewModel wordPairs)
		{
			wordPairs.SelectedChangeWordPairs.Clear();
			foreach (WordPairViewModel wordPair in wordPairs.WordPairs)
			{
				bool selected = false;
				foreach (AlignedNodeViewModel node in wordPair.AlignedNodes)
				{
					if (_currentSoundChange == null)
					{
						node.IsSelected = false;
					}
					else
					{
						SoundContext lhs = GetLhs(node);
						Ngram corr = _varietyPair.Variety2.Segments[node.Annotation2];
						node.IsSelected = lhs.Equals(_currentSoundChange.ModelSoundChangeLhs) && corr.Equals(_currentSoundChange.ModelCorrespondence);
						if (node.IsSelected)
							selected = true;
					}
				}

				if (selected)
					wordPairs.SelectedChangeWordPairs.Add(wordPair);
			}
		}

		private SoundContext GetLhs(AlignedNodeViewModel node)
		{
			IAligner aligner = _project.Aligners["primary"];
			return node.Annotation1.Sound(_varietyPair.Variety1, aligner.ContextualSoundClasses);
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

		public ReadOnlyCollection<SoundChangeViewModel> SoundChanges
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

		public VarietyPair ModelVarietyPair
		{
			get { return _varietyPair; }
		}
	}
}
