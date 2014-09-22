using SIL.Cog.Domain;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Application.Services.ProjectMigrations
{
	/// <summary>
	/// This migration adds tone numbers and alveolo-palatal consonants.
	/// </summary>
	internal class ProjectMigration3 : IProjectMigration
	{
		public int Version
		{
			get { return 3; }
		}

		public void Migrate(SegmentPool segmentPool, CogProject project)
		{
			AddToneNumber(project, "¹");
			AddToneNumber(project, "²");
			AddToneNumber(project, "³");
			AddToneNumber(project, "⁴");
			AddToneNumber(project, "⁵");

			FeatureSymbol alvPal;
			if (project.FeatureSystem.TryGetSymbol("alveolo-palatal", out alvPal))
			{
				project.Segmenter.Consonants.AddSymbolBasedOn("ȶ", "t", alvPal);
				project.Segmenter.Consonants.AddSymbolBasedOn("ȡ", "d", alvPal);
				project.Segmenter.Consonants.AddSymbolBasedOn("ȵ", "n", alvPal);
				project.Segmenter.Consonants.AddSymbolBasedOn("ȴ", "l", alvPal);
			}
		}

		private static void AddToneNumber(CogProject project, string strRep)
		{
			if (!project.Segmenter.Modifiers.Contains(strRep))
				project.Segmenter.Modifiers.Add(strRep);
		}
	}
}
