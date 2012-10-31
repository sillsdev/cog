using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Aligners;
using SIL.Cog.Services;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public enum AlineMode
	{
		[Description("Local")]
		Local,
		[Description("Global")]
		Global,
		[Description("Semi-global")]
		SemiGlobal,
		[Description("Half-local")]
		HalfLocal
	}

	public class AlineViewModel : ComponentSettingsViewModelBase
	{
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private readonly IDialogService _dialogService;
		private AlineMode _mode;
		private bool _disableExpansionCompression;
		private readonly ReadOnlyCollection<RelevantFeatureViewModel> _features;
		private readonly ObservableCollection<NaturalClassViewModel> _naturalClasses;
		private NaturalClassViewModel _currentNaturalClass;
		private readonly ICommand _newNaturalClassCommand;
		private readonly ICommand _editNaturalClassCommand;
		private readonly ICommand _removeNaturalClassCommand;

		public AlineViewModel(SpanFactory<ShapeNode> spanFactory, IDialogService dialogService, CogProject project, Aline aligner)
			: base("Alignment", project)
		{
			_spanFactory = spanFactory;
			_dialogService = dialogService;
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
					_mode = AlineMode.Local;
					break;
				case AlignerMode.Global:
					_mode = AlineMode.Global;
					break;
				case AlignerMode.SemiGlobal:
					_mode = AlineMode.SemiGlobal;
					break;
				case AlignerMode.HalfLocal:
					_mode = AlineMode.HalfLocal;
					break;
			}
			_disableExpansionCompression = aligner.Settings.DisableExpansionCompression;
			_naturalClasses = new ObservableCollection<NaturalClassViewModel>(aligner.NaturalClasses.Select(nc => new NaturalClassViewModel(nc)));
			if (_naturalClasses.Count > 0)
				_currentNaturalClass = _naturalClasses[0];

			_newNaturalClassCommand = new RelayCommand(NewNaturalClass);
			_editNaturalClassCommand = new RelayCommand(EditNaturalClass, CanEditNaturalClass);
			_removeNaturalClassCommand = new RelayCommand(RemoveNaturalClass, CanRemoveNaturalClass);
		}

		private void NewNaturalClass()
		{
			var vm = new EditNaturalClassViewModel(Project.FeatureSystem, _naturalClasses.Select(nc => nc.ModelNaturalClass));
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				var fs = new FeatureStruct();
				fs.AddValue(CogFeatureSystem.Type, vm.Type == SoundType.Consonant ? CogFeatureSystem.ConsonantType : CogFeatureSystem.VowelType);
				foreach (FeatureViewModel feature in vm.SelectedFeatures)
					fs.AddValue(feature.ModelFeature, feature.CurrentValue.ModelSymbol);
				var newNaturalClass = new NaturalClassViewModel(new NaturalClass(vm.Name, fs));
				_naturalClasses.Add(newNaturalClass);
				CurrentNaturalClass = newNaturalClass;
				IsChanged = true;
			}
		}

		private bool CanEditNaturalClass()
		{
			return _currentNaturalClass != null;
		}

		private void EditNaturalClass()
		{
			var vm = new EditNaturalClassViewModel(Project.FeatureSystem, _naturalClasses.Select(nc => nc.ModelNaturalClass), _currentNaturalClass.ModelNaturalClass);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				var fs = new FeatureStruct();
				fs.AddValue(CogFeatureSystem.Type, vm.Type == SoundType.Consonant ? CogFeatureSystem.ConsonantType : CogFeatureSystem.VowelType);
				foreach (FeatureViewModel feature in vm.SelectedFeatures)
					fs.AddValue(feature.ModelFeature, feature.CurrentValue.ModelSymbol);
				var newNaturalClass = new NaturalClassViewModel(new NaturalClass(vm.Name, fs));
				int index = _naturalClasses.IndexOf(_currentNaturalClass);
				_naturalClasses[index] = newNaturalClass;
				CurrentNaturalClass = newNaturalClass;
				IsChanged = true;
			}
		}

		private bool CanRemoveNaturalClass()
		{
			return _currentNaturalClass != null;
		}

		private void RemoveNaturalClass()
		{
			int index = _naturalClasses.IndexOf(_currentNaturalClass);
			_naturalClasses.RemoveAt(index);
			CurrentNaturalClass = index < _naturalClasses.Count ? _naturalClasses[index] : null;
			IsChanged = true;
		}

		public AlineMode Mode
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

		public NaturalClassViewModel CurrentNaturalClass
		{
			get { return _currentNaturalClass; }
			set { Set(() => CurrentNaturalClass, ref _currentNaturalClass, value); }
		}

		public ObservableCollection<NaturalClassViewModel> NaturalClasses
		{
			get { return _naturalClasses; }
		}

		public ICommand NewNaturalClassCommand
		{
			get { return _newNaturalClassCommand; }
		}

		public ICommand EditNaturalClassCommand
		{
			get { return _editNaturalClassCommand; }
		}

		public ICommand RemoveNaturalClassCommand
		{
			get { return _removeNaturalClassCommand; }
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			foreach (RelevantFeatureViewModel relevantFeature in _features)
				relevantFeature.AcceptChanges();
		}

		public override void UpdateComponent()
		{
			var mode = AlignerMode.Local;
			switch (_mode)
			{
				case AlineMode.Local:
					mode = AlignerMode.Local;
					break;
				case AlineMode.Global:
					mode = AlignerMode.Global;
					break;
				case AlineMode.SemiGlobal:
					mode = AlignerMode.SemiGlobal;
					break;
				case AlineMode.HalfLocal:
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
				new AlignerSettings {DisableExpansionCompression = _disableExpansionCompression, Mode = mode, NaturalClasses = _naturalClasses.Select(nc => nc.ModelNaturalClass)});
		}
	}
}
