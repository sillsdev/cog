using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Cog.Domain.SequenceAlignment;
using SIL.Collections;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Applications.ViewModels
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
		private readonly CogProject _project;
		private AlineMode _mode;
		private bool _expansionCompressionEnabled;
		private readonly ReadOnlyList<RelevantFeatureViewModel> _features;
		private readonly SoundClassesViewModel _soundClasses;

		public AlineViewModel(IDialogService dialogService, CogProject project, Aline aligner)
			: base("Alignment")
		{
			_project = project;
			var features = new List<RelevantFeatureViewModel>();
			foreach (KeyValuePair<SymbolicFeature, int> kvp in aligner.FeatureWeights)
			{
				var vm = new RelevantFeatureViewModel(kvp.Key, kvp.Value, aligner.RelevantVowelFeatures.Contains(kvp.Key), aligner.RelevantConsonantFeatures.Contains(kvp.Key), aligner.ValueMetrics);
				vm.PropertyChanged += ChildPropertyChanged;
				features.Add(vm);
			}
			_features = new ReadOnlyList<RelevantFeatureViewModel>(features);
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
			_soundClasses = new SoundClassesViewModel(dialogService, _project.FeatureSystem, _project.Segmenter, aligner.ContextualSoundClasses.Select(sc => new SoundClassViewModel(sc)), false);
			_soundClasses.PropertyChanged += ChildPropertyChanged;
		}

		public AlineMode Mode
		{
			get { return _mode; }
			set { SetChanged(() => Mode, ref _mode, value); }
		}

		public bool ExpansionCompressionEnabled
		{
			get { return _expansionCompressionEnabled; }
			set { SetChanged(() => ExpansionCompressionEnabled, ref _expansionCompressionEnabled, value); }
		}

		public ReadOnlyList<RelevantFeatureViewModel> Features
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
			_soundClasses.AcceptChanges();
			ChildrenAcceptChanges(_features);
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
					relevantVowelFeatures.Add(feature.DomainFeature);
				if (feature.Consonant)
					relevantConsFeatures.Add(feature.DomainFeature);
				featureWeights[feature.DomainFeature] = feature.Weight;
				foreach (RelevantValueViewModel value in feature.Values)
					valueMetrics[value.DomainSymbol] = value.Metric;
			}

			var aligner = new Aline(relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics,
				new WordPairAlignerSettings {ExpansionCompressionEnabled = _expansionCompressionEnabled, Mode = mode, ContextualSoundClasses = _soundClasses.SoundClasses.Select(nc => nc.DomainSoundClass)});
			_project.WordAligners["primary"] = aligner;
			return aligner;
		}
	}
}
