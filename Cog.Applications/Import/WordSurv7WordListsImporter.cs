using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.Import
{
	public class WordSurv7WordListsImporter : IWordListsImporter
	{
		public object CreateImportSettingsViewModel()
		{
			return null;
		}

		public void Import(object importSettingsViewModel, Stream stream, CogProject project)
		{
			var reader = new CsvReader(new StreamReader(stream), ',');
			if (!SkipRows(reader, 5))
			{
				project.Senses.Clear();
				project.Varieties.Clear();
				return;
			}

			var varieties = new List<Variety>();
			var senses = new Dictionary<string, Sense>();
			IList<string> varietyRow;
			while (reader.ReadRow(out varietyRow))
			{
				if (string.IsNullOrEmpty(varietyRow[0]))
					break;

				var variety = new Variety(varietyRow[0].Trim());
				if (!SkipRows(reader, 2))
					throw new ImportException("Metadata for a variety is incomplete.");

				Sense curSense = null;
				IList<string> glossRow;
				while (reader.ReadRow(out glossRow) && glossRow.Any(s => !string.IsNullOrEmpty(s)))
				{
					if (!string.IsNullOrEmpty(glossRow[0]))
					{
						string gloss = glossRow[0].Trim();
						curSense = senses.GetValue(gloss, () => new Sense(gloss, null));
					}
					if (curSense == null)
						throw new ImportException("A gloss is missing.");

					string wordStr = glossRow[1].Trim();
					if (!string.IsNullOrEmpty(wordStr))
						variety.Words.Add(new Word(wordStr, curSense));
				}
				varieties.Add(variety);
			}

			project.Senses.ReplaceAll(senses.Values);
			project.Varieties.ReplaceAll(varieties);
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
