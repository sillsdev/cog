using System;
using System.Collections.Generic;

namespace SIL.Cog.Application.Services
{
	public interface IImportService
	{
		bool ImportWordListsFromFile(object ownerViewModel);
		bool CanImportWordListsFromClipboard();
		bool ImportWordListsFromClipboard(object ownerViewModel);
		bool ImportSegmentMappings(object ownerViewModel, out IEnumerable<Tuple<string, string>> mappings);
		bool ImportGeographicRegions(object ownerViewModel);
	}
}
