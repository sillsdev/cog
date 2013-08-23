using System;
using System.Collections.Generic;

namespace SIL.Cog.Applications.Services
{
	public interface IImportService
	{
		bool ImportWordLists(object ownerViewModel);
		bool ImportSegmentMappings(object ownerViewModel, out IEnumerable<Tuple<string, string>> mappings);
		bool ImportGeographicRegions(object ownerViewModel);
	}
}
