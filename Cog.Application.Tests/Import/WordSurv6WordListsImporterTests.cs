using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using SIL.Cog.Application.Import;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Tests.Import
{
	[TestFixture]
	public class WordSurv6WordListsImporterTests
	{
		[Test]
		public void Import()
		{
			var importer = new WordSurv6WordListsImporter();

			Assert.That(importer.CreateImportSettingsViewModel(), Is.Null);

			var project = new CogProject()
				{
					Varieties = {new Variety("variety")},
					Meanings = {new Meaning("gloss", "cat")}
				};

			// empty file
			const string file1 = "";
			Assert.Throws<XmlException>(() => importer.Import(null, new MemoryStream(Encoding.Unicode.GetBytes(file1), false), project));

			// no meanings or varieties
			const string file2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><survey />";
			importer.Import(null, new MemoryStream(Encoding.Unicode.GetBytes(file2), false), project);
			Assert.That(project.Meanings, Is.Empty);
			Assert.That(project.Varieties, Is.Empty);

			// meanings only
			const string file3 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
							   + "<survey><glosses><gloss id=\"0\"><name>gloss1</name><part_of_speech>cat1</part_of_speech></gloss><gloss id=\"0\"><name>gloss2</name></gloss></glosses></survey>";
			importer.Import(null, new MemoryStream(Encoding.Unicode.GetBytes(file3), false), project);
			Assert.That(project.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1", "gloss2"}));
			Assert.That(project.Meanings.Select(s => s.Category), Is.EqualTo(new[] {"cat1", null}));
			Assert.That(project.Varieties, Is.Empty);

			// varieties only
			const string file4 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
							   + "<survey><word_lists><word_list id=\"0\"><name>variety1</name></word_list><word_list id=\"1\"><name>variety2</name></word_list></word_lists></survey>";
			importer.Import(null, new MemoryStream(Encoding.Unicode.GetBytes(file4), false), project);
			Assert.That(project.Meanings, Is.Empty);
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));

			// meanings, varieties, categories, and words
			const string file5 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
							   + "<survey>"
							   + "<word_lists><word_list id=\"0\"><name>variety1</name></word_list><word_list id=\"1\"><name>variety2</name></word_list></word_lists>"
							   + "<glosses>"
							   + "<gloss id=\"0\"><name>gloss1</name><part_of_speech>cat1</part_of_speech>"
							   + "<transcriptions><transcription><word_list_id>0</word_list_id><name>word1</name></transcription><transcription><word_list_id>1</word_list_id><name>word4</name></transcription></transcriptions>"
							   + "</gloss>"
							   + "<gloss id=\"1\"><name>gloss2</name><part_of_speech>cat2</part_of_speech>"
							   + "<transcriptions><transcription><word_list_id>0</word_list_id><name>word2</name></transcription><transcription><word_list_id>1</word_list_id><name>word5</name></transcription></transcriptions>"
							   + "</gloss>"
							   + "<gloss id=\"2\"><name>gloss3</name><part_of_speech>cat3</part_of_speech>"
							   + "<transcriptions><transcription><word_list_id>0</word_list_id><name>word3</name></transcription><transcription><word_list_id>1</word_list_id><name>word6</name></transcription></transcriptions>"
							   + "</gloss>"
							   + "</glosses>"
							   + "</survey>";
			importer.Import(null, new MemoryStream(Encoding.Unicode.GetBytes(file5), false), project);
			Assert.That(project.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1", "gloss2", "gloss3"}));
			Assert.That(project.Meanings.Select(s => s.Category), Is.EqualTo(new[] {"cat1", "cat2", "cat3"}));
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));
			Assert.That(project.Varieties.SelectMany(v => v.Words).Select(w => w.StrRep), Is.EqualTo(new[] {"word1", "word2", "word3", "word4", "word5", "word6"}));

			// no variety name
			const string file6 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
							   + "<survey>"
							   + "<word_lists><word_list id=\"0\"></word_list><word_list id=\"1\"><name>variety2</name></word_list></word_lists>"
							   + "<glosses>"
							   + "<gloss id=\"0\"><name>gloss1</name><part_of_speech>cat1</part_of_speech>"
							   + "<transcriptions><transcription><word_list_id>0</word_list_id><name>word1</name></transcription><transcription><word_list_id>1</word_list_id><name>word4</name></transcription></transcriptions>"
							   + "</gloss>"
							   + "<gloss id=\"1\"><name>gloss2</name><part_of_speech>cat2</part_of_speech>"
							   + "<transcriptions><transcription><word_list_id>0</word_list_id><name>word2</name></transcription><transcription><word_list_id>1</word_list_id><name>word5</name></transcription></transcriptions>"
							   + "</gloss>"
							   + "<gloss id=\"2\"><name>gloss3</name><part_of_speech>cat3</part_of_speech>"
							   + "<transcriptions><transcription><word_list_id>0</word_list_id><name>word3</name></transcription><transcription><word_list_id>1</word_list_id><name>word6</name></transcription></transcriptions>"
							   + "</gloss>"
							   + "</glosses>"
							   + "</survey>";
			Assert.Throws<ImportException>(() => importer.Import(null, new MemoryStream(Encoding.Unicode.GetBytes(file6), false), project));

			// no gloss
			const string file7 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
							   + "<survey>"
							   + "<word_lists><word_list id=\"0\"><name>variety1</name></word_list><word_list id=\"1\"><name>variety2</name></word_list></word_lists>"
							   + "<glosses>"
							   + "<gloss id=\"0\"><name>gloss1</name><part_of_speech>cat1</part_of_speech>"
							   + "<transcriptions><transcription><word_list_id>0</word_list_id><name>word1</name></transcription><transcription><word_list_id>1</word_list_id><name>word4</name></transcription></transcriptions>"
							   + "</gloss>"
							   + "<gloss id=\"1\"><part_of_speech>cat2</part_of_speech>"
							   + "<transcriptions><transcription><word_list_id>0</word_list_id><name>word2</name></transcription><transcription><word_list_id>1</word_list_id><name>word5</name></transcription></transcriptions>"
							   + "</gloss>"
							   + "<gloss id=\"2\"><name>gloss3</name><part_of_speech>cat3</part_of_speech>"
							   + "<transcriptions><transcription><word_list_id>0</word_list_id><name>word3</name></transcription><transcription><word_list_id>1</word_list_id><name>word6</name></transcription></transcriptions>"
							   + "</gloss>"
							   + "</glosses>"
							   + "</survey>";
			Assert.Throws<ImportException>(() => importer.Import(null, new MemoryStream(Encoding.Unicode.GetBytes(file7), false), project));

			// duplicate variety names
			const string file8 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
							   + "<survey>"
							   + "<word_lists><word_list id=\"0\"><name>variety1</name></word_list><word_list id=\"1\"><name>variety1</name></word_list></word_lists>"
							   + "<glosses>"
							   + "<gloss id=\"0\"><name>gloss1</name><part_of_speech>cat1</part_of_speech>"
							   + "<transcriptions><transcription><word_list_id>0</word_list_id><name>word1</name></transcription><transcription><word_list_id>1</word_list_id><name>word4</name></transcription></transcriptions>"
							   + "</gloss>"
							   + "<gloss id=\"1\"><name>gloss2</name><part_of_speech>cat2</part_of_speech>"
							   + "<transcriptions><transcription><word_list_id>0</word_list_id><name>word2</name></transcription><transcription><word_list_id>1</word_list_id><name>word5</name></transcription></transcriptions>"
							   + "</gloss>"
							   + "<gloss id=\"2\"><name>gloss3</name><part_of_speech>cat3</part_of_speech>"
							   + "<transcriptions><transcription><word_list_id>0</word_list_id><name>word3</name></transcription><transcription><word_list_id>1</word_list_id><name>word6</name></transcription></transcriptions>"
							   + "</gloss>"
							   + "</glosses>"
							   + "</survey>";
			Assert.Throws<ImportException>(() => importer.Import(null, new MemoryStream(Encoding.Unicode.GetBytes(file8), false), project));

			// duplicate glosses
			const string file9 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
							   + "<survey>"
							   + "<word_lists><word_list id=\"0\"><name>variety1</name></word_list><word_list id=\"1\"><name>variety2</name></word_list></word_lists>"
							   + "<glosses>"
							   + "<gloss id=\"0\"><name>gloss1</name><part_of_speech>cat1</part_of_speech>"
							   + "<transcriptions><transcription><word_list_id>0</word_list_id><name>word1</name></transcription><transcription><word_list_id>1</word_list_id><name>word4</name></transcription></transcriptions>"
							   + "</gloss>"
							   + "<gloss id=\"1\"><name>gloss2</name><part_of_speech>cat2</part_of_speech>"
							   + "<transcriptions><transcription><word_list_id>0</word_list_id><name>word2</name></transcription><transcription><word_list_id>1</word_list_id><name>word5</name></transcription></transcriptions>"
							   + "</gloss>"
							   + "<gloss id=\"2\"><name>gloss1</name><part_of_speech>cat3</part_of_speech>"
							   + "<transcriptions><transcription><word_list_id>0</word_list_id><name>word3</name></transcription><transcription><word_list_id>1</word_list_id><name>word6</name></transcription></transcriptions>"
							   + "</gloss>"
							   + "</glosses>"
							   + "</survey>";
			Assert.Throws<ImportException>(() => importer.Import(null, new MemoryStream(Encoding.Unicode.GetBytes(file9), false), project));
		}
	}
}
