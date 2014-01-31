using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SIL.Cog.Applications.Import;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;

namespace SIL.Cog.Applications.Test.Import
{
	[TestFixture]
	public class TextWordListsImporterTest
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
					Senses = {new Sense("sense", "cat")}
				};

			// empty file
			const string file1 = "";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file1), false), project);
			// no change
			Assert.That(project.Senses, Is.Empty);
			Assert.That(project.Varieties, Is.Empty);

			// only senses
			const string file2 = "\tsense1\tsense2\tsense3\r\n";
			Assert.Throws<ImportException>(() => importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file2), false), project));

			// only senses and categories
			const string file3 = "\tsense1\tsense2\tsense3\r\n"
							   + "\tcat1\tcat2\tcat3\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file3), false), project);
			Assert.That(project.Senses.Select(s => s.Gloss), Is.EqualTo(new[] {"sense1", "sense2", "sense3"}));
			Assert.That(project.Senses.Select(s => s.Category), Is.EqualTo(new[] {"cat1", "cat2", "cat3"}));
			Assert.That(project.Varieties, Is.Empty);

			// only varieties
			const string file4 = "\r\n\r\nvariety1\r\nvariety2\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file4), false), project);
			Assert.That(project.Senses, Is.Empty);
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));

			// senses, categories, varieties, and words
			const string file5 = "\tsense1\tsense2\tsense3\r\n"
							   + "\tcat1\tcat2\tcat3\r\n"
							   + "variety1\tword1\tword2\tword3\r\n"
							   + "variety2\tword4\tword5\tword6\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file5), false), project);
			Assert.That(project.Senses.Select(s => s.Gloss), Is.EqualTo(new[] {"sense1", "sense2", "sense3"}));
			Assert.That(project.Senses.Select(s => s.Category), Is.EqualTo(new[] {"cat1", "cat2", "cat3"}));
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));
			Assert.That(project.Varieties.SelectMany(v => v.Words).Select(w => w.StrRep), Is.EqualTo(new[] {"word1", "word2", "word3", "word4", "word5", "word6"}));

			// empty sense
			const string file6 = "\tsense1\t\tsense3\r\n"
							   + "\tcat1\tcat2\tcat3\r\n"
							   + "variety1\tword1\tword2\tword3\r\n"
							   + "variety2\tword4\tword5\tword6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file6), false), project));

			// empty variety
			const string file7 = "\tsense1\tsense2\tsense3\r\n"
							   + "\tcat1\tcat2\tcat3\r\n"
							   + "\tword1\tword2\tword3\r\n"
							   + "variety2\tword4\tword5\tword6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file7), false), project));

			// duplicate sense
			const string file8 = "\tsense1\tsense1\tsense3\r\n"
							   + "\tcat1\tcat2\tcat3\r\n"
							   + "variety1\tword1\tword2\tword3\r\n"
							   + "variety2\tword4\tword5\tword6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file8), false), project));

			// duplicate variety
			const string file9 = "\tsense1\tsense2\tsense3\r\n"
							   + "\tcat1\tcat2\tcat3\r\n"
							   + "variety1\tword1\tword2\tword3\r\n"
							   + "variety1\tword4\tword5\tword6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file9), false), project));

			// incorrect number of words
			const string file10 = "\tsense1\tsense2\tsense3\r\n"
							    + "\tcat1\tcat2\tcat3\r\n"
							    + "variety1\tword1\tword2\r\n"
							    + "variety2\tword4\tword5\tword6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file10), false), project));

			// double quotes
			const string file11 = "\tsense1\tsense2\tsense3\r\n"
							    + "\tcat1\tcat2\tcat3\r\n"
							    + "\"variety1\ttest\"\tword1\tword2\tword3\r\n"
							    + "variety2\tword4\tword5\tword6\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file11), false), project);
			Assert.That(project.Senses.Select(s => s.Gloss), Is.EqualTo(new[] {"sense1", "sense2", "sense3"}));
			Assert.That(project.Senses.Select(s => s.Category), Is.EqualTo(new[] {"cat1", "cat2", "cat3"}));
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1\ttest", "variety2"}));
			Assert.That(project.Varieties.SelectMany(v => v.Words).Select(w => w.StrRep), Is.EqualTo(new[] {"word1", "word2", "word3", "word4", "word5", "word6"}));

			// extra tab at the end of each line
			const string file12 = "\tsense1\tsense2\tsense3\t\r\n"
							   + "\tcat1\tcat2\tcat3\t\r\n"
							   + "variety1\tword1\tword2\tword3\t\r\n"
							   + "variety2\tword4\tword5\tword6\t\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file12), false), project);
			Assert.That(project.Senses.Select(s => s.Gloss), Is.EqualTo(new[] {"sense1", "sense2", "sense3"}));
			Assert.That(project.Senses.Select(s => s.Category), Is.EqualTo(new[] {"cat1", "cat2", "cat3"}));
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));
			Assert.That(project.Varieties.SelectMany(v => v.Words).Select(w => w.StrRep), Is.EqualTo(new[] {"word1", "word2", "word3", "word4", "word5", "word6"}));
		}

		[Test]
		public void Import_SenseRows()
		{
			var importer = new TextWordListsImporter(',');

			var settings = (ImportTextWordListsViewModel) importer.CreateImportSettingsViewModel();
			settings.Format = TextWordListsFormat.SenseRows;
			settings.CategoriesIncluded = true;

			var project = new CogProject(_spanFactory)
				{
					Varieties = {new Variety("variety")},
					Senses = {new Sense("sense", "cat")}
				};

			// empty file
			const string file1 = "";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file1), false), project);
			// no change
			Assert.That(project.Senses, Is.Empty);
			Assert.That(project.Varieties, Is.Empty);

			// only varieties
			const string file2 = ",,variety1,variety2\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file2), false), project);
			Assert.That(project.Senses, Is.Empty);
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));

			// only senses
			const string file3 = "\r\nsense1\r\nsense2\r\nsense3\r\n";
			Assert.Throws<ImportException>(() => importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file3), false), project));

			// only senses and categories
			const string file4 = ",\r\nsense1,cat1\r\nsense2,cat2\r\nsense3,cat3\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file4), false), project);
			Assert.That(project.Senses.Select(s => s.Gloss), Is.EqualTo(new[] {"sense1", "sense2", "sense3"}));
			Assert.That(project.Varieties, Is.Empty);

			// senses, categories, varieties, and words
			const string file5 = ",,variety1,variety2\r\n"
							   + "sense1,cat1,word1,word4\r\n"
							   + "sense2,cat2,word2,word5\r\n"
							   + "sense3,cat3,word3,word6\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file5), false), project);
			Assert.That(project.Senses.Select(s => s.Gloss), Is.EqualTo(new[] {"sense1", "sense2", "sense3"}));
			Assert.That(project.Senses.Select(s => s.Category), Is.EqualTo(new[] {"cat1", "cat2", "cat3"}));
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));
			Assert.That(project.Varieties.SelectMany(v => v.Words).Select(w => w.StrRep), Is.EqualTo(new[] {"word1", "word2", "word3", "word4", "word5", "word6"}));

			// empty sense
			const string file6 = ",,variety1,variety2\r\n"
							   + "sense1,cat1,word1,word4\r\n"
							   + ",cat2,word2,word5\r\n"
							   + "sense3,cat3,word3,word6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file6), false), project));

			// empty variety
			const string file7 = ",,,variety2\r\n"
							   + "sense1,cat1,word1,word4\r\n"
							   + "sense2,cat2,word2,word5\r\n"
							   + "sense3,cat3,word3,word6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file7), false), project));

			// duplicate sense
			const string file8 = ",,variety1,variety2\r\n"
							   + "sense1,cat1,word1,word4\r\n"
							   + "sense2,cat2,word2,word5\r\n"
							   + "sense1,cat3,word3,word6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file8), false), project));

			// duplicate variety
			const string file9 = ",,variety1,variety1\r\n"
							   + "sense1,cat1,word1,word4\r\n"
							   + "sense2,cat2,word2,word5\r\n"
							   + "sense3,cat3,word3,word6\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file9), false), project));

			// incorrect number of words
			const string file10 = ",,variety1,variety2\r\n"
							    + "sense1,cat1,word1,word4\r\n"
							    + "sense2,cat2,word2,word5\r\n"
							    + "sense3,cat3,word3\r\n";
			Assert.Throws<ImportException>(() =>  importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file10), false), project));

			// double quotes
			const string file11 = ",,variety1,variety2\r\n"
							    + "sense1,cat1,word1,word4\r\n"
							    + "\"sense2,test\",cat2,word2,word5\r\n"
							    + "sense3,cat3,word3,word6\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file11), false), project);
			Assert.That(project.Senses.Select(s => s.Gloss), Is.EqualTo(new[] {"sense1", "sense2,test", "sense3"}));
			Assert.That(project.Senses.Select(s => s.Category), Is.EqualTo(new[] {"cat1", "cat2", "cat3"}));
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));
			Assert.That(project.Varieties.SelectMany(v => v.Words).Select(w => w.StrRep), Is.EqualTo(new[] {"word1", "word2", "word3", "word4", "word5", "word6"}));

			// extra tab at the end of each line
			const string file12 = ",,variety1,variety2\t\r\n"
							   + "sense1,cat1,word1,word4\t\r\n"
							   + "sense2,cat2,word2,word5\t\r\n"
							   + "sense3,cat3,word3,word6\t\r\n";
			importer.Import(settings, new MemoryStream(Encoding.UTF8.GetBytes(file12), false), project);
			Assert.That(project.Senses.Select(s => s.Gloss), Is.EqualTo(new[] {"sense1", "sense2", "sense3"}));
			Assert.That(project.Senses.Select(s => s.Category), Is.EqualTo(new[] {"cat1", "cat2", "cat3"}));
			Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2"}));
			Assert.That(project.Varieties.SelectMany(v => v.Words).Select(w => w.StrRep), Is.EqualTo(new[] {"word1", "word2", "word3", "word4", "word5", "word6"}));
		}
	}
}
