using System.IO;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Export
{
	public interface IVarietyPairExporter
	{
		void Export(Stream stream, IWordAligner aligner, VarietyPair varietyPair);
	}
}
