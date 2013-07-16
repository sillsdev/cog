using System.Collections.Generic;
using System.Linq;
using SIL.Cog.Applications.GraphAlgorithms;

namespace SIL.Cog.Applications.ViewModels
{
	public class GlobalSegmentVertex : GridVertex
	{
		private readonly HashSet<string> _strReps; 

		public GlobalSegmentVertex()
		{
			_strReps = new HashSet<string>();
		}

		public string StrRep
		{
			get { return string.Join(",", _strReps.OrderBy(s => s)); }
		}

		internal ISet<string> StrReps
		{
			get { return _strReps; }
		}
	}
}
