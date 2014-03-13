using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SIL.Cog.Application.Import;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.Tests.Import
{
	[TestFixture]
	public class WordSurv7WordListsImporterTests
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
					Meanings = {new Meaning("gloss", "cat")}
				};

			// empty file
			const string file1 = "";
			importer.Import(null, new MemoryStream(Encoding.UTF8.GetBytes(file1), false), project);
			Assert.That(project.Meanings, Is.Empty);
			Assert.That(project.Varieties, Is.Empty);

			// no varieties or meanings
			const string file2 = "Survey\r\n\"Survey1\",\"Survey Metadata\"\r\n\r\n\r\n\r\n";
			importer.Import(null, new MemoryStream(Encoding.UTF8.GetBytes(file2), false), project);
			Assert.That(project.Meanings, Is.Empty);
			Assert.That(project.Varieties, Is.Empty);

			// varieties, meanings, and words
			const string file3 = "Survey\r\n\"Survey1\",\"Survey Metadata\"\r\n\r\n\r\n\r\n"
							   + "\"variety1\",\"\"\r\n\r\n\r\n"
							   + "\"gloss1\",\"word1\",\"\",\"\"\r\n"
							   + "\"gloss2\",\"word2\",\"\",\"\"\r\n"
							   + "\"\",\"word3\",\"\",\"\"\r\n"
							   + "\"gloss3\",\"word4\",\"\",\"\"\r\n"
							   + "\r\n"
							   + "\"variety2\",\"\"\r\n\r\n\r\n"
							   + "\"gloss1\",\"word5\",\"\",\"\"\r\n"
							   + "\"gloss2\",\"word6\",\"\",\"\"\r\n"
							   + "\"gloss3\",\"word7\",\"\",\"\"\r\n"
							   + "\r\n";
			importer.Import(null, new MemoryStream(Encoding.UTF8.GetBytes(file3), false), project);
			Assert.That(project.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1", "gloss2", "gloss3"}));
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));
			Assert.That(project.Varieties.SelectMany(v => v.Words).Select(w => w.StrRep), Is.EqualTo(new[] {"word1", "word2", "word3", "word4", "word5", "word6", "word7"}));
		}
	}
}
