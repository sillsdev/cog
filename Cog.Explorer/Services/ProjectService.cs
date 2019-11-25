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
using SIL.Machine.Annotations;

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
				SegmentAll(false);
				return true;
			}
			return false;
		}

		public IReadOnlyList<Word> FocusWords(Variety variety, string syllablePattern)
		{
			SegmentAll(true);

			var results = new List<Word>();
			foreach (Word word in variety.Words.Where(w => w.Shape.Count > 0))
			{
				int wordStartIndex = 0;
				int curIndex = 0;
				bool match = false;
				foreach (Annotation<ShapeNode> ann in word.Stem.Children)
				{
					string strRep = GetOriginalStrRep(word, ann.Range);
					if (ann.Type() == CogFeatureSystem.BoundaryType && strRep == " ")
					{
						if (match)
						{
							MarkWord(word, wordStartIndex, curIndex);
							results.Add(word);
							match = false;
							break;
						}
						wordStartIndex = curIndex + strRep.Length;
					}
					else if (ann.Type() == CogFeatureSystem.SyllableType)
					{
						if (strRep.StartsWith(syllablePattern))
							match = true;
					}
					curIndex += strRep.Length;
				}

				if (match)
				{
					MarkWord(word, wordStartIndex, curIndex);
					results.Add(word);
				}
			}

			SegmentAll(false);
			return results;
		}

		private void SegmentAll(bool stripAffixes)
		{
			var processors = new List<IProcessor<Variety>>();
			if (stripAffixes)
				processors.Add(new AffixStripper(Project.Segmenter));
			processors.Add(new VarietySegmenter(Project.Segmenter));
			processors.Add(Project.VarietyProcessors[ComponentIdentifiers.Syllabifier]);
			var pipeline = new MultiThreadedPipeline<Variety>(processors);
			pipeline.Process(Project.Varieties);
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

		private void MarkWord(Word word, int startIndex, int endIndex)
		{
			word.StemIndex = startIndex;
			word.StemLength = endIndex - startIndex;
		}
	}
}
