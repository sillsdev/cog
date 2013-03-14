using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Services;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public class SoundClassesViewModel : ViewModelBase
	{
		private readonly CogProject _project;
		private readonly IDialogService _dialogService;
		private readonly ObservableCollection<SoundClassViewModel> _soundClasses;
		private SoundClassViewModel _currentSoundClass;
		private readonly ICommand _newNaturalClassCommand;
		private readonly ICommand _newUnnaturalClassCommand;
		private readonly ICommand _editSoundClassCommand;
		private readonly ICommand _removeSoundClassCommand;
		private readonly ICommand _moveSoundClassUpCommand;
		private readonly ICommand _moveSoundClassDownCommand;

		public SoundClassesViewModel(IDialogService dialogService, CogProject project, IEnumerable<SoundClass> soundClasses)
		{
			_dialogService = dialogService;
			_project = project;
			_soundClasses = new ObservableCollection<SoundClassViewModel>(soundClasses.Select(nc => new SoundClassViewModel(nc)));
			if (_soundClasses.Count > 0)
				_currentSoundClass = _soundClasses[0];

			_newNaturalClassCommand = new RelayCommand(NewNaturalClass);
			_newUnnaturalClassCommand = new RelayCommand(NewUnnaturalClass);
			_editSoundClassCommand = new RelayCommand(EditSoundClass, CanEditSoundClass);
			_removeSoundClassCommand = new RelayCommand(RemoveSoundClass, CanRemoveSoundClass);
			_moveSoundClassUpCommand = new RelayCommand(MoveSoundClassUp, CanMoveSoundClassUp);
			_moveSoundClassDownCommand = new RelayCommand(MoveSoundClassDown, CanMoveSoundClassDown);
		}

		private void NewNaturalClass()
		{
			var vm = new EditNaturalClassViewModel(_project.FeatureSystem, _soundClasses.Select(nc => nc.ModelSoundClass));
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				var fs = new FeatureStruct();
				fs.AddValue(CogFeatureSystem.Type, vm.Type == SoundType.Consonant ? CogFeatureSystem.ConsonantType : CogFeatureSystem.VowelType);
				foreach (FeatureViewModel feature in vm.SelectedFeatures)
					fs.AddValue(feature.ModelFeature, feature.CurrentValue.ModelSymbol);
				var newNaturalClass = new SoundClassViewModel(new NaturalClass(vm.Name, fs));
				_soundClasses.Add(newNaturalClass);
				CurrentSoundClass = newNaturalClass;
			}
		}

		private void NewUnnaturalClass()
		{
			var vm = new EditUnnaturalClassViewModel(_dialogService, _project, _soundClasses.Select(nc => nc.ModelSoundClass));
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				var newNaturalClass = new SoundClassViewModel(new UnnaturalClass(_project.Segmenter, vm.Name, vm.Segments, vm.IgnoreModifiers));
				_soundClasses.Add(newNaturalClass);
				CurrentSoundClass = newNaturalClass;
			}
		}

		private bool CanEditSoundClass()
		{
			return _currentSoundClass != null;
		}

		private void EditSoundClass()
		{
			var currentNC = _currentSoundClass.ModelSoundClass as NaturalClass;
			if (currentNC != null)
			{
				var vm = new EditNaturalClassViewModel(_project.FeatureSystem, _soundClasses.Select(nc => nc.ModelSoundClass), currentNC);
				if (_dialogService.ShowDialog(this, vm) == true)
				{
					var fs = new FeatureStruct();
					fs.AddValue(CogFeatureSystem.Type, vm.Type == SoundType.Consonant ? CogFeatureSystem.ConsonantType : CogFeatureSystem.VowelType);
					foreach (FeatureViewModel feature in vm.SelectedFeatures)
						fs.AddValue(feature.ModelFeature, feature.CurrentValue.ModelSymbol);
					var newNaturalClass = new SoundClassViewModel(new NaturalClass(vm.Name, fs));
					int index = _soundClasses.IndexOf(_currentSoundClass);
					_soundClasses[index] = newNaturalClass;
					CurrentSoundClass = newNaturalClass;
				}
			}
			else
			{
				var currentUnc = _currentSoundClass.ModelSoundClass as UnnaturalClass;
				if (currentUnc != null)
				{
					var vm = new EditUnnaturalClassViewModel(_dialogService, _project, _soundClasses.Select(nc => nc.ModelSoundClass), currentUnc);
					if (_dialogService.ShowDialog(this, vm) == true)
					{
						var newUnnaturalClass = new SoundClassViewModel(new UnnaturalClass(_project.Segmenter, vm.Name, vm.Segments, vm.IgnoreModifiers));
						int index = _soundClasses.IndexOf(_currentSoundClass);
						_soundClasses[index] = newUnnaturalClass;
						CurrentSoundClass = newUnnaturalClass;
					}
				}
			}
		}

		private bool CanRemoveSoundClass()
		{
			return _currentSoundClass != null;
		}

		private void RemoveSoundClass()
		{
			int index = _soundClasses.IndexOf(_currentSoundClass);
			_soundClasses.RemoveAt(index);
			if (_soundClasses.Count == 0)
				CurrentSoundClass = null;
			else
				CurrentSoundClass = index >= _soundClasses.Count ? _soundClasses[index - 1] : _soundClasses[index];
		}

		private bool CanMoveSoundClassUp()
		{
			return _currentSoundClass != null && _soundClasses.IndexOf(_currentSoundClass) > 0;
		}

		private void MoveSoundClassUp()
		{
			int index = _soundClasses.IndexOf(_currentSoundClass);
			_soundClasses.Move(index, index - 1);
		}

		private bool CanMoveSoundClassDown()
		{
			return _currentSoundClass != null && _soundClasses.IndexOf(_currentSoundClass) < _soundClasses.Count - 1;
		}

		private void MoveSoundClassDown()
		{
			int index = _soundClasses.IndexOf(_currentSoundClass);
			_soundClasses.Move(index, index + 1);
		}

		public SoundClassViewModel CurrentSoundClass
		{
			get { return _currentSoundClass; }
			set { Set(() => CurrentSoundClass, ref _currentSoundClass, value); }
		}

		public ObservableCollection<SoundClassViewModel> SoundClasses
		{
			get { return _soundClasses; }
		}

		public ICommand NewNaturalClassCommand
		{
			get { return _newNaturalClassCommand; }
		}

		public ICommand NewUnnaturalClassCommand
		{
			get { return _newUnnaturalClassCommand; }
		}

		public ICommand EditSoundClassCommand
		{
			get { return _editSoundClassCommand; }
		}

		public ICommand RemoveSoundClassCommand
		{
			get { return _removeSoundClassCommand; }
		}

		public ICommand MoveSoundClassUpCommand
		{
			get { return _moveSoundClassUpCommand; }
		}

		public ICommand MoveSoundClassDownCommand
		{
			get { return _moveSoundClassDownCommand; }
		}
	}
}
