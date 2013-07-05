using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class VarietyPairViewModel : ViewModelBase
	{
		private readonly VarietyPair _varietyPair;
		private readonly ReadOnlyList<SoundChangeViewModel> _soundChanges;
		private ListCollectionView _soundChangesView;
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

			_soundChanges = new ReadOnlyList<SoundChangeViewModel>(_varietyPair.SoundChangeProbabilityDistribution.Conditions.SelectMany(lhs => _varietyPair.SoundChangeProbabilityDistribution[lhs].Samples,
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
			IWordAligner aligner = _project.WordAligners["primary"];
			wordPairs.SelectedCorrespondenceWordPairs.Clear();
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
						SoundContext lhs = wordPair.ModelAlignment.ToSoundContext(wordPair.ModelWordPair.VarietyPair.Variety1.SegmentPool, 0, node.Column, wordPair.ModelWordPair.Word1, aligner.ContextualSoundClasses);
						Ngram corr = wordPair.ModelAlignment[1, node.Column].ToNgram(wordPair.ModelWordPair.VarietyPair.Variety2.SegmentPool);
						node.IsSelected = lhs.Equals(_currentSoundChange.ModelSoundChangeLhs) && corr.Equals(_currentSoundChange.ModelCorrespondence);
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

		public ICollectionView SoundChangesView
		{
			get
			{
				if (_soundChangesView == null)
				{
					_soundChangesView = new ListCollectionView(_soundChanges);
					Debug.Assert(_soundChangesView.GroupDescriptions != null);
					_soundChangesView.GroupDescriptions.Add(new PropertyGroupDescription("Lhs"));
				}
				return _soundChangesView;
			}
		}

		public WordPairsViewModel Cognates
		{
			get { return _cognates; }
		}

		public WordPairsViewModel Noncognates
		{
			get { return _noncognates; }
		}

		internal VarietyPair ModelVarietyPair
		{
			get { return _varietyPair; }
		}
	}
}
