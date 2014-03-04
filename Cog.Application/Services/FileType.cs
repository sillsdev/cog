using System;
using System.Collections.Generic;

namespace SIL.Cog.Application.Services
{
	public class FileType
	{
        private readonly string _description;
        private readonly List<string> _fileExtensions;

        public FileType(string description, params string[] fileExtensions)
        {
            if (string.IsNullOrEmpty(description))
				throw new ArgumentException("The description must not be null or empty.", "description");
            if (fileExtensions.Length == 0)
				throw new ArgumentException("A file extension must be specified.", "fileExtensions");

            _description = description;
			_fileExtensions = new List<string>();
			foreach (string ext in fileExtensions)
			{
				if (string.IsNullOrEmpty(ext))
					throw new ArgumentException("A file extension cannot be empty.", "fileExtensions");
				if (ext[0] != '.')
					throw new ArgumentException("A file extension must start with the '.' character.", "fileExtensions");
				_fileExtensions.Add(ext);
			}
        }

		public string Description
		{
			get { return _description; }
		}

		public IEnumerable<string> FileExtensions
		{
			get { return _fileExtensions; }
		}
	}
}
