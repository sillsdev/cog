using System.Collections.Generic;
using System.IO;
using System.Text;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Import
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

		public void Import(object importSettingsViewModel, string path, CogProject project)
		{
			var vm = (ImportTextWordListsViewModel) importSettingsViewModel;

			switch (vm.Format)
			{
				case TextWordListsFormat.VarietyRows:
					ImportVarietyRows(path, project, vm.CategoriesIncluded);
					break;

				case TextWordListsFormat.SenseRows:
					ImportSenseRows(path, project, vm.CategoriesIncluded);
					break;
			}
		}

		private void ImportVarietyRows(string path, CogProject project, bool categoriesIncluded)
		{
			using (var file = new StreamReader(path))
			{
				var reader = new CsvReader(file, _delimiter);

				IList<string> glosses;
				if (!reader.ReadRow(out glosses))
					return;
				IList<string> categories = null;
				if (categoriesIncluded)
				{
					if (!reader.ReadRow(out categories))
						return;
				}

				var senses = new Dictionary<string, Sense>();
				for (int i = 1; i < glosses.Count; i++)
				{
					string gloss = glosses[i].Trim();
					if (senses.ContainsKey(gloss))
						throw new ImportException(string.Format("The gloss, \"{0}\", is not unique.", gloss));
					string category = null;
					if (categories != null)
						category = categories[i].Trim();
					senses[gloss] = new Sense(gloss, string.IsNullOrEmpty(category) ? null : category);
				}

				var varieties = new Dictionary<string, Variety>();
				IList<string> varietyRow;
				while (reader.ReadRow(out varietyRow))
				{
					string name = varietyRow[0].Trim();
					if (varieties.ContainsKey(name))
						throw new ImportException(string.Format("The variety name, \"{0}\", is not unique.", name));
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
				}

				project.Senses.ReplaceAll(senses.Values);
				project.Varieties.ReplaceAll(varieties.Values);
			}
		}

		private void ImportSenseRows(string path, CogProject project, bool categoriesIncluded)
		{
			using (var file = new StreamReader(path))
			{
				var reader = new CsvReader(file, _delimiter);

				IList<string> varietyNames;
				if (!reader.ReadRow(out varietyNames))
					return;
				var varieties = new Dictionary<string, Variety>();
				for (int i = (categoriesIncluded ? 2 : 1); i < varietyNames.Count; i++)
				{
					string name = varietyNames[i].Trim();
					if (varieties.ContainsKey(name))
						throw new ImportException(string.Format("The variety name, \"{0}\", is not unique.", name));
					varieties[name] = new Variety(name);
				}

				var senses = new Dictionary<string, Sense>();
				IList<string> glossRow;
				while (reader.ReadRow(out glossRow))
				{
					int column = 0;
					string gloss = glossRow[column++].Trim();
					if (senses.ContainsKey(gloss))
						throw new ImportException(string.Format("The gloss, \"{0}\", is not unique.", gloss));
					string category = null;
					if (categoriesIncluded)
						category = glossRow[column++].Trim();
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
				}

				project.Senses.ReplaceAll(senses.Values);
				project.Varieties.ReplaceAll(varieties.Values);
			}
		}
	}
}
