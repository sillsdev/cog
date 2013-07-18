using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class VarietyPairViewModel : ViewModelBase
	{
		private readonly VarietyPair _varietyPair;
		private readonly ReadOnlyList<SoundChangeViewModel> _soundChanges;
		private readonly Lazy<ListCollectionView> _soundChangesView;
		private SoundChangeViewModel _currentSoundChange;
		private readonly bool _areVarietiesInOrder;
		private readonly IWordAligner _aligner;
		private readonly WordPairsViewModel _cognates;
		private readonly WordPairsViewModel _noncognates;

		public VarietyPairViewModel(IWordAligner aligner, VarietyPair varietyPair, bool areVarietiesInOrder)
		{
			_aligner = aligner;
			_varietyPair = varietyPair;
			_areVarietiesInOrder = areVarietiesInOrder;

			_cognates = new WordPairsViewModel(_aligner, _varietyPair.WordPairs.Where(wp => wp.AreCognatePredicted), areVarietiesInOrder);
			_noncognates = new WordPairsViewModel(_aligner, _varietyPair.WordPairs.Where(wp => !wp.AreCognatePredicted), areVarietiesInOrder);

			_soundChanges = new ReadOnlyList<SoundChangeViewModel>(_varietyPair.SoundChangeProbabilityDistribution.Conditions.SelectMany(lhs => _varietyPair.SoundChangeProbabilityDistribution[lhs].Samples,
				(lhs, segment) => new SoundChangeViewModel(lhs, segment, _varietyPair.SoundChangeProbabilityDistribution[lhs][segment], _varietyPair.SoundChangeFrequencyDistribution[lhs][segment])).ToList());
			_soundChangesView = new Lazy<ListCollectionView>(() =>
				{
					var view = new ListCollectionView(_soundChanges);
					Debug.Assert(view.GroupDescriptions != null);
					view.GroupDescriptions.Add(new PropertyGroupDescription("Lhs"));
					return view;
				}, false);
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
						SoundContext lhs = wordPair.DomainAlignment.ToSoundContext(wordPair.DomainWordPair.VarietyPair.Variety1.SegmentPool, 0, node.Column, wordPair.DomainWordPair.Word1, _aligner.ContextualSoundClasses);
						Ngram corr = wordPair.DomainAlignment[1, node.Column].ToNgram(wordPair.DomainWordPair.VarietyPair.Variety2.SegmentPool);
						node.IsSelected = lhs.Equals(_currentSoundChange.DomainSoundChangeLhs) && corr.Equals(_currentSoundChange.DomainCorrespondence);
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
			get { return _soundChangesView.Value; }
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
