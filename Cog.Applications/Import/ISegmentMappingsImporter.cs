using System;
using System.Collections.Generic;
using System.IO;

namespace SIL.Cog.Applications.Import
{
	public interface ISegmentMappingsImporter : IImporter
	{
		IEnumerable<Tuple<string, string>> Import(object importSettingsViewModel, Stream stream);
	}
}
