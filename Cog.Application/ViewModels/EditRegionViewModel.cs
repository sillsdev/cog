using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Application.ViewModels
{
	public class EditRegionViewModel : ViewModelBase
	{
		private readonly ReadOnlyList<VarietyViewModel> _varieties;
		private VarietyViewModel _selectedVariety;
		private string _description;
		private readonly string _title;

		public EditRegionViewModel(IEnumerable<Variety> varieties)
		{
			_title = "New Region";
			_varieties = new ReadOnlyList<VarietyViewModel>(varieties.Select(v => new VarietyViewModel(v)).OrderBy(vm => vm.Name).ToArray());
			_selectedVariety = _varieties[0];
		}

		public EditRegionViewModel(IEnumerable<Variety> varieties, Variety variety, GeographicRegion region)
		{
			_title = "Edit Region";
			_varieties = new ReadOnlyList<VarietyViewModel>(varieties.Select(v => new VarietyViewModel(v)).OrderBy(vm => vm.Name).ToArray());
			_selectedVariety = _varieties.First(vm => vm.DomainVariety == variety);
			_description = region.Description;
		}

		public string Title
		{
			get { return _title; }
		}

		public ReadOnlyList<VarietyViewModel> Varieties
		{
			get { return _varieties; }
		}

		public VarietyViewModel SelectedVariety
		{
			get { return _selectedVariety; }
			set { Set(() => SelectedVariety, ref _selectedVariety, value); }
		}

		public string Description
		{
			get { return _description; }
			set { Set(() => Description, ref _description, value); }
		}
	}
}
