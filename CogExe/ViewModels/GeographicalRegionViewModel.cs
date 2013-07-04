using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Services;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class GeographicalRegionViewModel : WrapperViewModel
	{
		private readonly IDialogService _dialogService;
		private readonly CogProject _project;
		private readonly GeographicalVarietyViewModel _variety;
		private readonly GeographicRegion _region;
		private readonly BindableList<Tuple<double, double>> _coordinates;
		private readonly ICommand _editCommand;
		private readonly ICommand _removeCommand;

		public GeographicalRegionViewModel(IDialogService dialogService, CogProject project, GeographicalVarietyViewModel variety, GeographicRegion region)
			: base(region)
		{
			_dialogService = dialogService;
			_project = project;
			_variety = variety;
			_region = region;
			_coordinates = new BindableList<Tuple<double, double>>(_region.Coordinates.Select(coord => Tuple.Create(coord.Latitude, coord.Longitude)));
			_coordinates.CollectionChanged += CoordinatesChanged;
			_editCommand = new RelayCommand(EditRegion);
			_removeCommand = new RelayCommand(RemoveRegion);
		}

		private void CoordinatesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					var newCoord = (Tuple<double, double>) e.NewItems[0];
					_region.Coordinates.Insert(e.NewStartingIndex, new GeographicCoordinate(newCoord.Item1, newCoord.Item2));
					break;

				case NotifyCollectionChangedAction.Replace:
					var replCoord = (Tuple<double, double>) e.NewItems[0];
					_region.Coordinates[e.NewStartingIndex] = new GeographicCoordinate(replCoord.Item1, replCoord.Item2);
					break;

				case NotifyCollectionChangedAction.Remove:
					_region.Coordinates.RemoveAt(e.OldStartingIndex);
					break;

				case NotifyCollectionChangedAction.Move:
					_region.Coordinates.Move(e.OldStartingIndex, e.NewStartingIndex);
					break;

				case NotifyCollectionChangedAction.Reset:
					_region.Coordinates.Clear();
					break;
			}
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

		public ObservableList<Tuple<double, double>> Coordinates
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
			set { _region.Description = value; }
		}

		public ICommand EditCommand
		{
			get { return _editCommand; }
		}

		public ICommand RemoveCommand
		{
			get { return _removeCommand; }
		}

		internal GeographicRegion ModelRegion
		{
			get { return _region; }
		}
	}
}
