using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Application.Collections;
using SIL.Cog.Domain;
using SIL.Machine.FeatureModel;
using SIL.ObjectModel;

namespace SIL.Cog.Application.ViewModels
{
	public class EditNaturalClassViewModel : EditSoundClassViewModel
	{
		private SoundType _type;
		private readonly BindableList<FeatureViewModel> _availableFeatures;
		private readonly BindableList<FeatureViewModel> _activeFeatures;
		private FeatureViewModel _selectedActiveFeature;
		private FeatureViewModel _selectedAvailableFeature;
		private readonly ICommand _addCommand;
		private readonly ICommand _removeCommand;

		public EditNaturalClassViewModel(FeatureSystem featSys, IEnumerable<SoundClass> soundClasses)
			: base("New Feature-based Class", soundClasses)
		{
			_availableFeatures = new BindableList<FeatureViewModel>(featSys.OfType<SymbolicFeature>().Select(f => new FeatureViewModel(f)));
			_activeFeatures = new BindableList<FeatureViewModel>();

			_addCommand = new RelayCommand(AddFeature, CanAddFeature);
			_removeCommand = new RelayCommand(RemoveFeature, CanRemoveFeature);
		}

		public EditNaturalClassViewModel(FeatureSystem featSys, IEnumerable<SoundClass> soundClasses, NaturalClass naturalClass)
			: base("Edit Feature-based Class", soundClasses, naturalClass)
		{
			_type = naturalClass.Type == CogFeatureSystem.ConsonantType ? SoundType.Consonant : SoundType.Vowel;
			_availableFeatures = new BindableList<FeatureViewModel>();
			_activeFeatures = new BindableList<FeatureViewModel>();
			foreach (SymbolicFeature feature in featSys.OfType<SymbolicFeature>())
			{
				SymbolicFeatureValue sfv;
				if (naturalClass.FeatureStruct.TryGetValue(feature, out sfv))
					_activeFeatures.Add(new FeatureViewModel(feature, (FeatureSymbol) sfv));
				else
					_availableFeatures.Add(new FeatureViewModel(feature));
			}

			_addCommand = new RelayCommand(AddFeature, CanAddFeature);
			_removeCommand = new RelayCommand(RemoveFeature, CanRemoveFeature);
		}

		private bool CanAddFeature()
		{
			return _selectedAvailableFeature != null;
		}

		private void AddFeature()
		{
			FeatureViewModel feature = _selectedAvailableFeature;
			_availableFeatures.Remove(feature);
			if (feature.Values.Count > 0)
				feature.SelectedValue = feature.Values[0];
			_activeFeatures.Add(feature);
			SelectedActiveFeature = feature;
		}

		private bool CanRemoveFeature()
		{
			return _selectedActiveFeature != null;
		}

		private void RemoveFeature()
		{
			FeatureViewModel feature = _selectedActiveFeature;
			_activeFeatures.Remove(feature);
			_availableFeatures.Add(feature);
			SelectedAvailableFeature = feature;
		}

		public SoundType Type
		{
			get { return _type; }
			set { Set(() => Type, ref _type, value); }
		}

		public FeatureViewModel SelectedAvailableFeature
		{
			get { return _selectedAvailableFeature; }
			set { Set(() => SelectedAvailableFeature, ref _selectedAvailableFeature, value); }
		}

		public ObservableList<FeatureViewModel> AvailableFeatures
		{
			get { return _availableFeatures; }
		}

		public FeatureViewModel SelectedActiveFeature
		{
			get { return _selectedActiveFeature; }
			set { Set(() => SelectedActiveFeature, ref _selectedActiveFeature, value); }
		}

		public ObservableList<FeatureViewModel> ActiveFeatures
		{
			get { return _activeFeatures; }
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
