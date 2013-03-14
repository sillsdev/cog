using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SIL.Cog.ViewModels
{
	public class EditSoundClassViewModel : CogViewModelBase, IDataErrorInfo
	{
		private string _name;
		private readonly HashSet<string> _soundClassNames;

		public EditSoundClassViewModel(string displayName, IEnumerable<SoundClass> soundClasses)
			: base(displayName)
		{
			_soundClassNames = new HashSet<string>(soundClasses.Select(nc => nc.Name));
		}

		public EditSoundClassViewModel(string displayName, IEnumerable<SoundClass> soundClasses, SoundClass soundClass)
			: base(displayName)
		{
			_name = soundClass.Name;
			_soundClassNames = new HashSet<string>(soundClasses.Where(nc => nc != soundClass).Select(nc => nc.Name));
		}

		public string Name
		{
			get { return _name; }
			set { Set(() => Name, ref _name, value); }
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
						if (_soundClassNames.Contains(_name))
							return "A natural class with that name already exists";
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
