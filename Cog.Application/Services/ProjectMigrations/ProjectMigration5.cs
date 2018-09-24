using SIL.Cog.Domain;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Application.Services.ProjectMigrations
{
	public class ProjectMigration5 : IProjectMigration
	{
		public int Version => 5;

		public void Migrate(SegmentPool segmentPool, CogProject project)
		{
			Symbol symbol;
			if (project.Segmenter.Consonants.TryGetValue("ħ", out symbol))
			{
				FeatureStruct fs = symbol.FeatureStruct.DeepClone();
				fs.PriorityUnion(FeatureStruct.New(project.FeatureSystem)
					.Symbol("pharyngeal")
					.Symbol("fricative").Value);
				fs.Freeze();
				project.Segmenter.Consonants.Remove(symbol.StrRep);
				project.Segmenter.Consonants.Add(symbol.StrRep, fs, symbol.Overwrite);
			}
		}
	}
}
