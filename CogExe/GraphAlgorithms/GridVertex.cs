namespace SIL.Cog.GraphAlgorithms
{
	public abstract class GridVertex : IGridVertex
	{
		protected GridVertex()
		{
 			RowSpan = 1;
			ColumnSpan = 1;
			HorizontalAlignment = GridHorizontalAlignment.Center;
			VerticalAlignment = GridVerticalAlignment.Center;
		}

		public int Row { get; set; }

		public int Column { get; set; }

		public int RowSpan { get; set; }

		public int ColumnSpan { get; set; }

		public GridHorizontalAlignment HorizontalAlignment { get; set; }

		public GridVerticalAlignment VerticalAlignment { get; set; }
	}
}
