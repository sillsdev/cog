using System;

namespace SIL.Cog.Applications.Import
{
	public class ImportException : Exception
	{
		public ImportException(string message)
			: base(message)
		{
		}

		public ImportException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
