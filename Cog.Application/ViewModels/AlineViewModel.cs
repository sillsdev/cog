using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Collections;
using SIL.Machine.FeatureModel;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Application.ViewModels
{
	public enum AlineMode
	{
		[Description("Full (global)")]
		Global,
		[Description("Partial (local)")]
		Local,
		[Description("Beginning (half-local)")]
		HalfLocal,
		[Description("Hybrid (semi-global)")]
		SemiGlobal
	}

	public class AlineViewModel : ComponentSettingsViewModelBase
	{
		private readonly SegmentPool _segmentPool;
		private readonly IProjectService _projectService;
		private AlineMode _mode;
		private bool _expansionCompressionEnabled;
		private ReadOnlyList<RelevantFeatureViewModel> _features;
		private readonly SoundClassesViewModel _soundClasses;
		private bool _soundChangeScoringEnabled;
		private bool _syllablePositionCostEnabled;

		public AlineViewModel(SegmentPool segmentPool, IProjectService projectService, SoundClassesViewModel soundClasses)
			: base("Alignment", "Comparison-Settings#alignment")
		{
			_segmentPool = segmentPool;
			_projectService = projectService;
			_soundClasses = soundClasses;
			_soundClasses.PropertyChanged += ChildPropertyChanged;
		}

		public override void Setup()
		{
			var aligner = (Aline) _projectService.Project.WordAligners[ComponentIdentifiers.PrimaryWordAligner];
			var features = new List<RelevantFeatureViewModel>();
			foreach (KeyValuePair<SymbolicFeature, int> kvp in aligner.FeatureWeights)
			{
				var vm = new RelevantFeatureViewModel(kvp.Key, kvp.Value, aligner.RelevantVowelFeatures.Contains(kvp.Key), aligner.RelevantConsonantFeatures.Contains(kvp.Key), aligner.ValueMetrics);
				vm.PropertyChanged += ChildPropertyChanged;
				features.Add(vm);
			}
			Set(() => Features, ref _features, new ReadOnlyList<RelevantFeatureViewModel>(features));
			switch (aligner.Settings.Mode)
			{
				case AlignmentMode.Local:
					Set(() => Mode, ref _mode, AlineMode.Local);
					break;
				case AlignmentMode.Global:
					Set(() => Mode, ref _mode, AlineMode.Global);
					break;
				case AlignmentMode.SemiGlobal:
					Set(() => Mode, ref _mode, AlineMode.SemiGlobal);
					break;
				case AlignmentMode.HalfLocal:
					Set(() => Mode, ref _mode, AlineMode.HalfLocal);
					break;
			}
			Set(() => ExpansionCompressionEnabled, ref _expansionCompressionEnabled, aligner.Settings.ExpansionCompressionEnabled);
			Set(() => SoundChangeScoringEnabled, ref _soundChangeScoringEnabled, aligner.Settings.SoundChangeScoringEnabled);
			Set(() => SyllablePositionCostEnabled, ref _syllablePositionCostEnabled, aligner.Settings.SyllablePositionCostEnabled);

			_soundClasses.SelectedSoundClass = null;
			_soundClasses.SoundClasses.Clear();
			foreach (SoundClass soundClass in aligner.ContextualSoundClasses)
				_soundClasses.SoundClasses.Add(new SoundClassViewModel(soundClass));
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

		public bool SoundChangeScoringEnabled
		{
			get { return _soundChangeScoringEnabled; }
			set { SetChanged(() => SoundChangeScoringEnabled, ref _soundChangeScoringEnabled, value); }
		}

		public bool SyllablePositionCostEnabled
		{
			get { return _syllablePositionCostEnabled; }
			set { SetChanged(() => SyllablePositionCostEnabled, ref _syllablePositionCostEnabled, value); }
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

			var aligner = new Aline(_segmentPool, relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics,
				new AlineSettings
				{
					ExpansionCompressionEnabled = _expansionCompressionEnabled,
					Mode = mode,
					ContextualSoundClasses = _soundClasses.SoundClasses.Select(nc => nc.DomainSoundClass),
					SoundChangeScoringEnabled = _soundChangeScoringEnabled,
					SyllablePositionCostEnabled = _syllablePositionCostEnabled
				});
			_projectService.Project.WordAligners[ComponentIdentifiers.PrimaryWordAligner] = aligner;
			return aligner;
		}
	}
}
