using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SIL.Cog.CommandLine
{
	public class Errors
	{
		public class Error
		{
			public Error(string message, params object[] formatParams)
				: this(null, message, formatParams)
			{
			}

			public Error(string source, string message, params object[] formatParams)
			{
				Source = source;
				Message = string.Format(message, formatParams);
			}

			public string Message { get; set; }

			public string Source { get; }

			public override string ToString()
			{
				if (Source == null)
					return Message;

				StringBuilder sb = new StringBuilder(Message);
				sb.AppendLine();
				sb.Append($"  This was caused by the line: \"{Source}\"");
				return sb.ToString();
			}

			// If needed, could implement ToXml(), ToJson(), or whatever
		}

		private readonly List<Error> _errors;

		public bool Empty => _errors.Count == 0;
		public int Count => _errors.Count;

		public Errors()
		{
			_errors = new List<Error>();
		}

		public void Add(string message, params object[] formatParams)
		{
			Add(null, message, formatParams);
		}

		public void Add(string source, string message, params object[] formatParams)
		{
			_errors.Add(new Error(source, message, formatParams));
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();
			foreach (Error error in _errors)
			{
				result.AppendLine(error.ToString());
			}
			return result.ToString();
		}

		public void Write(TextWriter errorWriter)
		{
			errorWriter.Write(ToString());
		}

		public void WriteToStdErr()
		{
			Write(Console.Error);
		}
	}

}
