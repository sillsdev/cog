using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Import
{
	public class WordSurv6WordListsImporter : IWordListsImporter
	{
		public object CreateImportSettingsViewModel()
		{
			return null;
		}

		public void Import(object importSettingsViewModel, string path, CogProject project)
		{
			XElement root = XElement.Load(path);
			var varieties = new Dictionary<string, Tuple<Variety, List<Word>>>();
			foreach (XElement wordListElem in root.Elements("word_lists").Elements("word_list"))
			{
				var id = (string) wordListElem.Attribute("id");
				var name = (string) wordListElem.Element("name");
				if (varieties.ContainsKey(name))
					throw new ImportException(string.Format("The variety name, \"{0}\", is not unique.", name));
				var variety = new Variety(name);
				varieties[id] = Tuple.Create(variety, new List<Word>());
			}

			var senses = new Dictionary<string, Sense>();
			foreach (XElement glossElem in root.Elements("glosses").Elements("gloss"))
			{
				var gloss = ((string) glossElem.Element("name")).Trim();
				var pos = (string) glossElem.Element("part_of_speech");
				if (senses.ContainsKey(gloss))
					throw new ImportException(string.Format("The gloss, \"{0}\", is not unique.", gloss));
				var sense = new Sense(gloss, pos);
				senses[gloss] = sense;
				foreach (XElement transElem in glossElem.Elements("transcriptions").Elements("transcription"))
				{
					var varietyID = (string) transElem.Element("word_list_id");
					var wordform = (string) transElem.Element("name");
					if (wordform != null)
					{
						Tuple<Variety, List<Word>> variety;
						if (varieties.TryGetValue(varietyID, out variety))
						{
							foreach (string w in wordform.Split(','))
							{
								string str = w.Trim().Normalize(NormalizationForm.FormD);
								variety.Item2.Add(new Word(str, sense));
							}
						}
					}
				}
			}

			project.Senses.ReplaceAll(senses.Values);
			using (project.Varieties.BulkUpdate())
			{
				project.Varieties.Clear();
				foreach (Tuple<Variety, List<Word>> variety in varieties.Values)
				{
					variety.Item1.Words.AddRange(variety.Item2);
					project.Varieties.Add(variety.Item1);
				}
			}
		}
	}
}
