using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SIL.Machine;

namespace SIL.Cog
{
	public class WordSurvXmlLoader : WordListsLoader
	{
		private readonly string _path;

		public WordSurvXmlLoader(Segmenter segmenter, string path)
			: base(segmenter)
		{
			_path = path;
		}

		public override IEnumerable<Variety> Load()
		{
			var varietyInfos = new Dictionary<string, Tuple<string, List<Word>>>();
			XElement root = XElement.Load(_path);
			foreach (XElement wordListElem in root.Elements("word_lists").Elements("word_list"))
			{
				var id = (string) wordListElem.Attribute("id");
				var name = (string) wordListElem.Element("name");
				varietyInfos[id] = Tuple.Create(name, new List<Word>());
			}

			foreach (XElement glossElem in root.Elements("glosses").Elements("gloss"))
			{
				var gloss = (string) glossElem.Element("name");
				var pos = (string) glossElem.Element("part_of_speech");
				var sense = new Sense(gloss, pos);
				foreach (XElement transElem in glossElem.Elements("transcriptions").Elements("transcription"))
				{
					var varietyID = (string) transElem.Element("word_list_id");
					var wordform = (string) transElem.Element("name");
					Tuple<string, List<Word>> varietyInfo;
					if (varietyInfos.TryGetValue(varietyID, out varietyInfo))
					{
						Shape shape;
						if (Segmenter.ToShape(wordform, out shape))
							varietyInfo.Item2.Add(new Word(shape, sense));
					}
				}
			}

			List<Variety> varieties = varietyInfos.Select(kvp => new Variety(kvp.Key, kvp.Value.Item2) {Description = kvp.Value.Item1}).ToList();
			LoadVarietyPairs(varieties);
			return varieties;
		}
	}
}
