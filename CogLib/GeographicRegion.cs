using System.Collections.Generic;
using System.Collections.ObjectModel;
using SIL.Collections;

namespace SIL.Cog
{
	public class GeographicRegion : NotifyPropertyChangedBase
	{
		private string _desc;
		private readonly ObservableCollection<GeographicCoordinate> _coordinates;

		public GeographicRegion()
		{
			_coordinates = new ObservableCollection<GeographicCoordinate>();
		}

		public GeographicRegion(IEnumerable<GeographicCoordinate> coordinates)
		{
			_coordinates = new ObservableCollection<GeographicCoordinate>(coordinates);
		}

		public ObservableCollection<GeographicCoordinate> Coordinates
		{
			get { return _coordinates; }
		}

		public string Description
		{
			get { return _desc; }
			set
			{
				_desc = value;
				OnPropertyChanged("Description");
			}
		}
	}
}
