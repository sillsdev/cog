using System.ComponentModel;
using System.Linq;

namespace SIL.Cog.ViewModels
{
	public class EditSenseViewModel : CogViewModelBase, IDataErrorInfo
	{
		private readonly CogProject _project;
		private readonly Sense _sense;

		private string _gloss;
		private string _category;

		public EditSenseViewModel(CogProject project, Sense sense)
			: base("Edit Sense")
		{
			_project = project;
			_sense = sense;
			_gloss = sense.Gloss;
			_category = sense.Category;
		}

		public EditSenseViewModel(CogProject project)
			: base("New Sense")
		{
			_project = project;
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
