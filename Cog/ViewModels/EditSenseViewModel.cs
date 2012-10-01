using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class EditSenseViewModel : ViewModelBase, IDataErrorInfo
	{
		private readonly CogProject _project;
		private readonly string _displayName;
		private readonly Sense _sense;

		private string _gloss;
		private string _category;

		public EditSenseViewModel(CogProject project, Sense sense)
		{
			_displayName = "Edit Sense";
			_project = project;
			_sense = sense;
			_gloss = sense.Gloss;
			_category = sense.Category;
		}

		public EditSenseViewModel(CogProject project)
		{
			_project = project;
			_displayName = "New Sense";
		}

		public string DisplayName
		{
			get { return _displayName; }
		}

		public string Gloss
		{
			get { return _gloss; }
			set { Set("Gloss", ref _gloss, value); }
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
					case "Gloss":
						if (string.IsNullOrEmpty(_gloss))
							return "Please enter a gloss";
						if (_project.Senses.Any(sense => _sense != sense && sense.Gloss == _gloss))
							return "A variety with that gloss already exists";
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
