using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SIL.Cog.Application.CommandLine
{
	class Error
	{
		public string Message { get; set; }
		private readonly string _source;

		public string Source
		{
			get { return _source; }
		}

		public Error(string message, params object[] formatParams)
			: this(null, message, formatParams)
		{
		}

		public Error(string source, string message, params object[] formatParams)
		{
			_source = source;
			Message = string.Format(message, formatParams);
		}

		public override string ToString()
		{
			if (_source == null)
				return Message;
			else
			{
				StringBuilder sb = new StringBuilder(Message);
				sb.AppendLine();
				sb.AppendFormat("  Above error caused by line: \"{0}\"", _source);
				return sb.ToString();
			}
		}

		// If needed, could implement ToXml(), ToJson(), or whatever
	}

	public class Errors
	{
		private List<Error> errors;

		public bool OK { get { return (errors.Count == 0); } }

		public Errors()
		{
			errors = new List<Error>();
		}

		public void Add(string message, params object[] formatParams)
		{
			Add(null, message, formatParams);
		}

		public void Add(string source, string message, params object[] formatParams)
		{
			errors.Add(new Error(source, message, formatParams));
		}

		public override string ToString()
		{
			return string.Join("\n", errors);
		}

		public void DumpToStream(TextWriter errorWriter)
		{
			foreach (Error error in errors)
			{
				errorWriter.WriteLine(error.ToString());
			}
			errorWriter.WriteLine();
		}

		public void DumpToStdErr()
		{
			DumpToStream(Console.Error);
		}
	}

}
