using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class VarietyPairViewModel : ViewModelBase
	{
		private readonly VarietyPair _varietyPair;
		private readonly ReadOnlyCollection<SoundCorrespondenceViewModel> _correspondences;
		private SoundCorrespondenceViewModel _currentCorrespondence;
		private readonly ReadOnlyCollection<WordPairViewModel> _wordPairs;
		private readonly bool _areVarietiesInOrder;
		private readonly ObservableCollection<WordPairViewModel> _selectedWordPairs;
		private readonly CogProject _project;

		public VarietyPairViewModel(CogProject project, VarietyPair varietyPair, bool areVarietiesInOrder)
		{
			_project = project;
			_varietyPair = varietyPair;
			_areVarietiesInOrder = areVarietiesInOrder;

			_wordPairs = new ReadOnlyCollection<WordPairViewModel>(_varietyPair.WordPairs.Select(pair => new WordPairViewModel(project, pair)).ToList());

			_correspondences = new ReadOnlyCollection<SoundCorrespondenceViewModel>(_varietyPair.SoundChangeProbabilityDistribution.Conditions.SelectMany(lhs => _varietyPair.SoundChangeProbabilityDistribution[lhs].Samples,
				(lhs, segment) => new SoundCorrespondenceViewModel(lhs, segment, _varietyPair.SoundChangeProbabilityDistribution[lhs][segment], _varietyPair.SoundChangeFrequencyDistribution[lhs][segment])).ToList());

			_selectedWordPairs = new ObservableCollection<WordPairViewModel>();
		}

		public SoundCorrespondenceViewModel CurrentCorrespondence
		{
			get { return _currentCorrespondence; }
			set
			{
				Set("CurrentCorrespondence", ref _currentCorrespondence, value);
				_selectedWordPairs.Clear();
				foreach (WordPairViewModel wordPair in _wordPairs.Where(wp => wp.AreCognate))
				{
					bool selected = false;
					foreach (AlignedNodeViewModel node in wordPair.AlignedNodes)
					{
						if (_currentCorrespondence == null)
						{
							node.IsSelected = false;
						}
						else
						{
							SoundChangeLhs lhs = GetLhs(node);
							Ngram corr = _varietyPair.Variety2.Segments[node.Annotation2];
							node.IsSelected = lhs.Equals(_currentCorrespondence.ModelSoundChangeLhs) && corr.Equals(_currentCorrespondence.ModelCorrespondence);
							if (node.IsSelected)
								selected = true;
						}
					}

					if (selected)
						_selectedWordPairs.Add(wordPair);
				}
			}
		}

		private SoundChangeLhs GetLhs(AlignedNodeViewModel node)
		{
			IAligner aligner = _project.Aligners["primary"];
			Ngram nseg1 = _varietyPair.Variety1.Segments[node.Annotation1];
			ShapeNode leftCtxt = node.Annotation1.Span.Start.GetPrev(n => n.Type().IsOneOf(CogFeatureSystem.AnchorType, CogFeatureSystem.ConsonantType, CogFeatureSystem.VowelType));
			SoundClass leftEnv = aligner.ContextualSoundClasses.FirstOrDefault(constraint => constraint.Matches(leftCtxt.Annotation));
			ShapeNode rightCtxt = node.Annotation1.Span.End.GetNext(n => n.Type().IsOneOf(CogFeatureSystem.AnchorType, CogFeatureSystem.ConsonantType, CogFeatureSystem.VowelType));
			SoundClass rightEnv = aligner.ContextualSoundClasses.FirstOrDefault(constraint => constraint.Matches(rightCtxt.Annotation));
			return new SoundChangeLhs(leftEnv, nseg1, rightEnv);
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

		public ObservableCollection<WordPairViewModel> SelectedWordPairs
		{
			get { return _selectedWordPairs; }
		}

		public VarietyPair ModelVarietyPair
		{
			get { return _varietyPair; }
		}
	}
}
