using System.IO;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Export
{
	public interface ISimilarityMatrixExporter
	{
		void Export(Stream stream, CogProject project, SimilarityMetric similarityMetric);
	}
}
