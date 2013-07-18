using System.Collections.Generic;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class GeographicalVarietyViewModel : VarietyViewModel
	{
		private readonly ReadOnlyMirroredList<GeographicRegion, GeographicalRegionViewModel> _regions;
		private int _clusterIndex;

		public GeographicalVarietyViewModel(IDialogService dialogService, ICollection<Variety> varieties, Variety variety)
			: base(variety)
		{
			_regions = new ReadOnlyMirroredList<GeographicRegion, GeographicalRegionViewModel>(variety.Regions,
				region => new GeographicalRegionViewModel(dialogService, varieties, this, region), vm => vm.DomainRegion);
			_clusterIndex = -1;
		}

		public ReadOnlyObservableList<GeographicalRegionViewModel> Regions
		{
			get { return _regions; }
		}

		public int ClusterIndex
		{
			get { return _clusterIndex; }
			set { Set(() => ClusterIndex, ref _clusterIndex, value); }
		}
	}
}
