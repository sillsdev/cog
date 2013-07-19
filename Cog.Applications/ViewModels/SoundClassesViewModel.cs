using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Applications.ViewModels
{
	public class SoundClassesViewModel : ChangeTrackingViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly IDialogService _dialogService;
		private readonly BindableList<SoundClassViewModel> _soundClasses;
		private SoundClassViewModel _currentSoundClass;
		private readonly ICommand _newNaturalClassCommand;
		private readonly ICommand _newUnnaturalClassCommand;
		private readonly ICommand _editSoundClassCommand;
		private readonly ICommand _removeSoundClassCommand;
		private readonly ICommand _moveSoundClassUpCommand;
		private readonly ICommand _moveSoundClassDownCommand;
		private bool _displaySonority;

		public SoundClassesViewModel(IProjectService projectService, IDialogService dialogService)
		{
			_projectService = projectService;
			_dialogService = dialogService;
			_soundClasses = new BindableList<SoundClassViewModel>();
			_soundClasses.CollectionChanged += _soundClasses_CollectionChanged;

			_newNaturalClassCommand = new RelayCommand(NewNaturalClass);
			_newUnnaturalClassCommand = new RelayCommand(NewUnnaturalClass);
			_editSoundClassCommand = new RelayCommand(EditSoundClass, CanEditSoundClass);
			_removeSoundClassCommand = new RelayCommand(RemoveSoundClass, CanRemoveSoundClass);
			_moveSoundClassUpCommand = new RelayCommand(MoveSoundClassUp, CanMoveSoundClassUp);
			_moveSoundClassDownCommand = new RelayCommand(MoveSoundClassDown, CanMoveSoundClassDown);
		}

		private void _soundClasses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddSoundClasses(e.NewItems.Cast<SoundClassViewModel>());
					break;

				case NotifyCollectionChangedAction.Remove:
					RemoveSoundClasses(e.OldItems.Cast<SoundClassViewModel>());
					break;

				case NotifyCollectionChangedAction.Replace:
					RemoveSoundClasses(e.OldItems.Cast<SoundClassViewModel>());
					AddSoundClasses(e.NewItems.Cast<SoundClassViewModel>());
					break;

				case NotifyCollectionChangedAction.Reset:
					AddSoundClasses(_soundClasses);
					break;
			}
		}

		private void AddSoundClasses(IEnumerable<SoundClassViewModel> soundClasses)
		{
			foreach (SoundClassViewModel soundClass in soundClasses)
				soundClass.PropertyChanged += ChildPropertyChanged;
		}

		private void RemoveSoundClasses(IEnumerable<SoundClassViewModel> soundClasses)
		{
			foreach (SoundClassViewModel soundClass in soundClasses)
				soundClass.PropertyChanged -= ChildPropertyChanged;
		}

		private void NewNaturalClass()
		{
			var vm = new EditNaturalClassViewModel(_projectService.Project.FeatureSystem, _soundClasses.Select(nc => nc.DomainSoundClass));
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var fs = new FeatureStruct();
				fs.AddValue(CogFeatureSystem.Type, vm.Type == SoundType.Consonant ? CogFeatureSystem.ConsonantType : CogFeatureSystem.VowelType);
				foreach (FeatureViewModel feature in vm.SelectedFeatures)
					fs.AddValue(feature.DomainFeature, feature.CurrentValue.DomainSymbol);
				var newNaturalClass = new SoundClassViewModel(new NaturalClass(vm.Name, fs), _displaySonority ? 0 : -1);
				IsChanged = true;
				_soundClasses.Add(newNaturalClass);
				CurrentSoundClass = newNaturalClass;
			}
		}

		private void NewUnnaturalClass()
		{
			var vm = new EditUnnaturalClassViewModel(_dialogService, _projectService.Project.Segmenter, _soundClasses.Select(nc => nc.DomainSoundClass));
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var newUnnaturalClass = new SoundClassViewModel(new UnnaturalClass(vm.Name, vm.Segments, vm.IgnoreModifiers, _projectService.Project.Segmenter), _displaySonority ? 0 : -1);
				IsChanged = true;
				_soundClasses.Add(newUnnaturalClass);
				CurrentSoundClass = newUnnaturalClass;
			}
		}

		private bool CanEditSoundClass()
		{
			return _currentSoundClass != null;
		}

		private void EditSoundClass()
		{
			var currentNC = _currentSoundClass.DomainSoundClass as NaturalClass;
			if (currentNC != null)
			{
				var vm = new EditNaturalClassViewModel(_projectService.Project.FeatureSystem, _soundClasses.Select(nc => nc.DomainSoundClass), currentNC);
				if (_dialogService.ShowModalDialog(this, vm) == true)
				{
					var fs = new FeatureStruct();
					fs.AddValue(CogFeatureSystem.Type, vm.Type == SoundType.Consonant ? CogFeatureSystem.ConsonantType : CogFeatureSystem.VowelType);
					foreach (FeatureViewModel feature in vm.SelectedFeatures)
						fs.AddValue(feature.DomainFeature, feature.CurrentValue.DomainSymbol);
					var newNaturalClass = new SoundClassViewModel(new NaturalClass(vm.Name, fs), _currentSoundClass.Sonority);
					int index = _soundClasses.IndexOf(_currentSoundClass);
					IsChanged = true;
					_soundClasses[index] = newNaturalClass;
					CurrentSoundClass = newNaturalClass;
				}
			}
			else
			{
				var currentUnc = _currentSoundClass.DomainSoundClass as UnnaturalClass;
				if (currentUnc != null)
				{
					var vm = new EditUnnaturalClassViewModel(_dialogService, _projectService.Project.Segmenter, _soundClasses.Select(nc => nc.DomainSoundClass), currentUnc);
					if (_dialogService.ShowModalDialog(this, vm) == true)
					{
						var newUnnaturalClass = new SoundClassViewModel(new UnnaturalClass(vm.Name, vm.Segments, vm.IgnoreModifiers, _projectService.Project.Segmenter), _currentSoundClass.Sonority);
						int index = _soundClasses.IndexOf(_currentSoundClass);
						IsChanged = true;
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
			IsChanged = true;
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
			IsChanged = true;
			_soundClasses.Move(index, index - 1);
		}

		private bool CanMoveSoundClassDown()
		{
			return _currentSoundClass != null && _soundClasses.IndexOf(_currentSoundClass) < _soundClasses.Count - 1;
		}

		private void MoveSoundClassDown()
		{
			int index = _soundClasses.IndexOf(_currentSoundClass);
			IsChanged = true;
			_soundClasses.Move(index, index + 1);
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			ChildrenAcceptChanges(_soundClasses);
		}

		public SoundClassViewModel CurrentSoundClass
		{
			get { return _currentSoundClass; }
			set { Set(() => CurrentSoundClass, ref _currentSoundClass, value); }
		}

		public bool DisplaySonority
		{
			get { return _displaySonority; }
			set { Set(() => DisplaySonority, ref _displaySonority, value); }
		}

		public ObservableList<SoundClassViewModel> SoundClasses
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
