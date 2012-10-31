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

		public AffixViewModel(Affix affix)
		{
			_affix = affix;
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
	}
}
