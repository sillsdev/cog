using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SIL.Machine;

namespace SIL.Cog
{
	public class ComparandaLoader : WordListsLoader
	{
		private readonly string _path;

		public ComparandaLoader(Segmenter segmenter, string path)
			: base(segmenter)
		{
			_path = path;
		}

		public override IEnumerable<Variety> Load()
		{
			var varietyWords = new Dictionary<string, List<Word>>();
			var cognates = new Dictionary<Tuple<string, string>, HashSet<string>>();

			XElement root = XElement.Load(_path);
			foreach (XElement concept in root.Elements("CONCEPT"))
			{
				var gloss = (string) concept.Attribute("ID");
				var category = (string) concept.Attribute("ROLE");
				var sense = new Sense(gloss, category);
				foreach (XElement varietyElem in concept.Elements().Where(elem => elem.Name != "NOTE"))
				{
					string id = varietyElem.Name.LocalName.ToLowerInvariant();
					List<Word> words;
					if (!varietyWords.TryGetValue(id, out words))
					{
						words = new List<Word>();
						varietyWords[id] = words;
					}
					var str = ((string) varietyElem.Element("STEM").Attribute("PHON")).Replace(" ", "");
					Shape shape;
					if (Segmenter.ToShape(str, out shape))
						words.Add(new Word(shape, sense));

					var cognateVarieties = (string) varietyElem.Attribute("COGN_PROB");
					if (!string.IsNullOrEmpty(cognateVarieties))
					{
						foreach (string id2 in cognateVarieties.Split(','))
						{
							Tuple<string, string> key = Tuple.Create(id, id2);
							HashSet<string> cogs;
							if (!cognates.TryGetValue(key, out cogs))
							{
								cogs = new HashSet<string>();
								cognates[key] = cogs;
							}
							cogs.Add(gloss);
						}
					}
				}
			}

			List<Variety> varieties = varietyWords.Select(kvp => new Variety(kvp.Key, kvp.Value)).ToList();
			LoadVarietyPairs(varieties, vp =>
			                            	{
			                            		HashSet<string> cognateGlosses;
												if (cognates.TryGetValue(Tuple.Create(vp.Variety1.ID, vp.Variety2.ID), out cognateGlosses))
												{
													foreach (WordPair wp in vp.WordPairs)
														wp.AreCognatesActual = cognateGlosses.Contains(wp.Word1.Sense.Gloss);
												}
			                            	});
			return varieties;
		}
	}
}
