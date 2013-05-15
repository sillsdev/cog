using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Services;

namespace SIL.Cog.ViewModels
{
	public class GeographicalRegionViewModel : WrapperViewModel
	{
		private readonly IDialogService _dialogService;
		private readonly CogProject _project;
		private readonly GeographicalVarietyViewModel _variety;
		private readonly GeographicRegion _region;
		private readonly ListViewModelCollection<ObservableCollection<GeographicCoordinate>, Tuple<double, double>, GeographicCoordinate> _coordinates;
		private readonly ICommand _editCommand;
		private readonly ICommand _removeCommand;

		public GeographicalRegionViewModel(IDialogService dialogService, CogProject project, GeographicalVarietyViewModel variety, GeographicRegion region)
			: base(region)
		{
			_dialogService = dialogService;
			_project = project;
			_variety = variety;
			_region = region;
			_coordinates = new ListViewModelCollection<ObservableCollection<GeographicCoordinate>, Tuple<double, double>, GeographicCoordinate>(_region.Coordinates,
				coord => Tuple.Create(coord.Latitude, coord.Longitude));
			_coordinates.CollectionChanged += CoordinatesChanged;
			_editCommand = new RelayCommand(EditRegion);
			_removeCommand = new RelayCommand(RemoveRegion);
		}

		private void CoordinatesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		private void EditRegion()
		{
			var vm = new EditRegionViewModel(_project, _variety.ModelVariety, _region);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				_region.Description = vm.Description;
				IsChanged = true;
				if (vm.CurrentVariety.ModelVariety != _variety.ModelVariety)
				{
					_variety.ModelVariety.Regions.Remove(_region);
					vm.CurrentVariety.ModelVariety.Regions.Add(_region);
				}
			}
		}

		private void RemoveRegion()
		{
			_variety.ModelVariety.Regions.Remove(_region);
		}

		public ObservableCollection<Tuple<double, double>> Coordinates
		{
			get { return _coordinates; }
		}

		public GeographicalVarietyViewModel Variety
		{
			get { return _variety; }
		}

		public string Description
		{
			get { return _region.Description; }
			set
			{
				_region.Description = value;
				IsChanged = true;
			}
		}

		public ICommand EditCommand
		{
			get { return _editCommand; }
		}

		public ICommand RemoveCommand
		{
			get { return _removeCommand; }
		}

		public GeographicRegion ModelRegion
		{
			get { return _region; }
		}
	}
}
