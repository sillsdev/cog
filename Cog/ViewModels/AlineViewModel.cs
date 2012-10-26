using System.Collections.Generic;
using System.Collections.ObjectModel;
using SIL.Cog.Aligners;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public class AlineViewModel : ComponentSettingsViewModelBase
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 
		private string _mode;
		private bool _disableExpansionCompression;
		private readonly ReadOnlyCollection<RelevantFeatureViewModel> _features;

		public AlineViewModel(SpanFactory<ShapeNode> spanFactory, CogProject project, Aline aligner)
			: base("Alignment", project)
		{
			_spanFactory = spanFactory;
			var features = new List<RelevantFeatureViewModel>();
			foreach (KeyValuePair<SymbolicFeature, int> kvp in aligner.FeatureWeights)
			{
				var vm = new RelevantFeatureViewModel(kvp.Key, kvp.Value, aligner.RelevantVowelFeatures.Contains(kvp.Key), aligner.RelevantConsonantFeatures.Contains(kvp.Key), aligner.ValueMetrics);
				vm.PropertyChanged += ChildPropertyChanged;
				features.Add(vm);
			}
			_features = new ReadOnlyCollection<RelevantFeatureViewModel>(features);
			switch (aligner.Settings.Mode)
			{
				case AlignerMode.Local:
					_mode = "Local";
					break;
				case AlignerMode.Global:
					_mode = "Global";
					break;
				case AlignerMode.SemiGlobal:
					_mode = "Semi-global";
					break;
				case AlignerMode.HalfLocal:
					_mode = "Half-local";
					break;
			}
			_disableExpansionCompression = aligner.Settings.DisableExpansionCompression;
		}

		public string Mode
		{
			get { return _mode; }
			set
			{
				Set(() => Mode, ref _mode, value);
				IsChanged = true;
			}
		}

		public bool DisableExpansionCompression
		{
			get { return _disableExpansionCompression; }
			set
			{
				Set(() => DisableExpansionCompression, ref _disableExpansionCompression, value);
				IsChanged = true;
			}
		}

		public ReadOnlyCollection<RelevantFeatureViewModel> Features
		{
			get { return _features; }
		}

		public override void UpdateComponent()
		{
			var mode = AlignerMode.Local;
			switch (_mode)
			{
				case "Local":
					mode = AlignerMode.Local;
					break;
				case "Global":
					mode = AlignerMode.Global;
					break;
				case "Semi-global":
					mode = AlignerMode.SemiGlobal;
					break;
				case "Half-local":
					mode = AlignerMode.HalfLocal;
					break;
			}

			var relevantVowelFeatures = new List<SymbolicFeature>();
			var relevantConsFeatures = new List<SymbolicFeature>();
			var featureWeights = new Dictionary<SymbolicFeature, int>();
			var valueMetrics = new Dictionary<FeatureSymbol, int>();
			foreach (RelevantFeatureViewModel feature in _features)
			{
				if (feature.Vowel)
					relevantVowelFeatures.Add(feature.ModelFeature);
				if (feature.Consonant)
					relevantConsFeatures.Add(feature.ModelFeature);
				featureWeights[feature.ModelFeature] = feature.Weight;
				foreach (RelevantValueViewModel value in feature.Values)
					valueMetrics[value.ModelSymbol] = value.Metric;
			}

			Project.Aligners["primary"] = new Aline(_spanFactory, relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics,
				new AlignerSettings {DisableExpansionCompression = _disableExpansionCompression, Mode = mode});
		}
	}
}
