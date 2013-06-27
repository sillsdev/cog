using System.Collections.Generic;
using System.IO;
using SIL.Machine;

namespace SIL.Cog.Import
{
	public class TextWordListsImporter : IWordListsImporter
	{
		public void Import(string path, CogProject project)
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

				var senses = new Dictionary<string, Sense>();
				for (int i = 1; i < glosses.Length; i++)
				{
					string gloss = glosses[i].Trim();
					if (senses.ContainsKey(gloss))
						throw new ImportException(string.Format("The gloss, \"{0}\", is not unique.", gloss));
					senses[gloss] = new Sense(gloss, categories.Length <= i ? null : categories[i].Trim());
				}
				project.Senses.AddRange(senses.Values);

				using (project.Varieties.BulkUpdate())
				{
					while ((line = file.ReadLine()) != null)
					{
						string[] wordStrs = line.Split('\t');
						string name = wordStrs[0].Trim();
						if (project.Varieties.Contains(name))
							throw new ImportException(string.Format("The variety name, \"{0}\", is not unique.", name));
						var variety = new Variety(name);
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
									variety.Words.Add(new Word(str, shape, senses[glosses[i].Trim()]));
								}
							}
						}

						project.Varieties.Add(variety);
					}
				}
			}
		}
	}
}
