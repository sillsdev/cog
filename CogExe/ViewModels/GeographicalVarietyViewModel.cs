using System.Collections.ObjectModel;
using SIL.Cog.Services;

namespace SIL.Cog.ViewModels
{
	public class GeographicalVarietyViewModel : VarietyViewModel
	{
		private readonly ListViewModelCollection<ObservableCollection<GeographicRegion>, GeographicalRegionViewModel, GeographicRegion> _regions;
		private int _clusterIndex;

		public GeographicalVarietyViewModel(IDialogService dialogService, CogProject project, Variety variety)
			: base(variety)
		{
			_regions = new ListViewModelCollection<ObservableCollection<GeographicRegion>, GeographicalRegionViewModel, GeographicRegion>(variety.Regions,
				region =>
					{
						var newRegion = new GeographicalRegionViewModel(dialogService, project, this, region);
						newRegion.PropertyChanged += ChildPropertyChanged;
						return newRegion;
					});
			_clusterIndex = -1;
		}

		public ObservableCollection<GeographicalRegionViewModel> Regions
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
