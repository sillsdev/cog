using System.IO;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Export
{
	public interface IVarietyPairExporter
	{
		void Export(Stream stream, IWordAligner aligner, VarietyPair varietyPair);
	}
}
