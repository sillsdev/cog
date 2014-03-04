using System.ComponentModel;
using GalaSoft.MvvmLight;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Application.ViewModels
{
	public class EditSenseViewModel : ViewModelBase, IDataErrorInfo
	{
		private readonly IKeyedCollection<string, Sense> _senses;
		private readonly Sense _sense;
		private readonly string _title;

		private string _gloss;
		private string _category;

		public EditSenseViewModel(IKeyedCollection<string, Sense> senses, Sense sense)
		{
			_title = "Edit Sense";
			_senses = senses;
			_sense = sense;
			_gloss = sense.Gloss;
			_category = sense.Category;
		}

		public EditSenseViewModel(IKeyedCollection<string, Sense> senses)
		{
			_title = "New Sense";
			_senses = senses;
		}

		public string Title
		{
			get { return _title; }
		}

		public string Gloss
		{
			get { return _gloss; }
			set { Set(() => Gloss, ref _gloss, value); }
		}

		public string Category
		{
			get { return _category; }
			set { Set(() => Category, ref _category, value); }
		}

		string IDataErrorInfo.this[string columnName]
		{
			get
			{
				switch (columnName)
				{
					case "Gloss":
						if (string.IsNullOrEmpty(_gloss))
							return "Please enter a gloss.";
						Sense sense;
						if (_senses.TryGetValue(_gloss, out sense) && sense != _sense)
							return "A variety with that gloss already exists.";
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
