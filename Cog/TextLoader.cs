using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog
{
	public class TextLoader : WordListsLoader
	{
		private readonly string _path;

		public TextLoader(Segmenter segmenter, string path)
			: base(segmenter)
		{
			_path = path;
		}

		public override IEnumerable<Variety> Load()
		{
			var varieties = new List<Variety>();
			using (var file = new StreamReader(_path))
			{
				string line = file.ReadLine();
				if (line == null)
					return Enumerable.Empty<Variety>();

				string[] glosses = line.Split('\t');

				line = file.ReadLine();
				if (line == null)
					return Enumerable.Empty<Variety>();

				string[] categories = line.Split('\t');

				var senses = new List<Sense>();
				for (int i = 1; i < glosses.Length; i++)
					senses.Add(new Sense(glosses[i].Trim(), categories.Length <= i ? null : categories[i].Trim()));

				while ((line = file.ReadLine()) != null)
				{
					var words = new List<Word>();
					string[] wordStrs = line.Split('\t');
					for (int i = 1; i < wordStrs.Length; i++)
					{
						string wordStr = wordStrs[i].Trim();
						if (!string.IsNullOrEmpty(wordStr))
						{
							foreach (string w in wordStr.Split(','))
							{
								Shape shape;
								if (Segmenter.ToShape(w.Trim(), out shape))
									words.Add(new Word(shape, senses[i - 1]));
							}
						}
					}

					varieties.Add(new Variety(wordStrs[0].Trim(), words));
				}
			}
			LoadVarietyPairs(varieties);
			return varieties;
		}
	}
}
