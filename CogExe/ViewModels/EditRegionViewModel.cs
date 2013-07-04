using System.Linq;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class EditRegionViewModel : CogViewModelBase
	{
		private readonly ReadOnlyList<VarietyViewModel> _varieties;
		private VarietyViewModel _currentVariety;
		private string _description;

		public EditRegionViewModel(CogProject project)
			: base("New Region")
		{
			_varieties = new ReadOnlyList<VarietyViewModel>(project.Varieties.Select(v => new VarietyViewModel(v)).OrderBy(vm => vm.Name).ToArray());
			_currentVariety = _varieties[0];
		}

		public EditRegionViewModel(CogProject project, Variety variety, GeographicRegion region)
			: base("Edit Region")
		{
			_varieties = new ReadOnlyList<VarietyViewModel>(project.Varieties.Select(v => new VarietyViewModel(v)).OrderBy(vm => vm.Name).ToArray());
			_currentVariety = _varieties.First(vm => vm.ModelVariety == variety);
			_description = region.Description;
		}

		public ReadOnlyList<VarietyViewModel> Varieties
		{
			get { return _varieties; }
		}

		public VarietyViewModel CurrentVariety
		{
			get { return _currentVariety; }
			set { Set(() => CurrentVariety, ref _currentVariety, value); }
		}

		public string Description
		{
			get { return _description; }
			set { Set(() => Description, ref _description, value); }
		}
	}
}
