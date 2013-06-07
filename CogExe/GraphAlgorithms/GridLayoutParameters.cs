using System.Collections.Generic;
using GraphSharp.Algorithms.Layout;

namespace SIL.Cog.GraphAlgorithms
{
	public class GridLayoutRow
	{
		public bool AutoHeight { get; set; }
		public double Height { get; set; }
	}

	public class GridLayoutColumn
	{
		public bool AutoWidth { get; set; }
		public double Width { get; set; }
	}

	public class GridLayoutParameters : LayoutParametersBase
	{
		private readonly List<GridLayoutRow> _rows;
		private readonly List<GridLayoutColumn> _columns;
		private int _gridThickness;

		public GridLayoutParameters()
		{
			_rows = new List<GridLayoutRow>();
			_columns = new List<GridLayoutColumn>();
			_gridThickness = 1;
		}

		public List<GridLayoutRow> Rows
		{
			get { return _rows; }
		}

		public List<GridLayoutColumn> Columns
		{
			get { return _columns; }
		}

		public int GridThickness
		{
			get { return _gridThickness; }
			set
			{
				_gridThickness = value;
				NotifyPropertyChanged("GridThickness");
			}
		}
	}
}
