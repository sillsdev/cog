using System.Collections.Generic;
using GraphSharp;
using SIL.Cog.Services;
using SIL.Cog.WordListsLoaders;

namespace SIL.Cog.ViewModels
{
	public static class ViewModelUtilities
	{
		private static readonly Dictionary<FileType, IWordListsLoader> WordListsLoaders;
		static ViewModelUtilities()
		{
			WordListsLoaders = new Dictionary<FileType, IWordListsLoader>
				{
					{new FileType("Tab-delimited Text", ".txt"), new TextLoader()},
					{new FileType("WordSurv XML", ".xml"), new WordSurvXmlLoader()}
				};
		}

		public static bool ImportWordLists(IDialogService dialogService, CogProject project, object ownerViewModel)
		{
			FileDialogResult result = dialogService.ShowOpenFileDialog(ownerViewModel, "Import Word Lists", WordListsLoaders.Keys);
			if (result.IsValid)
			{
				project.Senses.Clear();
				project.Varieties.Clear();
				WordListsLoaders[result.SelectedFileType].Load(result.FileName, project);
				return true;
			}
			return false;
		}
	}
}
