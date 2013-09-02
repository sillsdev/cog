using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SIL.Cog.Applications.Import;
using SIL.Cog.Domain;
using SIL.Machine;

namespace SIL.Cog.Applications.Test.Import
{
	[TestFixture]
	public class WordSurv7WordListsImporterTest
	{
		private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();

		[Test]
		public void Import()
		{
			var importer = new WordSurv7WordListsImporter();

			Assert.That(importer.CreateImportSettingsViewModel(), Is.Null);

			var project = new CogProject(_spanFactory)
				{
					Varieties = {new Variety("variety")},
					Senses = {new Sense("sense", "cat")}
				};

			// empty file
			const string file1 = "";
			importer.Import(null, new MemoryStream(Encoding.UTF8.GetBytes(file1), false), project);
			Assert.That(project.Senses, Is.Empty);
			Assert.That(project.Varieties, Is.Empty);

			// no varieties or senses
			const string file2 = "Survey\r\n\"Survey1\",\"Survey Metadata\"\r\n\r\n\r\n\r\n";
			importer.Import(null, new MemoryStream(Encoding.UTF8.GetBytes(file2), false), project);
			Assert.That(project.Senses, Is.Empty);
			Assert.That(project.Varieties, Is.Empty);

			// varieties, senses, and words
			const string file3 = "Survey\r\n\"Survey1\",\"Survey Metadata\"\r\n\r\n\r\n\r\n"
							   + "\"variety1\",\"\"\r\n\r\n\r\n"
							   + "\"sense1\",\"word1\",\"\",\"\"\r\n"
							   + "\"sense2\",\"word2\",\"\",\"\"\r\n"
							   + "\"sense3\",\"word3\",\"\",\"\"\r\n"
							   + "\r\n"
							   + "\"variety2\",\"\"\r\n\r\n\r\n"
							   + "\"sense1\",\"word4\",\"\",\"\"\r\n"
							   + "\"sense2\",\"word5\",\"\",\"\"\r\n"
							   + "\"sense3\",\"word6\",\"\",\"\"\r\n"
							   + "\r\n";
			importer.Import(null, new MemoryStream(Encoding.UTF8.GetBytes(file3), false), project);
			Assert.That(project.Senses.Select(s => s.Gloss), Is.EqualTo(new[] {"sense1", "sense2", "sense3"}));
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));
			Assert.That(project.Varieties.SelectMany(v => v.Words).Select(w => w.StrRep), Is.EqualTo(new[] {"word1", "word2", "word3", "word4", "word5", "word6"}));
		}
	}
}
