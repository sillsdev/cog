using SIL.Cog.Domain;

namespace SIL.Cog.Application.ViewModels
{
	public class SelectableVarietyViewModel : VarietyViewModel
	{
		private bool _isSelected;

		public SelectableVarietyViewModel(Variety variety)
			: base(variety)
		{
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set { Set(() => IsSelected, ref _isSelected, value); }
		}
	}
}
