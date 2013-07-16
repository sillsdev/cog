using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Export
{
	public interface IVarietyPairExporter
	{
		void Export(string path, CogProject project, VarietyPair varietyPair);
	}
}
