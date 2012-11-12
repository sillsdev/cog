using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class VarietyPairViewModel : ViewModelBase
	{
		private readonly VarietyPair _varietyPair;
		private readonly ReadOnlyCollection<SoundCorrespondenceViewModel> _correspondences;
		private SoundCorrespondenceViewModel _currentCorrespondence;
		private readonly ReadOnlyCollection<WordPairViewModel> _wordPairs;
		private readonly bool _areVarietiesInOrder;

		public VarietyPairViewModel(CogProject project, VarietyPair varietyPair, bool areVarietiesInOrder)
		{
			_varietyPair = varietyPair;
			_areVarietiesInOrder = areVarietiesInOrder;
			_correspondences = new ReadOnlyCollection<SoundCorrespondenceViewModel>(_varietyPair.SoundChanges.Conditions.SelectMany(lhs => _varietyPair.SoundChanges[lhs].Samples,
				(lhs, segment) => new SoundCorrespondenceViewModel(lhs, segment, _varietyPair.SoundChanges[lhs].GetProbability(segment))).ToList());
			_wordPairs = new ReadOnlyCollection<WordPairViewModel>(_varietyPair.WordPairs.Select(pair => new WordPairViewModel(project, pair)).ToList());
		}

		public SoundCorrespondenceViewModel CurrentCorrespondence
		{
			get { return _currentCorrespondence; }
			set
			{
				Set("CurrentCorrespondence", ref _currentCorrespondence, value);
				foreach (WordPairViewModel word in _wordPairs)
				{
					foreach (AlignedNodeViewModel node in word.AlignedNodes)
					{
						if (_currentCorrespondence == null)
						{
							node.IsSelected = false;
						}
						else
						{
							Ngram nseg1 = _varietyPair.Variety1.Segments[node.Annotation1];
							Ngram nseg2 = _varietyPair.Variety2.Segments[node.Annotation2];
							SoundChangeLhs lhs = _currentCorrespondence.ModelSoundChangeLhs;
							node.IsSelected = nseg1.Equals(lhs.Target) && nseg2.Equals(_currentCorrespondence.ModelCorrespondence)
								&& (lhs.LeftEnvironment == null || lhs.LeftEnvironment.FeatureStruct.IsUnifiable(node.Annotation1.GetPrev(a => a.Type() != CogFeatureSystem.NullType).FeatureStruct))
								&& (lhs.RightEnvironment == null || lhs.RightEnvironment.FeatureStruct.IsUnifiable(node.Annotation1.GetNext(a => a.Type() != CogFeatureSystem.NullType).FeatureStruct));
						}
					}
				}
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

		public ReadOnlyCollection<SoundCorrespondenceViewModel> Correspondences
		{
			get { return _correspondences; }
		}

		public ReadOnlyCollection<WordPairViewModel> WordPairs
		{
			get { return _wordPairs; }
		}

		public VarietyPair ModelVarietyPair
		{
			get { return _varietyPair; }
		}
	}
}
