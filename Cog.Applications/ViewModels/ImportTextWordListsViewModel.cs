using System.ComponentModel;
using GalaSoft.MvvmLight;

namespace SIL.Cog.Applications.ViewModels
{
	public enum TextWordListsFormat
	{
		[Description("Varieties as rows")]
		VarietyRows,
		[Description("Senses as rows")]
		SenseRows
	}

	public class ImportTextWordListsViewModel : ViewModelBase
	{
		private bool _categoriesIncluded;
		private TextWordListsFormat _format;

		public bool CategoriesIncluded
		{
			get { return _categoriesIncluded; }
			set { Set(() => CategoriesIncluded, ref _categoriesIncluded, value); }
		}

		public TextWordListsFormat Format
		{
			get { return _format; }
			set { Set(() => Format, ref _format, value); }
		}
	}
}
