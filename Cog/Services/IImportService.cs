using System;
using System.Collections.Generic;

namespace SIL.Cog.Services
{
	public interface IImportService
	{
		bool ImportWordLists(object ownerViewModel, CogProject project);
		bool ImportSimilarSegments(object ownerViewModel, out IEnumerable<Tuple<string, string>> mappings);
	}
}
