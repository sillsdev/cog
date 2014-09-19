using SIL.Cog.Domain;

namespace SIL.Cog.Application.Services
{
	internal interface IProjectMigration
	{
		int Version { get; }
		void Migrate(SegmentPool segmentPool, CogProject project);
	}
}
