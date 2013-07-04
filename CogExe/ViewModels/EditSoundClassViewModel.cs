using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SIL.Cog.ViewModels
{
	public abstract class EditSoundClassViewModel : CogViewModelBase, IDataErrorInfo
	{
		private string _name;
		private readonly HashSet<string> _soundClassNames;

		protected EditSoundClassViewModel(string displayName, IEnumerable<SoundClass> soundClasses)
			: base(displayName)
		{
			_soundClassNames = new HashSet<string>(soundClasses.Select(nc => nc.Name));
		}

		protected EditSoundClassViewModel(string displayName, IEnumerable<SoundClass> soundClasses, SoundClass soundClass)
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

		string IDataErrorInfo.this[string columnName]
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

		string IDataErrorInfo.Error
		{
			get { return null; }
		}
	}
}
