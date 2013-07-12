using System.Collections.Generic;
using SIL.Collections;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public class RelevantFeatureViewModel : ChangeTrackingViewModelBase
	{
		private readonly SymbolicFeature _feature;
		private bool _vowel;
		private bool _consonant;
		private int _weight;
		private readonly ReadOnlyList<RelevantValueViewModel> _values; 

		public RelevantFeatureViewModel(SymbolicFeature feature, int weight, bool vowel, bool consonant, IReadOnlyDictionary<FeatureSymbol, int> valueMetrics)
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
			_values = new ReadOnlyList<RelevantValueViewModel>(values);
		}

		public string Description
		{
			get { return _feature.Description; }
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			ChildrenAcceptChanges(_values);
		}

		public int Weight
		{
			get { return _weight; }
			set { SetChanged(() => Weight, ref _weight, value); }
		}

		public bool Vowel
		{
			get { return _vowel; }
			set { SetChanged(() => Vowel, ref _vowel, value); }
		}

		public bool Consonant
		{
			get { return _consonant; }
			set { SetChanged(() => Consonant, ref _consonant, value); }
		}

		public ReadOnlyList<RelevantValueViewModel> Values
		{
			get { return _values; }
		}

		internal SymbolicFeature ModelFeature
		{
			get { return _feature; }
		}
	}
}
