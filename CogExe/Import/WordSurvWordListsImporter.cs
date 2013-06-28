using System;
using System.Collections.Generic;
using System.Xml.Linq;
using SIL.Machine;

namespace SIL.Cog.Import
{
	public class WordSurvWordListsImporter : IWordListsImporter
	{
		public void Import(string path, CogProject project)
		{
			XElement root = XElement.Load(path);
			var varieties = new Dictionary<string, Tuple<Variety, List<Word>>>();
			using (project.Varieties.BulkUpdate())
			{
				foreach (XElement wordListElem in root.Elements("word_lists").Elements("word_list"))
				{
					var id = (string) wordListElem.Attribute("id");
					var name = (string) wordListElem.Element("name");
					if (project.Varieties.Contains(name))
						throw new ImportException(string.Format("The variety name, \"{0}\", is not unique.", name));
					var variety = new Variety(name);
					project.Varieties.Add(variety);
					varieties[id] = Tuple.Create(variety, new List<Word>());
				}
			}

			using (project.Senses.BulkUpdate())
			{
				foreach (XElement glossElem in root.Elements("glosses").Elements("gloss"))
				{
					var gloss = ((string) glossElem.Element("name")).Trim();
					var pos = (string) glossElem.Element("part_of_speech");
					if (project.Senses.Contains(gloss))
						throw new ImportException(string.Format("The gloss, \"{0}\", is not unique.", gloss));
					var sense = new Sense(gloss, pos);
					project.Senses.Add(sense);
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
									string str = w.Trim();
									Shape shape;
									if (!project.Segmenter.ToShape(null, str, null, out shape))
										shape = project.Segmenter.EmptyShape;
									variety.Item2.Add(new Word(str, shape, sense));
								}
							}
						}
					}
				}
			}

			foreach (Tuple<Variety, List<Word>> variety in varieties.Values)
			{
				variety.Item1.Words.AddRange(variety.Item2);
				project.Syllabifier.Syllabify(variety.Item1);
			}
		}
	}
}
