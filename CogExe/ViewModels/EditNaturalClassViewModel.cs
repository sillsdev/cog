using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Collections;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public class EditNaturalClassViewModel : EditSoundClassViewModel
	{
		private SoundType _type;
		private readonly ObservableList<FeatureViewModel> _availableFeatures;
		private readonly ObservableList<FeatureViewModel> _selectedFeatures;
		private FeatureViewModel _currentSelectedFeature;
		private FeatureViewModel _currentAvailableFeature;
		private readonly ICommand _addCommand;
		private readonly ICommand _removeCommand;

		public EditNaturalClassViewModel(FeatureSystem featSys, IEnumerable<SoundClass> soundClasses)
			: base("New Natural Class", soundClasses)
		{
			_availableFeatures = new ObservableList<FeatureViewModel>(featSys.OfType<SymbolicFeature>().Select(f => new FeatureViewModel(f)));
			_selectedFeatures = new ObservableList<FeatureViewModel>();

			_addCommand = new RelayCommand(AddFeature, CanAddFeature);
			_removeCommand = new RelayCommand(RemoveFeature, CanRemoveFeature);
		}

		public EditNaturalClassViewModel(FeatureSystem featSys, IEnumerable<SoundClass> soundClasses, NaturalClass naturalClass)
			: base("Edit Natural Class", soundClasses, naturalClass)
		{
			_type = naturalClass.Type == CogFeatureSystem.ConsonantType ? SoundType.Consonant : SoundType.Vowel;
			_availableFeatures = new ObservableList<FeatureViewModel>();
			_selectedFeatures = new ObservableList<FeatureViewModel>();
			foreach (SymbolicFeature feature in featSys.OfType<SymbolicFeature>())
			{
				SymbolicFeatureValue sfv;
				if (naturalClass.FeatureStruct.TryGetValue(feature, out sfv))
					_selectedFeatures.Add(new FeatureViewModel(feature, (FeatureSymbol) sfv));
				else
					_availableFeatures.Add(new FeatureViewModel(feature));
			}

			_addCommand = new RelayCommand(AddFeature, CanAddFeature);
			_removeCommand = new RelayCommand(RemoveFeature, CanRemoveFeature);
		}

		private bool CanAddFeature()
		{
			return _currentAvailableFeature != null;
		}

		private void AddFeature()
		{
			FeatureViewModel feature = _currentAvailableFeature;
			_availableFeatures.Remove(feature);
			if (feature.Values.Count > 0)
				feature.CurrentValue = feature.Values[0];
			_selectedFeatures.Add(feature);
			CurrentSelectedFeature = feature;
		}

		private bool CanRemoveFeature()
		{
			return _currentSelectedFeature != null;
		}

		private void RemoveFeature()
		{
			FeatureViewModel feature = _currentSelectedFeature;
			_selectedFeatures.Remove(feature);
			_availableFeatures.Add(feature);
			CurrentAvailableFeature = feature;
		}

		public SoundType Type
		{
			get { return _type; }
			set { Set(() => Type, ref _type, value); }
		}

		public FeatureViewModel CurrentAvailableFeature
		{
			get { return _currentAvailableFeature; }
			set { Set(() => CurrentAvailableFeature, ref _currentAvailableFeature, value); }
		}

		public ObservableList<FeatureViewModel> AvailableFeatures
		{
			get { return _availableFeatures; }
		}

		public FeatureViewModel CurrentSelectedFeature
		{
			get { return _currentSelectedFeature; }
			set { Set(() => CurrentSelectedFeature, ref _currentSelectedFeature, value); }
		}

		public ObservableList<FeatureViewModel> SelectedFeatures
		{
			get { return _selectedFeatures; }
		}

		public ICommand AddCommand
		{
			get { return _addCommand; }
		}

		public ICommand RemoveCommand
		{
			get { return _removeCommand; }
		}
	}
}
