using System.Xml.Linq;

namespace SIL.Cog.Domain.Config.Migrations
{
	internal static class ConfigMigrationExtensions
	{
		public static void SetDefaultNamespace(this XElement elem, XNamespace ns)
		{
			elem.Name = ns + elem.Name.LocalName;
			if (elem.Attribute("xmlns") != null)
				elem.SetAttributeValue("xmlns", ns);
			foreach (XElement child in elem.Elements())
				child.SetDefaultNamespace(ns);
		}
	}
}
