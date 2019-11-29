using System.Collections.Generic;

namespace SIL.Cog.Explorer.Models
{
	public class DataGridSelectParams : DataGridEditorParams
	{
		public List<string> Values { get; set; } = new List<string>();
	}
}
