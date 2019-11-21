namespace SIL.Cog.Explorer.Models
{
	public abstract class DataGridColumn
	{
		protected DataGridColumn(string title)
		{
			Title = title;
		}

		public string Title { get; }
	}
}
