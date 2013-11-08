using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class SelectVarietiesViewModel : ViewModelBase
	{
		private readonly ReadOnlyList<SelectableVarietyViewModel> _varieties;
		private bool? _selectAll;
		private readonly SimpleMonitor _monitor;

		public SelectVarietiesViewModel(IEnumerable<Variety> varieties, ISet<Variety> selectedVarieties)
		{
			_varieties = new ReadOnlyList<SelectableVarietyViewModel>(varieties.Select(v => new SelectableVarietyViewModel(v) {IsSelected = selectedVarieties.Contains(v)}).ToArray());
			_selectAll = selectedVarieties.Count > 0;
			_monitor = new SimpleMonitor();
			if (selectedVarieties.Count > 0 && selectedVarieties.Count < _varieties.Count)
				_selectAll = null;

			foreach (SelectableVarietyViewModel variety in _varieties)
				variety.PropertyChanged += Varieties_PropertyChanged;
		}

		public ReadOnlyList<SelectableVarietyViewModel> Varieties
		{
			get { return _varieties; }
		}

		public bool? SelectAll
		{
			get { return _selectAll; }
			set
			{
				using (_monitor.Enter())
				{
					if (Set(() => SelectAll, ref _selectAll, value))
					{
						foreach (SelectableVarietyViewModel variety in _varieties)
							variety.IsSelected = _selectAll == null || (bool) _selectAll;
					}
				}
			}
		}

		private void Varieties_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsSelected" && !_monitor.Busy)
			{
				bool hasChecked = false;
				bool hasUnchecked = false;
				bool? newValue = null;
				foreach (SelectableVarietyViewModel variety in _varieties)
				{
					if (variety.IsSelected)
					{
						hasChecked = true;
						if (hasUnchecked)
							break;
					}
					else
					{
						hasUnchecked = true;
						if (hasChecked)
							break;
					}
				}
				if (!hasChecked || !hasUnchecked)
					newValue = hasChecked;
				Set(() => SelectAll, ref _selectAll, newValue);
			}
		}
	}
}
