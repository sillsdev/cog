using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using SIL.Cog.Domain.Config.Migrations;

namespace SIL.Cog.Domain.Tests.Config.Migrations
{
	[TestFixture]
	public class ConfigMigration1_3Tests
	{
		[Test]
		public void Migrate()
		{
			XDocument doc = XDocument.Parse(@"<?xml version='1.0' encoding='utf-8'?>
<CogProject xmlns='http://www.sil.org/CogProject/1.2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' version='4'>
	<Meanings>
		<Meaning id='meaning1' gloss='to be afraid' />
		<Meaning id='meaning2' gloss='flesh' />
		<Meaning id='meaning3' gloss='ashes' />
	</Meanings>
	<Varieties>
		<Variety name='Variety1'>
			<Words>
				<Word meaning='meaning1'>word1</Word>
				<Word meaning='meaning2'>word2</Word>
				<Word meaning='meaning3'>word3</Word>
			</Words>
		</Variety>
	</Varieties>
</CogProject>");

			var cm = new ConfigMigration1_3();
			cm.Migrate(doc);

			XElement root = doc.Root;
			Assert.That(root, Is.Not.Null);
			XNamespace toNamespace = "http://www.sil.org/CogProject/1.3";
			Assert.That(root.GetDefaultNamespace(), Is.EqualTo(toNamespace));
			Assert.That(root.Elements(toNamespace + "Meanings").Elements(toNamespace + "Meaning").All(e => e.Attribute("id") == null), Is.True);
			XElement varietiesElem = root.Element(toNamespace + "Varieties");
			Assert.That(varietiesElem, Is.Not.Null);
			XElement[] wordElems = varietiesElem.Elements(toNamespace + "Variety").Elements(toNamespace + "Words").Elements(toNamespace + "Word").ToArray();
			Assert.That((string) wordElems[0].Attribute("meaning"), Is.EqualTo("to be afraid"));
			Assert.That((string) wordElems[1].Attribute("meaning"), Is.EqualTo("flesh"));
			Assert.That((string) wordElems[2].Attribute("meaning"), Is.EqualTo("ashes"));

			Assert.That(((XElement) varietiesElem.NextNode).Name, Is.EqualTo(toNamespace + "CognacyDecisions"));
		}
	}
}
