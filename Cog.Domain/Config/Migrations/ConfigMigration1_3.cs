using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace SIL.Cog.Domain.Config.Migrations
{
	internal class ConfigMigration1_3 : IConfigMigration
	{
		public XNamespace FromNamespace
		{
			get { return "http://www.sil.org/CogProject/1.2"; }
		}

		public void Migrate(XDocument doc)
		{
			XElement root = doc.Root;
			Debug.Assert(root != null);

			var idToGlossMapping = new Dictionary<string, string>();
			foreach (XElement meaningElem in root.Elements(FromNamespace + "Meanings").Elements(FromNamespace + "Meaning"))
			{
				idToGlossMapping[(string) meaningElem.Attribute("id")] = (string) meaningElem.Attribute("gloss");
				meaningElem.Attribute("id").Remove();
			}

			XElement varietiesElem = root.Element(FromNamespace + "Varieties");
			Debug.Assert(varietiesElem != null);
			foreach (XElement wordElem in varietiesElem.Elements(FromNamespace + "Variety").Elements(FromNamespace + "Words").Elements(FromNamespace + "Word"))
			{
				XAttribute meaningAttr = wordElem.Attribute("meaning");
				meaningAttr.Value = idToGlossMapping[(string) meaningAttr];
			}

			XNamespace toNamespace = "http://www.sil.org/CogProject/1.3";
			varietiesElem.AddAfterSelf(new XElement(toNamespace + "CognacyDecisions"));

			root.SetDefaultNamespace(toNamespace);
		}
	}
}
