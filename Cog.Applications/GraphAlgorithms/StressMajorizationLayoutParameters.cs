using GraphSharp.Algorithms.Layout;

namespace SIL.Cog.Applications.GraphAlgorithms
{
	public class StressMajorizationLayoutParameters : LayoutParametersBase
	{
		private double _width = 300;
		/// <summary>
		/// Width of the bounding box.
		/// </summary>
		public double Width
		{
			get { return _width; }
			set
			{
				_width = value;
				NotifyPropertyChanged("Width");
			}
		}

		private double _height = 300;
		/// <summary>
		/// Height of the bounding box.
		/// </summary>
		public double Height
		{
			get { return _height; }
			set
			{
				_height = value;
				NotifyPropertyChanged("Height");
			}
		}

		private int _maxIterations = 1000;
		/// <summary>
		/// Maximum number of the iterations.
		/// </summary>
		public int MaxIterations
		{
			get { return _maxIterations; }
			set
			{
				_maxIterations = value;
				NotifyPropertyChanged("MaxIterations");
			}
		}

		private double _alpha = 2.0;

		public double Alpha
		{
			get { return _alpha; }
			set 
			{
				_alpha = value;
				NotifyPropertyChanged("Alpha");
			}
		}

		private double _lengthFactor = 1;
		/// <summary>
		/// Multiplier of the ideal edge length. (With this parameter the user can modify the ideal edge length).
		/// </summary>
		public double LengthFactor
		{
			get { return _lengthFactor; }
			set
			{
				_lengthFactor = value;
				NotifyPropertyChanged("LengthFactor");
			}
		}

		private double _disconnectedMultiplier = 0.5;
		/// <summary>
		/// Ideal distance between the disconnected points (1 is equal the ideal edge length).
		/// </summary>
		public double DisconnectedMultiplier
		{
			get { return _disconnectedMultiplier; }
			set
			{
				_disconnectedMultiplier = value;
				NotifyPropertyChanged("DisconnectedMultiplier");
			}
		}

		private double _weightAdjustment;

		public double WeightAdjustment
		{
			get { return _weightAdjustment; }
			set
			{
				_weightAdjustment = value;
				NotifyPropertyChanged("WeightAdjustment");
			}
		}
	}
}
