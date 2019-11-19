using System.Collections.Generic;
using SIL.ObjectModel;

namespace SIL.Cog.Domain
{
	public class GeographicRegion : ObservableObject
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
			set { Set(() => Description, ref _desc, value); }
		}
	}
}
