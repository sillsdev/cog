using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using SIL.Cog.Aligners;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public class AlineViewModel : ComponentSettingsViewModelBase
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 
		private string _mode;
		private bool _disableExpansionCompression;
		private ReadOnlyCollection<RelevantFeatureViewModel> _relevantConsFeatures;
		private ReadOnlyCollection<RelevantFeatureViewModel> _relevantVowelFeatures;

		public AlineViewModel(SpanFactory<ShapeNode> spanFactory, CogProject project, Aline aligner)
			: base("Alignment", project)
		{
			_spanFactory = spanFactory;
			_relevantConsFeatures = PopulateRelevantFeatures(project.FeatureSystem, aligner.RelevantConsonantFeatures);
			_relevantVowelFeatures = PopulateRelevantFeatures(project.FeatureSystem, aligner.RelevantVowelFeatures);
			project.PropertyChanged += ProjectPropertyChanged;
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

		private ReadOnlyCollection<RelevantFeatureViewModel> PopulateRelevantFeatures(FeatureSystem featSys, IEnumerable<SymbolicFeature> selected)
		{
			var relevantFeatures = new List<RelevantFeatureViewModel>();
			var selectedFeatures = new IDBearerSet<SymbolicFeature>(selected);
			foreach (SymbolicFeature feature in featSys)
			{
				var vm = new RelevantFeatureViewModel(feature, selectedFeatures.Contains(feature));
				vm.PropertyChanged += RelevantFeatureViewModel_PropertyChanged;
				relevantFeatures.Add(vm);
			}

			return new ReadOnlyCollection<RelevantFeatureViewModel>(relevantFeatures);
		}

		private void RelevantFeatureViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			IsChanged = true;
		}

		private void ProjectPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var project = (CogProject) sender;
			switch (e.PropertyName)
			{
				case "FeatureSystem":
					Set(() => RelevantConsonantFeatures, ref _relevantConsFeatures, PopulateRelevantFeatures(project.FeatureSystem, _relevantConsFeatures.Select(vm => vm.ModelFeature)));
					Set(() => RelevantVowelFeatures, ref _relevantVowelFeatures, PopulateRelevantFeatures(project.FeatureSystem, _relevantVowelFeatures.Select(vm => vm.ModelFeature)));
					break;
			}
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

		public ReadOnlyCollection<RelevantFeatureViewModel> RelevantConsonantFeatures
		{
			get { return _relevantConsFeatures; }
		}

		public ReadOnlyCollection<RelevantFeatureViewModel> RelevantVowelFeatures
		{
			get { return _relevantVowelFeatures; }
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

			Project.Aligners["primary"] = new Aline(_spanFactory, _relevantVowelFeatures.Select(f => f.ModelFeature), _relevantConsFeatures.Select(f => f.ModelFeature),
				new AlignerSettings {DisableExpansionCompression = _disableExpansionCompression, Mode = mode});
		}
	}
}
