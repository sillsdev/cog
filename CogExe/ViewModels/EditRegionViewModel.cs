using System.Collections.ObjectModel;
using System.Linq;

namespace SIL.Cog.ViewModels
{
	public class EditRegionViewModel : CogViewModelBase
	{
		private readonly ReadOnlyCollection<VarietyViewModel> _varieties;
		private VarietyViewModel _currentVariety;
		private string _description;

		public EditRegionViewModel(CogProject project)
			: base("New Region")
		{
			_varieties = new ReadOnlyCollection<VarietyViewModel>(project.Varieties.Select(v => new VarietyViewModel(v)).ToArray());
			_currentVariety = _varieties[0];
		}

		public EditRegionViewModel(CogProject project, Variety variety, GeographicRegion region)
			: base("Edit Region")
		{
			_varieties = new ReadOnlyCollection<VarietyViewModel>(project.Varieties.Select(v => new VarietyViewModel(v)).ToArray());
			_currentVariety = _varieties.First(vm => vm.ModelVariety == variety);
			_description = region.Description;
		}

		public ReadOnlyCollection<VarietyViewModel> Varieties
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
