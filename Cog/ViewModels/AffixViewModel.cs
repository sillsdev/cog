using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
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

		public string Type
		{
			get { return _affix.Type == AffixType.Prefix ? "Prefix" : "Suffix"; }
		}

		public Affix ModelAffix
		{
			get { return _affix; }
		}
	}
}
