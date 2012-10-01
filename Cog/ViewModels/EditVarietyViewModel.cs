using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class EditVarietyViewModel : ViewModelBase, IDataErrorInfo
	{
		private readonly CogProject _project;
		private readonly Variety _variety;
		private readonly string _displayName;
		private string _name;

		public EditVarietyViewModel(CogProject project)
		{
			_project = project;
			_displayName = "New Variety";
		}

		public EditVarietyViewModel(CogProject project, Variety variety)
		{
			_project = project;
			_variety = variety;
			_displayName = "Rename Variety";
			_name = variety.Name;
		}

		public string DisplayName
		{
			get { return _displayName; }
		}

		public string Name
		{
			get { return _name; }
			set { Set("Name", ref _name, value); }
		}

		public string this[string columnName]
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

		public string Error
		{
			get { return null; }
		}
	}
}
