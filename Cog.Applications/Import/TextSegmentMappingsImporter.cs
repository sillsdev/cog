using System;
using System.Collections.Generic;
using System.IO;

namespace SIL.Cog.Applications.Import
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

		public IEnumerable<Tuple<string, string>> Import(object importSettingsViewModel, string path)
		{
			var mappings = new List<Tuple<string, string>>();
			using (var file = new StreamReader(path))
			{
				var reader = new CsvReader(file, _delimiter);
				IList<string> mapping;
				while (reader.ReadRow(out mapping))
				{
					if (mapping.Count >= 2)
					{
						mappings.Add(Tuple.Create(mapping[0].Trim(), mapping[1].Trim()));
					}
				}
			}

			return mappings;
		}
	}
}
