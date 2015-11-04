using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using SIL.Cog.Domain.Config;
using SIL.Cog.Domain.Config.Migrations;

namespace SIL.Cog.Domain.Tests.Config.Migrations
{
	[TestFixture]
	public class ConfigMigration1_2Tests
	{
		[Test]
		public void Migrate()
		{
			XDocument doc = XDocument.Parse(@"<?xml version='1.0' encoding='utf-8'?>
<CogProject xmlns='http://www.sil.org/CogProject/1.0' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' version='3'>
	<VarietyPairProcessors>
		<VarietyPairProcessor xsi:type='CognicityWordPairGenerator' id='wordPairGenerator'>
			<InitialAlignmentThreshold>0.7</InitialAlignmentThreshold>
			<ApplicableWordAligner ref='primary' />
			<ApplicableCognateIdentifier ref='primary' />
		</VarietyPairProcessor>
	</VarietyPairProcessors>
</CogProject>");

			var cm = new ConfigMigration1_2();
			cm.Migrate(doc);

			XElement root = doc.Root;
			Debug.Assert(root != null);
			Assert.That(root.GetDefaultNamespace(), Is.EqualTo((XNamespace) "http://www.sil.org/CogProject/1.2"));
			XNamespace toNamespace = "http://www.sil.org/CogProject/1.2";
			XElement wordPairGeneratorElem = root.Elements(toNamespace + "VarietyPairProcessors").Elements(toNamespace + "VarietyPairProcessor")
				.Single(e => (string) e.Attribute("id") == "wordPairGenerator");
			Assert.That((string) wordPairGeneratorElem.Attribute(ConfigManager.Xsi + "type"), Is.EqualTo("CognacyWordPairGenerator"));
		}
	}
}
