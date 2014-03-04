using System;
using System.Collections.Generic;
using System.IO;

namespace SIL.Cog.Application.Import
{
	public class TextSegmentMappingsImporter : ISegmentMappingsImporter
	{
		private readonly char _delimiter;

		public TextSegmentMappingsImporter(char delimiter)
		{
			_delimiter = delimiter;
		}

		public object CreateImportSettingsViewModel()
		{
			return null;
		}

		public IEnumerable<Tuple<string, string>> Import(object importSettingsViewModel, Stream stream)
		{
			var mappings = new List<Tuple<string, string>>();
			var reader = new CsvReader(new StreamReader(stream), _delimiter);
			IList<string> mapping;
			while (reader.ReadRow(out mapping))
			{
				if (mapping.Count >= 2)
				{
					string str1 = mapping[0].Trim();
					string str2 = mapping[1].Trim();
					if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
						throw new ImportException("An empty segment is not allowed.");
					mappings.Add(Tuple.Create(str1, str2));
				}
			}

			return mappings;
		}
	}
}
