using System.IO;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Export
{
	public interface ISimilarityMatrixExporter
	{
		void Export(Stream stream, CogProject project, SimilarityMetric similarityMetric);
	}
}
