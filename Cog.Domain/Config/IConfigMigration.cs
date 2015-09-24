using System.Xml.Linq;

namespace SIL.Cog.Domain.Config
{
	internal interface IConfigMigration
	{
		XNamespace FromNamespace { get; }
		void Migrate(XDocument doc);
	}
}
