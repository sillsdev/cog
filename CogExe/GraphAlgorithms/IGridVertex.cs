namespace SIL.Cog.GraphAlgorithms
{
	public enum GridHorizontalAlignment
	{
		Left,
		Center,
		Right
	}

	public enum GridVerticalAlignment
	{
		Top,
		Center,
		Bottom
	}

	public interface IGridVertex
	{
		int Row { get; }
		int Column { get; }
		int RowSpan { get; }
		int ColumnSpan { get; }
		GridHorizontalAlignment HorizontalAlignment { get; }
		GridVerticalAlignment VerticalAlignment { get; }
	}
}
