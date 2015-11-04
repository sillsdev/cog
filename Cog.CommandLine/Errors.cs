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
					sb.AppendFormat("  This was caused by the line: \"{0}\"", _source);
					return sb.ToString();
				}
			}

			// If needed, could implement ToXml(), ToJson(), or whatever
		}

		private List<Error> errors;

		public bool Empty { get { return (errors.Count == 0); } }
		public int Count { get { return errors.Count; } }

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
			StringBuilder result = new StringBuilder();
			foreach (Error error in errors)
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
