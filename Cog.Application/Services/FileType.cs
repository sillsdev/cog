using System;
using System.Collections.Generic;

namespace SIL.Cog.Application.Services
{
	public class FileType
	{
		public FileType(string description, params string[] fileExtensions)
		{
			if (string.IsNullOrEmpty(description))
				throw new ArgumentException("The description must not be null or empty.", nameof(description));
			if (fileExtensions.Length == 0)
				throw new ArgumentException("A file extension must be specified.", nameof(fileExtensions));

			Description = description;
			foreach (string ext in fileExtensions)
			{
				if (string.IsNullOrEmpty(ext))
					throw new ArgumentException("A file extension cannot be empty.", nameof(fileExtensions));
				if (ext[0] != '.')
					throw new ArgumentException("A file extension must start with the '.' character.", nameof(fileExtensions));
			}
			FileExtensions = fileExtensions;
		}

		public string Description { get; }
		public IEnumerable<string> FileExtensions { get; }
	}
}
