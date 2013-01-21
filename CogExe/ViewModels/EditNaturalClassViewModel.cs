using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public class EditNaturalClassViewModel : CogViewModelBase, IDataErrorInfo
	{
		private string _name;
		private SoundType _type;
		private readonly HashSet<string> _naturalClassNames;
		private readonly ObservableCollection<FeatureViewModel> _availableFeatures;
		private readonly ObservableCollection<FeatureViewModel> _selectedFeatures;
		private FeatureViewModel _currentSelectedFeature;
		private FeatureViewModel _currentAvailableFeature;
		private readonly ICommand _addCommand;
		private readonly ICommand _removeCommand;

		public EditNaturalClassViewModel(FeatureSystem featSys, IEnumerable<NaturalClass> naturalClasses)
			: this("New Natural Class")
		{
			_availableFeatures = new ObservableCollection<FeatureViewModel>(featSys.OfType<SymbolicFeature>().Select(f => new FeatureViewModel(f)));
			_selectedFeatures = new ObservableCollection<FeatureViewModel>();
			_naturalClassNames = new HashSet<string>(naturalClasses.Select(nc => nc.Name));
		}

		public EditNaturalClassViewModel(FeatureSystem featSys, IEnumerable<NaturalClass> naturalClasses, NaturalClass naturalClass)
			: this("Edit Natural Class")
		{
			_name = naturalClass.Name;
			_availableFeatures = new ObservableCollection<FeatureViewModel>();
			_selectedFeatures = new ObservableCollection<FeatureViewModel>();
			foreach (SymbolicFeature feature in featSys.OfType<SymbolicFeature>())
			{
				SymbolicFeatureValue sfv;
				if (naturalClass.FeatureStruct.TryGetValue(feature, out sfv))
					_selectedFeatures.Add(new FeatureViewModel(feature, (FeatureSymbol) sfv));
				else
					_availableFeatures.Add(new FeatureViewModel(feature));
			}
			_naturalClassNames = new HashSet<string>(naturalClasses.Where(nc => nc != naturalClass).Select(nc => nc.Name));
		}

		private EditNaturalClassViewModel(string displayName)
			: base(displayName)
		{
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

		public string Name
		{
			get { return _name; }
			set { Set(() => Name, ref _name, value); }
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

		public ObservableCollection<FeatureViewModel> AvailableFeatures
		{
			get { return _availableFeatures; }
		}

		public FeatureViewModel CurrentSelectedFeature
		{
			get { return _currentSelectedFeature; }
			set { Set(() => CurrentSelectedFeature, ref _currentSelectedFeature, value); }
		}

		public ObservableCollection<FeatureViewModel> SelectedFeatures
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

		public string this[string columnName]
		{
			get
			{
				switch (columnName)
				{
					case "Name":
						if (string.IsNullOrEmpty(_name))
							return "Please enter a name";
						if (_naturalClassNames.Contains(_name))
							return "A natural class with that name already exists";
						break;
				}

				return null;
			}
		}

		public string Error
		{
			get { return null; }
		}
	}
}
