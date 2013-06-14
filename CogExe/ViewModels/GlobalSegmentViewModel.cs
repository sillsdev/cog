using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public abstract class GlobalSegmentViewModel : ViewModelBase
	{
		private readonly HashSet<string> _strReps; 

		protected GlobalSegmentViewModel()
		{
			_strReps = new HashSet<string>();
		}

		public string StrRep
		{
			get { return string.Join(",", _strReps.OrderBy(s => s)); }
		}

		public ISet<string> StrReps
		{
			get { return _strReps; }
		}
	}
}
