using System;
using System.Collections.Generic;

namespace SIL.Cog.Services
{
	public interface IImportService
	{
		bool ImportWordLists(object ownerViewModel, CogProject project);
		bool ImportSegmentMappings(object ownerViewModel, out IEnumerable<Tuple<string, string>> mappings);
		bool ImportGeographicRegions(object ownerViewModel, CogProject project);
	}
}
