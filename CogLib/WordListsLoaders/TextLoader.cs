using System.Collections.Generic;
using System.IO;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.WordListsLoaders
{
	public class TextLoader : IWordListsLoader
	{
		public void Load(string path, CogProject project)
		{
			using (var file = new StreamReader(path))
			{
				string line = file.ReadLine();
				if (line == null)
					return;

				string[] glosses = line.Split('\t');

				line = file.ReadLine();
				if (line == null)
					return;

				string[] categories = line.Split('\t');

				var senses = new List<Sense>();
				for (int i = 1; i < glosses.Length; i++)
					senses.Add(new Sense(glosses[i].Trim(), categories.Length <= i ? null : categories[i].Trim()));
				project.Senses.AddRange(senses);

				while ((line = file.ReadLine()) != null)
				{
					string[] wordStrs = line.Split('\t');
					var variety = new Variety(wordStrs[0].Trim());
					for (int i = 1; i < wordStrs.Length; i++)
					{
						string wordStr = wordStrs[i].Trim();
						if (!string.IsNullOrEmpty(wordStr))
						{
							foreach (string w in wordStr.Split(','))
							{
								string str = w.Trim();
								Shape shape;
								if (!project.Segmenter.ToShape(null, str, null, out shape))
									shape = project.Segmenter.EmptyShape;
								variety.Words.Add(new Word(str, shape, senses[i - 1]));
							}
						}
					}

					project.Varieties.Add(variety);
				}
			}
		}
	}
}
