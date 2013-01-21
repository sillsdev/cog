using GraphSharp.Algorithms.Layout;

namespace SIL.Cog.Controls
{
	public enum BranchLengthScaling
	{
		MinimizeLabelOverlapAverage,
		MinimizeLabelOverlapMinimum,
		FixedMinimumLength
	}

	public class RadialTreeLayoutParameters : LayoutParametersBase
	{
		private BranchLengthScaling _branchLengthScaling;
		private double _minLen;

		public RadialTreeLayoutParameters()
		{
			_minLen = 10;
		}

		public BranchLengthScaling BranchLengthScaling
		{
			get { return _branchLengthScaling; }
			set
			{
				_branchLengthScaling = value;
				NotifyPropertyChanged("BranchLengthScaling");
			}
		}

		public double MinimumLength
		{
			get { return _minLen; }
			set
			{
				_minLen = value;
				NotifyPropertyChanged("MinimumLength");
			}
		}
	}
}
