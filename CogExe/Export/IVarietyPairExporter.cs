namespace SIL.Cog.Export
{
	public interface IVarietyPairExporter
	{
		void Export(string path, CogProject project, VarietyPair varietyPair);
	}
}
