using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.ViewModels
{
	public abstract class EditSoundClassViewModel : ViewModelBase, IDataErrorInfo
	{
		private string _name;
		private readonly HashSet<string> _soundClassNames;
		private readonly string _title;

		protected EditSoundClassViewModel(string title, IEnumerable<SoundClass> soundClasses)
		{
			_title = title;
			_soundClassNames = new HashSet<string>(soundClasses.Select(nc => nc.Name));
		}

		protected EditSoundClassViewModel(string title, IEnumerable<SoundClass> soundClasses, SoundClass soundClass)
		{
			_title = title;
			_name = soundClass.Name;
			_soundClassNames = new HashSet<string>(soundClasses.Where(nc => nc != soundClass).Select(nc => nc.Name));
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
							return "Please enter a name.";
						if (_soundClassNames.Contains(_name))
							return "A natural class with that name already exists.";
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
