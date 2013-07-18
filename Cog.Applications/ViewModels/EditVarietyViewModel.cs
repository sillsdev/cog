using System.ComponentModel;
using GalaSoft.MvvmLight;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class EditVarietyViewModel : ViewModelBase, IDataErrorInfo
	{
		private readonly IKeyedCollection<string, Variety> _varieties;
		private readonly Variety _variety;
		private string _name;
		private readonly string _title;

		public EditVarietyViewModel(IKeyedCollection<string, Variety> varieties)
		{
			_title = "New Variety";
			_varieties = varieties;
		}

		public EditVarietyViewModel(IKeyedCollection<string, Variety> varieties, Variety variety)
		{
			_title = "Rename Variety";
			_varieties = varieties;
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
						Variety variety;
						if (_varieties.TryGetValue(_name, out variety) && variety != _variety)
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
