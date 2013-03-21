using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Services;

namespace SIL.Cog.ViewModels
{
	public class VarietyRegionViewModel : WrapperViewModel
	{
		private readonly IDialogService _dialogService;
		private readonly CogProject _project;
		private readonly Variety _variety;
		private readonly GeographicRegion _region;
		private readonly ObservableCollection<Tuple<double, double>> _coordinates;
		private readonly ICommand _editCommand;
		private readonly ICommand _removeCommand;
		private int _clusterIndex;

		public VarietyRegionViewModel(IDialogService dialogService, CogProject project, Variety variety, GeographicRegion region)
			: base(region)
		{
			_dialogService = dialogService;
			_project = project;
			_variety = variety;
			_region = region;
			_coordinates = new ObservableCollection<Tuple<double, double>>(_region.Coordinates.Select(coord => Tuple.Create(coord.Latitude, coord.Longitude)).ToList());
			_coordinates.CollectionChanged += CoordinatesChanged;
			_editCommand = new RelayCommand(EditRegion);
			_removeCommand = new RelayCommand(RemoveRegion);
		}

		private void CoordinatesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Tuple<double, double> addCoord = _coordinates[e.NewStartingIndex];
					_region.Coordinates.Insert(e.NewStartingIndex, new GeographicCoordinate(addCoord.Item1, addCoord.Item2));
					break;

				case NotifyCollectionChangedAction.Replace:
					Tuple<double, double> replaceCoord = _coordinates[e.NewStartingIndex];
					_region.Coordinates[e.NewStartingIndex] = new GeographicCoordinate(replaceCoord.Item1, replaceCoord.Item2);
					break;
			}
			IsChanged = true;
		}

		private void EditRegion()
		{
			var vm = new EditRegionViewModel(_project, _variety, _region);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				_region.Description = vm.Description;
				IsChanged = true;
				if (vm.CurrentVariety.ModelVariety != _variety)
				{
					_variety.Regions.Remove(_region);
					vm.CurrentVariety.ModelVariety.Regions.Add(_region);
				}
			}
		}

		private void RemoveRegion()
		{
			_variety.Regions.Remove(_region);
		}

		public ObservableCollection<Tuple<double, double>> Coordinates
		{
			get { return _coordinates; }
		}

		public string VarietyName
		{
			get { return _variety.Name; }
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

		public int ClusterIndex
		{
			get { return _clusterIndex; }
			set { Set(() => ClusterIndex, ref _clusterIndex, value); }
		}

		public ICommand EditCommand
		{
			get { return _editCommand; }
		}

		public ICommand RemoveCommand
		{
			get { return _removeCommand; }
		}

		public Variety ModelVariety
		{
			get { return _variety; }
		}

		public GeographicRegion ModelRegion
		{
			get { return _region; }
		}
	}
}
