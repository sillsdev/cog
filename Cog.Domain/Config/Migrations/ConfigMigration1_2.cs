using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace SIL.Cog.Domain.Config.Migrations
{
	internal class ConfigMigration1_2 : IConfigMigration
	{
		public XNamespace FromNamespace
		{
			get { return "http://www.sil.org/CogProject/1.0"; }
		}

		public void Migrate(XDocument doc)
		{
			XElement root = doc.Root;
			Debug.Assert(root != null);

			IEnumerable<XElement> elems = root.Elements(FromNamespace + "VarietyPairProcessors").Elements(FromNamespace + "VarietyPairProcessor")
				.Where(e => (string) e.Attribute(ConfigManager.Xsi + "type") == "CognicityWordPairGenerator");
			foreach (XElement elem in elems)
				elem.SetAttributeValue(ConfigManager.Xsi + "type", "CognacyWordPairGenerator");

			root.SetDefaultNamespace("http://www.sil.org/CogProject/1.2");
		}
	}
}
