using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using SIL.Cog.Components;
using SIL.Cog.SequenceAlignment;
using SIL.Cog.Services;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public enum AlineMode
	{
		[Description("Partial (local)")]
		Local,
		[Description("Full (global)")]
		Global,
		[Description("Beginning (half-local)")]
		HalfLocal,
		[Description("Hybrid (semi-global)")]
		SemiGlobal
	}

	public class AlineViewModel : ComponentSettingsViewModelBase
	{
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private AlineMode _mode;
		private bool _expansionCompressionEnabled;
		private readonly ReadOnlyCollection<RelevantFeatureViewModel> _features;
		private readonly SoundClassesViewModel _soundClasses;

		public AlineViewModel(SpanFactory<ShapeNode> spanFactory, IDialogService dialogService, CogProject project, Aline aligner)
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
				case AlignmentMode.Local:
					_mode = AlineMode.Local;
					break;
				case AlignmentMode.Global:
					_mode = AlineMode.Global;
					break;
				case AlignmentMode.SemiGlobal:
					_mode = AlineMode.SemiGlobal;
					break;
				case AlignmentMode.HalfLocal:
					_mode = AlineMode.HalfLocal;
					break;
			}
			_expansionCompressionEnabled = aligner.Settings.ExpansionCompressionEnabled;
			_soundClasses = new SoundClassesViewModel(dialogService, project, aligner.ContextualSoundClasses);
			_soundClasses.SoundClasses.CollectionChanged += NaturalClassesChanged;
		}

		private void NaturalClassesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		public AlineMode Mode
		{
			get { return _mode; }
			set
			{
				if (Set(() => Mode, ref _mode, value))
					IsChanged = true;
			}
		}

		public bool ExpansionCompressionEnabled
		{
			get { return _expansionCompressionEnabled; }
			set
			{
				if (Set(() => ExpansionCompressionEnabled, ref _expansionCompressionEnabled, value))
					IsChanged = true;
			}
		}

		public ReadOnlyCollection<RelevantFeatureViewModel> Features
		{
			get { return _features; }
		}

		public SoundClassesViewModel SoundClasses
		{
			get { return _soundClasses; }
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			foreach (RelevantFeatureViewModel relevantFeature in _features)
				relevantFeature.AcceptChanges();
		}

		public override object UpdateComponent()
		{
			var mode = AlignmentMode.Local;
			switch (_mode)
			{
				case AlineMode.Local:
					mode = AlignmentMode.Local;
					break;
				case AlineMode.Global:
					mode = AlignmentMode.Global;
					break;
				case AlineMode.SemiGlobal:
					mode = AlignmentMode.SemiGlobal;
					break;
				case AlineMode.HalfLocal:
					mode = AlignmentMode.HalfLocal;
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

			var aligner = new Aline(_spanFactory, relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics,
				new WordPairAlignerSettings {ExpansionCompressionEnabled = _expansionCompressionEnabled, Mode = mode, ContextualSoundClasses = _soundClasses.SoundClasses.Select(nc => nc.ModelSoundClass)});
			Project.Aligners["primary"] = aligner;
			return aligner;
		}
	}
}
