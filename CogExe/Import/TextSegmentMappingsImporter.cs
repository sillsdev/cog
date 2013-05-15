using System;
using System.Collections.Generic;
using System.IO;

namespace SIL.Cog.Import
{
	public class TextSegmentMappingsImporter : ISegmentMappingsImporter
	{
		public IEnumerable<Tuple<string, string>> Import(string path)
		{
			var mappings = new List<Tuple<string, string>>();
			using (var file = new StreamReader(path))
			{
				string line;
				while ((line = file.ReadLine()) != null)
				{
					line = line.Trim();
					if (string.IsNullOrEmpty(line))
						continue;

					string[] mapping = line.Split('\t');
					if (mapping.Length >= 2)
					{
						mappings.Add(Tuple.Create(mapping[0].Trim(), mapping[1].Trim()));
					}
				}
			}

			return mappings;
		}
	}
}
