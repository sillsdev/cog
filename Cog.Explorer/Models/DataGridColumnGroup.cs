using System.Collections.Generic;

namespace SIL.Cog.Explorer.Models
{
	public class DataGridColumnGroup : DataGridColumn
	{
		public DataGridColumnGroup(string title)
			: base(title)
		{
		}

		public List<DataGridValueColumn> Columns { get; set; } = new List<DataGridValueColumn>();
	}
}
