using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class GeographicalRegionViewModel : WrapperViewModel
	{
		public delegate GeographicalRegionViewModel Factory(GeographicalVarietyViewModel variety, GeographicRegion region);

		private readonly IDialogService _dialogService;
		private readonly IProjectService _projectService;
		private readonly GeographicalVarietyViewModel _variety;
		private readonly GeographicRegion _region;
		private readonly BindableList<Tuple<double, double>> _coordinates;
		private readonly ICommand _editCommand;
		private readonly ICommand _removeCommand;

		public GeographicalRegionViewModel(IProjectService projectService, IDialogService dialogService, GeographicalVarietyViewModel variety, GeographicRegion region)
			: base(region)
		{
			_projectService = projectService;
			_dialogService = dialogService;

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
			Messenger.Default.Send(new DomainModelChangedMessage(false));
		}

		private void EditRegion()
		{
			var vm = new EditRegionViewModel(_projectService.Project.Varieties, _variety.DomainVariety, _region);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				_region.Description = vm.Description;
				if (vm.SelectedVariety.DomainVariety != _variety.DomainVariety)
				{
					_variety.DomainVariety.Regions.Remove(_region);
					vm.SelectedVariety.DomainVariety.Regions.Add(_region);
				}
				Messenger.Default.Send(new DomainModelChangedMessage(false));
			}
		}

		private void RemoveRegion()
		{
			_variety.DomainVariety.Regions.Remove(_region);
			Messenger.Default.Send(new DomainModelChangedMessage(false));
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
		}

		public ICommand EditCommand
		{
			get { return _editCommand; }
		}

		public ICommand RemoveCommand
		{
			get { return _removeCommand; }
		}

		internal GeographicRegion DomainRegion
		{
			get { return _region; }
		}
	}
}
