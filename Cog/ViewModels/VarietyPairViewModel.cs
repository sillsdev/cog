using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class VarietyPairViewModel : ViewModelBase
	{
		private readonly VarietyPair _varietyPair;
		private readonly System.Collections.ObjectModel.ReadOnlyCollection<SoundCorrespondenceViewModel> _correspondences;
		private SoundCorrespondenceViewModel _currentCorrespondence;
		private readonly System.Collections.ObjectModel.ReadOnlyCollection<WordPairViewModel> _wordPairs;
		private readonly bool _areVarietiesInOrder;

		public VarietyPairViewModel(CogProject project, VarietyPair varietyPair, bool areVarietiesInOrder)
		{
			_varietyPair = varietyPair;
			_areVarietiesInOrder = areVarietiesInOrder;
			_correspondences = new System.Collections.ObjectModel.ReadOnlyCollection<SoundCorrespondenceViewModel>(_varietyPair.SoundChanges.SelectMany(change => change.ObservedCorrespondences,
				(change, segment) => new SoundCorrespondenceViewModel(change, segment)).ToList());
			_wordPairs = new System.Collections.ObjectModel.ReadOnlyCollection<WordPairViewModel>(_varietyPair.WordPairs.Select(pair => new WordPairViewModel(project, pair)).ToList());
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
							var nseg1 = new NSegment(_varietyPair.Variety1.Segments[node.Annotation1.StrRep()]);
							var nseg2 = new NSegment(_varietyPair.Variety2.Segments[node.Annotation2.StrRep()]);
							SoundChangeLhs lhs = _currentCorrespondence.ModelSoundChange.Lhs;
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

		public System.Collections.ObjectModel.ReadOnlyCollection<SoundCorrespondenceViewModel> Correspondences
		{
			get { return _correspondences; }
		}

		public System.Collections.ObjectModel.ReadOnlyCollection<WordPairViewModel> WordPairs
		{
			get { return _wordPairs; }
		}

		public VarietyPair ModelVarietyPair
		{
			get { return _varietyPair; }
		}
	}
}
