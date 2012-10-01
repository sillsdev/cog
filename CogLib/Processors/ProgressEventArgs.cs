using System;

namespace SIL.Cog.Processors
{
	public class ProgressEventArgs : EventArgs
	{
		private readonly int _percentCompleted;

		public ProgressEventArgs(int percentCompleted)
		{
			_percentCompleted = percentCompleted;
		}

		public int PercentCompleted
		{
			get { return _percentCompleted; }
		}
	}
}
