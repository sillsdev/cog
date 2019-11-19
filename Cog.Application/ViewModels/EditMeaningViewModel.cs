using System.ComponentModel;
using GalaSoft.MvvmLight;
using SIL.Cog.Domain;
using SIL.ObjectModel;

namespace SIL.Cog.Application.ViewModels
{
	public class EditMeaningViewModel : ViewModelBase, IDataErrorInfo
	{
		private readonly IKeyedCollection<string, Meaning> _meanings;
		private readonly Meaning _meaning;
		private readonly string _title;

		private string _gloss;
		private string _category;

		public EditMeaningViewModel(IKeyedCollection<string, Meaning> meanings, Meaning meaning)
		{
			_title = "Edit Meaning";
			_meanings = meanings;
			_meaning = meaning;
			_gloss = meaning.Gloss;
			_category = meaning.Category;
		}

		public EditMeaningViewModel(IKeyedCollection<string, Meaning> meanings)
		{
			_title = "New Meaning";
			_meanings = meanings;
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
						Meaning meaning;
						if (_meanings.TryGet(_gloss, out meaning) && meaning != _meaning)
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
