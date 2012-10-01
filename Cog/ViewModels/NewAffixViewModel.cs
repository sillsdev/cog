using System.ComponentModel;
using GalaSoft.MvvmLight;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class NewAffixViewModel : ViewModelBase, IDataErrorInfo
	{
		private readonly CogProject _project;
		private string _strRep;
		private string _type = "Prefix";
		private string _category;

		public NewAffixViewModel(CogProject project)
		{
			_project = project;
		}

		public string StrRep
		{
			get { return _strRep; }
			set { Set("StrRep", ref _strRep, value); }
		}

		public string Type
		{
			get { return _type; }
			set { Set("Type", ref _type, value); }
		}

		public string Category
		{
			get { return _category; }
			set { Set("Category", ref _category, value); }
		}

		public string this[string columnName]
		{
			get
			{
				switch (columnName)
				{
					case "StrRep":
						if (string.IsNullOrEmpty(_strRep))
							return "Please specify an affix";
						Shape shape;
						if (!_project.Segmenter.ToShape(_strRep, out shape))
							return "The affix contains invalid segments";
						break;
				}

				return null;
			}
		}

		public string Error
		{
			get { return null; }
		}
	}
}
