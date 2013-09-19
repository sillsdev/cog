using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class SelectVarietiesViewModel : ViewModelBase
	{
		private readonly ReadOnlyList<SelectableVarietyViewModel> _varieties;
		private bool _selectAll;

		public SelectVarietiesViewModel(IEnumerable<Variety> varieties, ISet<Variety> selectedVarieties)
		{
			_varieties = new ReadOnlyList<SelectableVarietyViewModel>(varieties.Select(v => new SelectableVarietyViewModel(v) {IsSelected = selectedVarieties.Contains(v)}).ToArray());
			_selectAll = selectedVarieties.Count > 0;
		}

		public ReadOnlyList<SelectableVarietyViewModel> Varieties
		{
			get { return _varieties; }
		}

		public bool SelectAll
		{
			get { return _selectAll; }
			set
			{
				if (Set(() => SelectAll, ref _selectAll, value))
				{
					foreach (SelectableVarietyViewModel variety in _varieties)
						variety.IsSelected = _selectAll;
				}
			}
		}
	}
}
