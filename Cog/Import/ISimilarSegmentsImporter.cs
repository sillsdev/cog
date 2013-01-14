using System;
using System.Collections.Generic;

namespace SIL.Cog.Import
{
	public interface ISimilarSegmentsImporter
	{
		IEnumerable<Tuple<string, string>> Import(string path);
	}
}
