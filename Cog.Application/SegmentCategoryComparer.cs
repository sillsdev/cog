using System.Collections.Generic;

namespace SIL.Cog.Application
{
	public class SegmentCategoryComparer : IComparer<string>
	{
		private readonly static Dictionary<string, int> CategorySortOrderLookup = new Dictionary<string, int>
			{
				{"Close", 0},
				{"Mid", 1},
				{"Open", 2},
				{"Labial", 0},
				{"Coronal", 1},
				{"Dorsal", 2},
				{"Guttural", 3},
				{string.Empty, 99}
			};

		public int Compare(string x, string y)
		{
			int xnum = CategorySortOrderLookup[x];
			int ynum = CategorySortOrderLookup[y];
			return Comparer<int>.Default.Compare(xnum, ynum);
		}
	}
}
