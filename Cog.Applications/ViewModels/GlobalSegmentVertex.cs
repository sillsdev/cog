using System.Collections.Generic;
using System.Linq;

namespace SIL.Cog.Applications.ViewModels
{
	public abstract class GlobalSegmentVertex : GlobalCorrespondencesGraphVertex
	{
		private readonly HashSet<string> _strReps;

	    protected GlobalSegmentVertex()
		{
			_strReps = new HashSet<string>();
		}

		public override string StrRep
		{
			get { return string.Join(",", _strReps.OrderBy(s => s)); }
		}

	    public override bool IsProperty
	    {
	        get { return false; }
	    }

	    internal ISet<string> StrReps
		{
			get { return _strReps; }
		}
	}
}
