using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class VarietyPairViewModel : ViewModelBase
	{
		private readonly VarietyPair _varietyPair;
		private readonly ReadOnlyCollection<SoundCorrespondenceViewModel> _correspondences;
		private SoundCorrespondenceViewModel _currentCorrespondence;
		private readonly ReadOnlyCollection<WordPairViewModel> _wordPairs;  

		public VarietyPairViewModel(CogProject project, VarietyPair varietyPair)
		{
			_varietyPair = varietyPair;
			_correspondences = new ReadOnlyCollection<SoundCorrespondenceViewModel>(_varietyPair.SoundChanges.SelectMany(change => change.ObservedCorrespondences,
				(change, segment) => new SoundCorrespondenceViewModel(change, segment)).ToList());
			_wordPairs = new ReadOnlyCollection<WordPairViewModel>(_varietyPair.WordPairs.Select(pair => new WordPairViewModel(project, pair)).ToList());
		}

		public SoundCorrespondenceViewModel CurrentCorrespondence
		{
			get { return _currentCorrespondence; }
			set { Set("CurrentCorrespondence", ref _currentCorrespondence, value); }
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
