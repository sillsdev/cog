using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Export
{
	public interface IVarietyPairExporter
	{
		void Export(string path, IWordAligner aligner, VarietyPair varietyPair);
	}
}
