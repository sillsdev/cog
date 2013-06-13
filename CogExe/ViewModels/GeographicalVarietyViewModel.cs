using System.Collections.ObjectModel;
using SIL.Cog.Services;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class GeographicalVarietyViewModel : VarietyViewModel
	{
		private readonly ReadOnlyMirroredCollection<GeographicRegion, GeographicalRegionViewModel> _regions;
		private int _clusterIndex;

		public GeographicalVarietyViewModel(IDialogService dialogService, CogProject project, Variety variety)
			: base(variety)
		{
			_regions = new ReadOnlyMirroredCollection<GeographicRegion, GeographicalRegionViewModel>(variety.Regions,
				region =>
					{
						var newRegion = new GeographicalRegionViewModel(dialogService, project, this, region);
						newRegion.PropertyChanged += ChildPropertyChanged;
						return newRegion;
					});
			_clusterIndex = -1;
		}

		public ReadOnlyObservableCollection<GeographicalRegionViewModel> Regions
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
