using System.Collections.Generic;
using System.Collections.ObjectModel;
using SIL.Collections;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public class RelevantFeatureViewModel : CogViewModelBase
	{
		private readonly SymbolicFeature _feature;
		private bool _vowel;
		private bool _consonant;
		private int _weight;
		private readonly ReadOnlyCollection<RelevantValueViewModel> _values; 

		public RelevantFeatureViewModel(SymbolicFeature feature, int weight, bool vowel, bool consonant, IReadOnlyDictionary<FeatureSymbol, int> valueMetrics)
			: base(feature.Description)
		{
			_feature = feature;
			_weight = weight;
			_vowel = vowel;
			_consonant = consonant;

			var values = new List<RelevantValueViewModel>();
			foreach (FeatureSymbol symbol in feature.PossibleSymbols)
			{
				var vm = new RelevantValueViewModel(symbol, valueMetrics[symbol]);
				vm.PropertyChanged += ChildPropertyChanged;
				values.Add(vm);
			}
			_values = new ReadOnlyCollection<RelevantValueViewModel>(values);
		}

		public SymbolicFeature ModelFeature
		{
			get { return _feature; }
		}

		public int Weight
		{
			get { return _weight; }
			set
			{
				if (Set(() => Weight, ref _weight, value))
					IsChanged = true;
			}
		}

		public bool Vowel
		{
			get { return _vowel; }
			set
			{
				if (Set(() => Vowel, ref _vowel, value))
					IsChanged = true;
			}
		}

		public bool Consonant
		{
			get { return _consonant; }
			set
			{
				if (Set(() => Consonant, ref _consonant, value))
					IsChanged = true;
			}
		}

		public ReadOnlyCollection<RelevantValueViewModel> Values
		{
			get { return _values; }
		}
	}
}
