using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SIL.Cog.Application.Import;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.Tests.Import
{
	[TestFixture]
	public class TextWordListsImporterTests
	{
		private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();

		[Test]
		public void Import_VarietyRows()
		{
			var importer = new TextWordListsImporter('\t');

			var settings = (ImportTextWordListsViewModel) importer.CreateImportSettingsViewModel();
			settings.Format = TextWordListsFormat.VarietyRows;
			settings.CategoriesIncluded = true;

			var project = new CogProject(_spanFactory)
				{
					Varieties = {new Variety("variety")},
					Meanings = {new Meaning("gloss", "cat")}
				};

			// empty file
			const string file1 = "";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file1), false), project);
			// no change
			Assert.That(project.Meanings, Is.Empty);
			Assert.That(project.Varieties, Is.Empty);

			// only glosses
			const string file2 = "\tgloss1\tgloss2\tgloss3\r\n";
			Assert.Throws<ImportException>(() => importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file2), false), project));

			// only glosses and categories
			const string file3 = "\tgloss1\tgloss2\tgloss3\r\n"
							   + "\tcat1\tcat2\tcat3\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file3), false), project);
			Assert.That(project.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1", "gloss2", "gloss3"}));
			Assert.That(project.Meanings.Select(s => s.Category), Is.EqualTo(new[] {"cat1", "cat2", "cat3"}));
			Assert.That(project.Varieties, Is.Empty);

			// only varieties
			const string file4 = "\r\n\r\nvariety1\r\nvariety2\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file4), false), project);
			Assert.That(project.Meanings, Is.Empty);
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));

			// glosses, categories, varieties, and words
			const string file5 = "\tgloss1\tgloss2\tgloss3\r\n"
							   + "\tcat1\tcat2\tcat3\r\n"
							   + "variety1\tword1\tword2\tword3\r\n"
							   + "variety2\tword4\tword5\tword6\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file5), false), project);
			Assert.That(project.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1", "gloss2", "gloss3"}));
			Assert.That(project.Meanings.Select(s => s.Category), Is.EqualTo(new[] {"cat1", "cat2", "cat3"}));
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));
			Assert.That(project.Varieties.SelectMany(v => v.Words).Select(w => w.StrRep), Is.EqualTo(new[] {"word1", "word2", "word3", "word4", "word5", "word6"}));

			// empty gloss
			const string file6 = "\tgloss1\t\tgloss3\r\n"
							   + "\tcat1\tcat2\tcat3\r\n"
							   + "variety1\tword1\tword2\tword3\r\n"
							   + "variety2\tword4\tword5\tword6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file6), false), project));

			// empty variety
			const string file7 = "\tgloss1\tgloss2\tgloss3\r\n"
							   + "\tcat1\tcat2\tcat3\r\n"
							   + "\tword1\tword2\tword3\r\n"
							   + "variety2\tword4\tword5\tword6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file7), false), project));

			// duplicate gloss
			const string file8 = "\tgloss1\tgloss1\tgloss3\r\n"
							   + "\tcat1\tcat2\tcat3\r\n"
							   + "variety1\tword1\tword2\tword3\r\n"
							   + "variety2\tword4\tword5\tword6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file8), false), project));

			// duplicate variety
			const string file9 = "\tgloss1\tgloss2\tgloss3\r\n"
							   + "\tcat1\tcat2\tcat3\r\n"
							   + "variety1\tword1\tword2\tword3\r\n"
							   + "variety1\tword4\tword5\tword6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file9), false), project));

			// incorrect number of words
			const string file10 = "\tgloss1\tgloss2\tgloss3\r\n"
							    + "\tcat1\tcat2\tcat3\r\n"
							    + "variety1\tword1\tword2\r\n"
							    + "variety2\tword4\tword5\tword6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file10), false), project));

			// double quotes
			const string file11 = "\tgloss1\tgloss2\tgloss3\r\n"
							    + "\tcat1\tcat2\tcat3\r\n"
							    + "\"variety1\ttest\"\tword1\tword2\tword3\r\n"
							    + "variety2\tword4\tword5\tword6\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file11), false), project);
			Assert.That(project.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1", "gloss2", "gloss3"}));
			Assert.That(project.Meanings.Select(s => s.Category), Is.EqualTo(new[] {"cat1", "cat2", "cat3"}));
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1\ttest", "variety2"}));
			Assert.That(project.Varieties.SelectMany(v => v.Words).Select(w => w.StrRep), Is.EqualTo(new[] {"word1", "word2", "word3", "word4", "word5", "word6"}));

			// extra tabs at the end of each line
			const string file12 = "\tgloss1\tgloss2\tgloss3\t\t\t\r\n"
							   + "\tcat1\tcat2\tcat3\t\t\t\r\n"
							   + "variety1\tword1\tword2\tword3\t\t\t\r\n"
							   + "variety2\tword4\tword5\tword6\t\t\t\r\n"
							   + "\t\t\t\t\t\t\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file12), false), project);
			Assert.That(project.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1", "gloss2", "gloss3"}));
			Assert.That(project.Meanings.Select(s => s.Category), Is.EqualTo(new[] {"cat1", "cat2", "cat3"}));
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));
			Assert.That(project.Varieties.SelectMany(v => v.Words).Select(w => w.StrRep), Is.EqualTo(new[] {"word1", "word2", "word3", "word4", "word5", "word6"}));
		}

		[Test]
		public void Import_GlossRows()
		{
			var importer = new TextWordListsImporter(',');

			var settings = (ImportTextWordListsViewModel) importer.CreateImportSettingsViewModel();
			settings.Format = TextWordListsFormat.GlossRows;
			settings.CategoriesIncluded = true;

			var project = new CogProject(_spanFactory)
				{
					Varieties = {new Variety("variety")},
					Meanings = {new Meaning("gloss", "cat")}
				};

			// empty file
			const string file1 = "";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file1), false), project);
			// no change
			Assert.That(project.Meanings, Is.Empty);
			Assert.That(project.Varieties, Is.Empty);

			// only varieties
			const string file2 = ",,variety1,variety2\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file2), false), project);
			Assert.That(project.Meanings, Is.Empty);
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));

			// only glosses
			const string file3 = "\r\ngloss1\r\ngloss2\r\ngloss3\r\n";
			Assert.Throws<ImportException>(() => importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file3), false), project));

			// only glosses and categories
			const string file4 = ",\r\ngloss1,cat1\r\ngloss2,cat2\r\ngloss3,cat3\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file4), false), project);
			Assert.That(project.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1", "gloss2", "gloss3"}));
			Assert.That(project.Varieties, Is.Empty);

			// glosses, categories, varieties, and words
			const string file5 = ",,variety1,variety2\r\n"
							   + "gloss1,cat1,word1,word4\r\n"
							   + "gloss2,cat2,word2,word5\r\n"
							   + "gloss3,cat3,word3,word6\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file5), false), project);
			Assert.That(project.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1", "gloss2", "gloss3"}));
			Assert.That(project.Meanings.Select(s => s.Category), Is.EqualTo(new[] {"cat1", "cat2", "cat3"}));
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));
			Assert.That(project.Varieties.SelectMany(v => v.Words).Select(w => w.StrRep), Is.EqualTo(new[] {"word1", "word2", "word3", "word4", "word5", "word6"}));

			// empty gloss
			const string file6 = ",,variety1,variety2\r\n"
							   + "gloss1,cat1,word1,word4\r\n"
							   + ",cat2,word2,word5\r\n"
							   + "gloss3,cat3,word3,word6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file6), false), project));

			// empty variety
			const string file7 = ",,,variety2\r\n"
							   + "gloss1,cat1,word1,word4\r\n"
							   + "gloss2,cat2,word2,word5\r\n"
							   + "gloss3,cat3,word3,word6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file7), false), project));

			// duplicate gloss
			const string file8 = ",,variety1,variety2\r\n"
							   + "gloss1,cat1,word1,word4\r\n"
							   + "gloss2,cat2,word2,word5\r\n"
							   + "gloss1,cat3,word3,word6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file8), false), project));

			// duplicate variety
			const string file9 = ",,variety1,variety1\r\n"
							   + "gloss1,cat1,word1,word4\r\n"
							   + "gloss2,cat2,word2,word5\r\n"
							   + "gloss3,cat3,word3,word6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file9), false), project));

			// incorrect number of words
			const string file10 = ",,variety1,variety2\r\n"
							    + "gloss1,cat1,word1,word4\r\n"
							    + "gloss2,cat2,word2,word5\r\n"
							    + "gloss3,cat3,word3\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file10), false), project));

			// double quotes
			const string file11 = ",,variety1,variety2\r\n"
							    + "gloss1,cat1,word1,word4\r\n"
							    + "\"gloss2,test\",cat2,word2,word5\r\n"
							    + "gloss3,cat3,word3,word6\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file11), false), project);
			Assert.That(project.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1", "gloss2,test", "gloss3"}));
			Assert.That(project.Meanings.Select(s => s.Category), Is.EqualTo(new[] {"cat1", "cat2", "cat3"}));
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));
			Assert.That(project.Varieties.SelectMany(v => v.Words).Select(w => w.StrRep), Is.EqualTo(new[] {"word1", "word2", "word3", "word4", "word5", "word6"}));

			// extra tabs at the end of each line
			const string file12 = ",,variety1,variety2,,,\r\n"
							   + "gloss1,cat1,word1,word4,,,\r\n"
							   + "gloss2,cat2,word2,word5,,,\r\n"
							   + "gloss3,cat3,word3,word6,,,\r\n"
							   + ",,,,,,\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file12), false), project);
			Assert.That(project.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1", "gloss2", "gloss3"}));
			Assert.That(project.Meanings.Select(s => s.Category), Is.EqualTo(new[] {"cat1", "cat2", "cat3"}));
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));
			Assert.That(project.Varieties.SelectMany(v => v.Words).Select(w => w.StrRep), Is.EqualTo(new[] {"word1", "word2", "word3", "word4", "word5", "word6"}));
		}
	}
}
