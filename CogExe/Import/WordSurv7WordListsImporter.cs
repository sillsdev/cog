using System.Collections.Generic;
using System.IO;
using System.Text;
using SIL.Collections;

namespace SIL.Cog.Import
{
	public class WordSurv7WordListsImporter : IWordListsImporter
	{
		public object CreateImportSettingsViewModel()
		{
			return null;
		}

		public void Import(object importSettingsViewModel, string path, CogProject project)
		{
			using (var file = new StreamReader(path))
			{
				var reader = new CsvReader(file, ',');
				if (!SkipRows(reader, 5))
					return;

				var varieties = new List<Variety>();
				var senses = new Dictionary<string, Sense>();
				IList<string> varietyRow;
				while (reader.ReadRow(out varietyRow))
				{
					if (string.IsNullOrEmpty(varietyRow[0]))
						break;

					var variety = new Variety(varietyRow[0].Trim());
					if (!SkipRows(reader, 2))
						return;

					IList<string> glossRow;
					while (reader.ReadRow(out glossRow) && !string.IsNullOrEmpty(glossRow[0]))
					{
						string gloss = glossRow[0].Trim();
						Sense sense = senses.GetValue(gloss, () => new Sense(gloss, null));
						string wordStr = glossRow[1].Trim().Normalize(NormalizationForm.FormD);
						if (!string.IsNullOrEmpty(wordStr))
							variety.Words.Add(new Word(wordStr, sense));
					}
					varieties.Add(variety);
				}

				project.Senses.ReplaceAll(senses.Values);
				project.Varieties.ReplaceAll(varieties);
			}
		}

		private bool SkipRows(CsvReader reader, int numRows)
		{
			IList<string> row;
			int i = 0;
			while (reader.ReadRow(out row))
			{
				i++;
				if (i == numRows)
					break;
			}

			return i == numRows;
		}
	}
}
