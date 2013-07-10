using System;
using System.Collections.Generic;

namespace SIL.Cog.Import
{
	public interface ISegmentMappingsImporter : IImporter
	{
		IEnumerable<Tuple<string, string>> Import(object importSettingsViewModel, string path);
	}
}
