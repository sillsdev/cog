using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Cog.Domain.Config;
using SIL.Extensions;
using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Explorer.Services
{
	public class ProjectService
	{
		private readonly SegmentPool _segmentPool;

		public ProjectService(SegmentPool segmentPool)
		{
			_segmentPool = segmentPool;
		}

		public CogProject Project { get; private set; }
		public string ProjectName { get; private set; }
		public bool IsLoaded => Project != null;

		public async Task<bool> OpenAsync()
		{
			BrowserWindow mainWindow = Electron.WindowManager.BrowserWindows.First();
			string[] files = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, new OpenDialogOptions
			{
				Title = "Open project",
				Properties = new[] { OpenDialogProperty.openFile },
				Filters = new[]
				{
					new FileFilter { Name = "SayMore Project", Extensions = new[] { "sprj" } },
					new FileFilter { Name = "Cog Project", Extensions = new[] { "cogx" } },
				}
			});

			if (files.Length > 0)
			{
				string fileName = files[0];
				ProjectName = Path.GetFileNameWithoutExtension(fileName);
				string extension = Path.GetExtension(fileName);
				switch (extension.ToLowerInvariant())
				{
					case ".cogx":
						Project = ConfigManager.Load(_segmentPool, fileName);
						break;

					case ".sprj":
						using (Stream stream = Assembly.GetAssembly(GetType())
							.GetManifestResourceStream("SIL.Cog.Explorer.NewProject.cogx"))
						{
							Project = ConfigManager.Load(_segmentPool, stream);
						}
						var importer = new SayMoreWordListsImporter();
						importer.Import(fileName, Project);
						break;
				}
				if (Project.WordAligners[ComponentIdentifiers.PrimaryWordAligner] is Aline aline)
				{
					aline.MaxIndelScore = 50;
					aline.IndelCost = 0;
				}
				SegmentAll(Project.Varieties);
				return true;
			}
			return false;
		}

		public Dictionary<Word, Range<int>> SearchWords(Variety variety, string syllablePattern)
		{
			var newVariety = new Variety(variety.Name);
			var results = new Dictionary<Word, Range<int>>();
			foreach (Word word in variety.Words.Where(w => w.Shape.Count > 0))
			{
				int wordStartIndex = 0;
				int curIndex = 0;
				Range<ShapeNode> range = Range<ShapeNode>.Null;
				foreach (Annotation<ShapeNode> ann in word.Stem.Children)
				{
					string strRep = GetOriginalStrRep(word, ann.Range);
					if (ann.Type() == CogFeatureSystem.BoundaryType && strRep == " ")
					{
						if (!range.Equals(Range<ShapeNode>.Null))
						{
							Word newWord = CloneWord(word, wordStartIndex, curIndex);
							results[newWord] = Range<int>.Create(word.Shape.IndexOf(range.Start),
								word.Shape.IndexOf(range.End) + 1);
							newVariety.Words.Add(newWord);
							range = Range<ShapeNode>.Null;
						}
						wordStartIndex = curIndex + strRep.Length;
					}
					else if (ann.Type() == CogFeatureSystem.SyllableType)
					{
						if (strRep.StartsWith(syllablePattern))
						{
							range = ann.Range;
						}
					}
					curIndex += strRep.Length;
				}

				if (!range.Equals(Range<ShapeNode>.Null))
				{
					Word newWord = CloneWord(word, wordStartIndex, curIndex);
					results[newWord] = Range<int>.Create(word.Shape.IndexOf(range.Start),
						word.Shape.IndexOf(range.End) + 1);
					newVariety.Words.Add(newWord);
				}
			}

			SegmentAll(new[] { newVariety });
			return results;
		}

		public Alignment<Word, ShapeNode> Align(IEnumerable<Word> words)
		{
			IWordAligner aligner = Project.WordAligners[ComponentIdentifiers.PrimaryWordAligner];
			IWordAlignerResult result = aligner.Compute(words);
			return result.GetAlignments().First();
		}

		private void SegmentAll(IEnumerable<Variety> varieties)
		{
			var processors = new List<IProcessor<Variety>>
			{
				new VarietySegmenter(Project.Segmenter),
				Project.VarietyProcessors[ComponentIdentifiers.Syllabifier]
			};
			var pipeline = new MultiThreadedPipeline<Variety>(processors);
			pipeline.Process(varieties);
			pipeline.WaitForComplete();
		}

		private static string GetOriginalStrRep(Word word, Range<ShapeNode> range)
		{
			var sb = new StringBuilder();
			foreach (ShapeNode node in word.Shape.GetNodes(range))
			{
				string strRep = node.OriginalStrRep();
				sb.Append(strRep);
			}
			return sb.ToString();
		}

		private Word CloneWord(Word word, int startIndex, int endIndex)
		{
			Word newWord = word.Clone();
			newWord.StemIndex = startIndex;
			newWord.StemLength = endIndex - startIndex;
			return newWord;
		}
	}
}
