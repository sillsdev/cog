using System.Collections.Generic;
using SIL.Collections;

namespace SIL.Cog
{
	public class GeographicRegion : NotifyPropertyChangedBase
	{
		private string _desc;
		private readonly ObservableList<GeographicCoordinate> _coordinates;

		public GeographicRegion()
		{
			_coordinates = new ObservableList<GeographicCoordinate>();
		}

		public GeographicRegion(IEnumerable<GeographicCoordinate> coordinates)
		{
			_coordinates = new ObservableList<GeographicCoordinate>(coordinates);
		}

		public ObservableList<GeographicCoordinate> Coordinates
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
