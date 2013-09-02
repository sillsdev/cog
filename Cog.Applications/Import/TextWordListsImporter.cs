using System.Collections.Generic;
using System.IO;
using System.Text;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Import
{
	public class TextWordListsImporter : IWordListsImporter
	{
		private readonly char _delimiter;

		public TextWordListsImporter(char delimiter)
		{
			_delimiter = delimiter;
		}

		public object CreateImportSettingsViewModel()
		{
			return new ImportTextWordListsViewModel();
		}

		public void Import(object importSettingsViewModel, Stream stream, CogProject project)
		{
			var vm = (ImportTextWordListsViewModel) importSettingsViewModel;

			switch (vm.Format)
			{
				case TextWordListsFormat.VarietyRows:
					ImportVarietyRows(stream, project, vm.CategoriesIncluded);
					break;

				case TextWordListsFormat.SenseRows:
					ImportSenseRows(stream, project, vm.CategoriesIncluded);
					break;
			}
		}

		private void ImportVarietyRows(Stream stream, CogProject project, bool categoriesIncluded)
		{
			var reader = new CsvReader(new StreamReader(stream), _delimiter);
			IList<string> glosses;
			if (!reader.ReadRow(out glosses))
			{
				project.Senses.Clear();
				project.Varieties.Clear();
				return;
			}
			IList<string> categories = null;
			if (categoriesIncluded)
			{
				if (reader.ReadRow(out categories))
				{
					if (categories.Count <= 1)
						categories = null;
					else if (categories.Count != glosses.Count)
						throw new ImportException("A category is not specified for each gloss. Line: 2");
				}
				else
				{
					throw new ImportException("Missing categories row. Line: 2");
				}
			}

			var senses = new Dictionary<string, Sense>();
			for (int i = 1; i < glosses.Count; i++)
			{
				string gloss = glosses[i].Trim();
				if (string.IsNullOrEmpty(gloss))
					throw new ImportException("A blank gloss is not allowed. Line: 1");
				if (senses.ContainsKey(gloss))
					throw new ImportException(string.Format("The gloss, \"{0}\", is not unique. Line: 1", gloss));
				string category = null;
				if (categoriesIncluded && categories != null)
					category = categories[i].Trim();
				senses[gloss] = new Sense(gloss, string.IsNullOrEmpty(category) ? null : category);
			}

			int line = categoriesIncluded ? 3 : 2;

			var varieties = new Dictionary<string, Variety>();
			IList<string> varietyRow;
			while (reader.ReadRow(out varietyRow))
			{
				if (varietyRow.Count != glosses.Count)
					throw new ImportException(string.Format("Incorrect number of words specified for a variety. Line: {0}", line));
				string name = varietyRow[0].Trim();
				if (string.IsNullOrEmpty(name))
					throw new ImportException(string.Format("A blank variety name is not allowed. Line: {0}", line));
				if (varieties.ContainsKey(name))
					throw new ImportException(string.Format("The variety name, \"{0}\", is not unique. Line: {1}", name, line));
				var variety = new Variety(name);
				for (int j = 1; j < varietyRow.Count; j++)
				{
					string wordStr = varietyRow[j].Trim();
					if (!string.IsNullOrEmpty(wordStr))
					{
						foreach (string w in wordStr.Split(','))
						{
							string str = w.Trim().Normalize(NormalizationForm.FormD);
							variety.Words.Add(new Word(str, senses[glosses[j].Trim()]));
						}
					}
				}
				varieties[name] = variety;
				line++;
			}

			project.Senses.ReplaceAll(senses.Values);
			project.Varieties.ReplaceAll(varieties.Values);
		}

		private void ImportSenseRows(Stream stream, CogProject project, bool categoriesIncluded)
		{
			var reader = new CsvReader(new StreamReader(stream), _delimiter);

			IList<string> varietyNames;
			if (!reader.ReadRow(out varietyNames))
			{
				project.Varieties.Clear();
				project.Senses.Clear();
				return;
			}

			var varieties = new Dictionary<string, Variety>();
			for (int i = (categoriesIncluded ? 2 : 1); i < varietyNames.Count; i++)
			{
				string name = varietyNames[i].Trim();
				if (string.IsNullOrEmpty(name))
					throw new ImportException("A blank variety name is not allowed. Line: 1");
				if (varieties.ContainsKey(name))
					throw new ImportException(string.Format("The variety name, \"{0}\", is not unique. Line: 1", name));
				varieties[name] = new Variety(name);
			}

			int line = 2;
			var senses = new Dictionary<string, Sense>();
			IList<string> glossRow;
			while (reader.ReadRow(out glossRow))
			{
				if (glossRow.Count != varietyNames.Count)
					throw new ImportException(string.Format("Incorrect number of words specified for a gloss. Line: {0}", line));
				int column = 0;
				string gloss = glossRow[column++].Trim();
				if (string.IsNullOrEmpty(gloss))
					throw new ImportException(string.Format("A blank gloss is not allowed. Line: {0}", line));
				if (senses.ContainsKey(gloss))
					throw new ImportException(string.Format("The gloss, \"{0}\", is not unique. Line: {1}", gloss, line));
				string category = null;
				if (categoriesIncluded)
				{
					if (glossRow.Count == 1)
						throw new ImportException(string.Format("Missing categories column. Line: {0}", line));
					category = glossRow[column++].Trim();
				}
				var sense = new Sense(gloss, string.IsNullOrEmpty(category) ? null : category);
				senses[gloss] = sense;
				for (int j = column; j < glossRow.Count; j++)
				{
					string wordStr = glossRow[j].Trim();
					if (!string.IsNullOrEmpty(wordStr))
					{
						foreach (string w in wordStr.Split(','))
						{
							string str = w.Trim().Normalize(NormalizationForm.FormD);
							varieties[varietyNames[j].Trim()].Words.Add(new Word(str, sense));
						}
					}
				}
				line++;
			}

			project.Senses.ReplaceAll(senses.Values);
			project.Varieties.ReplaceAll(varieties.Values);
		}
	}
}
