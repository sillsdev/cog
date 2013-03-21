using System;

namespace SIL.Cog
{
	public class GeographicCoordinate : IEquatable<GeographicCoordinate>
	{
		private readonly double _latitude;
		private readonly double _longitude;

		public GeographicCoordinate(double latitude, double longitude)
		{
			_latitude = latitude;
			_longitude = longitude;
		}

		public double Latitude
		{
			get { return _latitude; }
		}

		public double Longitude
		{
			get { return _longitude; }
		}

		public override bool Equals(object obj)
		{
			if (!(obj is GeographicCoordinate))
				return false;
			return Equals((GeographicCoordinate) obj);
		}

		public bool Equals(GeographicCoordinate other)
		{
			return other != null && Math.Abs(_latitude - other._latitude) < double.Epsilon && Math.Abs(_longitude - other._longitude) < double.Epsilon;
		}

		public override int GetHashCode()
		{
			int code = 23;
			code += code * 31 + _latitude.GetHashCode();
			code += code * 31 + _longitude.GetHashCode();
			return code;
		}
	}
}
