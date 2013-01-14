using System.ComponentModel;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public enum AffixViewModelType
	{
		[Description("Prefix")]
		Prefix,
		[Description("Suffix")]
		Suffix
	}

	public class AffixViewModel : ViewModelBase
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
					IsValid = _affix.Shape.Count > 0;
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

		public Affix ModelAffix
		{
			get { return _affix; }
		}

		public bool IsValid
		{
			get { return _isValid; }
			set { Set(() => IsValid, ref _isValid, value); }
		}
	}
}
