using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class EditVarietyViewModel : ViewModelBase, IDataErrorInfo
	{
		private readonly CogProject _project;
		private readonly Variety _variety;
		private string _name;
		private readonly string _title;

		public EditVarietyViewModel(CogProject project)
		{
			_title = "New Variety";
			_project = project;
		}

		public EditVarietyViewModel(CogProject project, Variety variety)
		{
			_title = "Rename Variety";
			_project = project;
			_variety = variety;
			_name = variety.Name;
		}

		public string Title
		{
			get { return _title; }
		}

		public string Name
		{
			get { return _name; }
			set { Set(() => Name, ref _name, value); }
		}

		string IDataErrorInfo.this[string columnName]
		{
			get
			{
				switch (columnName)
				{
					case "Name":
						if (string.IsNullOrEmpty(_name))
							return "Please enter a name";
						if (_project.Varieties.Any(variety => variety != _variety && variety.Name == _name))
							return "A variety with that name already exists";
						break;
				}

				return null;
			}
		}

		string IDataErrorInfo.Error
		{
			get { return null; }
		}
	}
}
