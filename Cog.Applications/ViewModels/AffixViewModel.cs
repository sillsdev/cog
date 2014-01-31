using System.ComponentModel;
using GalaSoft.MvvmLight;
using SIL.Cog.Domain;
using SIL.Machine.Morphology;

namespace SIL.Cog.Applications.ViewModels
{
	public enum AffixViewModelType
	{
		[Description("Prefix")]
		Prefix,
		[Description("Suffix")]
		Suffix
	}

	public class AffixViewModel : ViewModelBase, IDataErrorInfo
	{
		private readonly Affix _affix;
		private bool _isValid;

		public AffixViewModel(Affix affix)
		{
			_affix = affix;
			_isValid = _affix.Shape.Count > 0;
			_affix.PropertyChanged += AffixPropertyChanged;
		}

		private void AffixPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Shape":
					_isValid = _affix.Shape.Count > 0;
					RaisePropertyChanged("Item[]");
					break;
			}
		}

		public string StrRep
		{
			get { return _affix.StrRep; }
		}

		public string Category
		{
			get { return _affix.Category; }
		}

		public AffixViewModelType Type
		{
			get { return _affix.Type == AffixType.Prefix ? AffixViewModelType.Prefix : AffixViewModelType.Suffix; }
		}

		internal Affix DomainAffix
		{
			get { return _affix; }
		}

		string IDataErrorInfo.this[string columnName]
		{
			get
			{
				switch (columnName)
				{
					case "StrRep":
						if (!_isValid)
							return "The affix contains invalid segments.";
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
