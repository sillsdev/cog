using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Clusterers;
using SIL.Cog.Services;
using SIL.Collections;
using GalaSoft.MvvmLight.Threading;

namespace SIL.Cog.ViewModels
{
	public class GeographicalViewModel : WorkspaceViewModelBase
	{
		private readonly IDialogService _dialogService;
		private readonly IExportService _exportService;
		private CogProject _project;
		private ObservableCollection<VarietyRegionViewModel> _regions;
		private readonly ICommand _newRegionCommand;
		private readonly List<Cluster<Variety>> _currentClusters;
		private double _similarityScoreThreshold;
		private SimilarityMetric _similarityMetric;

		public GeographicalViewModel(IDialogService dialogService, IExportService exportService)
			: base("Geographical")
		{
			_dialogService = dialogService;
			_exportService = exportService;
			_newRegionCommand = new RelayCommand<IEnumerable<Tuple<double, double>>>(AddNewRegion);
			_regions = new ObservableCollection<VarietyRegionViewModel>();
			_currentClusters = new List<Cluster<Variety>>();
			Messenger.Default.Register<NotificationMessage>(this, HandleNotificationMessage);
			_similarityScoreThreshold = 0.7;

			TaskAreas.Add(new TaskAreaGroupViewModel("Similarity metric",
				new CommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new CommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaViewModel("Other tasks",
				new CommandViewModel("Export this map", new RelayCommand(Export))));
		}

		private void Export()
		{
			_exportService.ExportCurrentMap(this);
		}

		private void AddNewRegion(IEnumerable<Tuple<double, double>> coordinates)
		{
			var vm = new EditRegionViewModel(_project);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				var region = new GeographicRegion(coordinates.Select(coord => new GeographicCoordinate(coord.Item1, coord.Item2))) {Description = vm.Description};
				vm.CurrentVariety.ModelVariety.Regions.Add(region);
			}
		}

		private void HandleNotificationMessage(NotificationMessage msg)
		{
			switch (msg.Notification)
			{
				case Notifications.ComparisonPerformed:
					ClusterVarieties();
					break;
			}
		}

		private void ClusterVarieties()
		{
			if (_project.VarietyPairs.Count == 0)
				return;

			Func<Variety, Variety, double> getDistance = null;
			switch (_similarityMetric)
			{
				case SimilarityMetric.Lexical:
					getDistance = (v1, v2) => 1.0 - v1.VarietyPairs[v2].LexicalSimilarityScore;
					break;
				case SimilarityMetric.Phonetic:
					getDistance = (v1, v2) => 1.0 - v1.VarietyPairs[v2].PhoneticSimilarityScore;
					break;
			}

			var clusterer = new FlatUpgmaClusterer<Variety>(getDistance, 1.0 - _similarityScoreThreshold);
			int index = 0;
			_currentClusters.Clear();
			_currentClusters.AddRange(clusterer.GenerateClusters(_regions.Select(vm => vm.ModelVariety).Distinct()).OrderByDescending(c => c.DataObjects.Count));
			foreach (Cluster<Variety> cluster in _currentClusters)
			{
				var varieties = new HashSet<Variety>(cluster.DataObjects);
				foreach (VarietyRegionViewModel region in _regions.Where(r => varieties.Contains(r.ModelVariety)))
					region.ClusterIndex = index;
				index++;
			}
		}

		private void ResetClusters()
		{
			if (_currentClusters.Count > 0)
			{
				_currentClusters.Clear();
				foreach (VarietyRegionViewModel region in _regions)
					region.ClusterIndex = 0;
			}
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			Set(() => Regions, ref _regions, new ObservableCollection<VarietyRegionViewModel>());
			AddVarieties(_project.Varieties);
			project.Varieties.CollectionChanged += VarietiesChanged;
		}

		private void RegionsChanged(Variety variety, NotifyCollectionChangedEventArgs e)
		{
			DispatcherHelper.CheckBeginInvokeOnUI(() =>
				{
					switch (e.Action)
					{
						case NotifyCollectionChangedAction.Add:
							AddRegions(variety, e.NewItems.Cast<GeographicRegion>());
							break;

						case NotifyCollectionChangedAction.Remove:
							RemoveRegions(e.OldItems.Cast<GeographicRegion>());
							break;

						case NotifyCollectionChangedAction.Replace:
							RemoveRegions(e.OldItems.Cast<GeographicRegion>());
							AddRegions(variety, e.NewItems.Cast<GeographicRegion>());
							break;

						case NotifyCollectionChangedAction.Reset:
							_regions.RemoveAll(vm => vm.ModelVariety == variety);
							AddRegions(variety, variety.Regions);
							break;
					}
					IsChanged = true;
				});
		}

		private void AddRegions(Variety variety, IEnumerable<GeographicRegion> regions)
		{
			bool recluster = false;
			foreach (GeographicRegion region in regions)
			{
				int clusterIndex = _currentClusters.Count == 0 ? 0 : _currentClusters.FindIndex(c => c.DataObjects.Contains(variety));
				if (clusterIndex == -1)
				{
					recluster = true;
					clusterIndex = 0;
				}
				var regionVM = new VarietyRegionViewModel(_dialogService, _project, variety, region) {ClusterIndex = clusterIndex};
				regionVM.PropertyChanged += ChildPropertyChanged;
				_regions.Add(regionVM);
			}

			if (recluster)
				ClusterVarieties();
		}

		private void RemoveRegions(IEnumerable<GeographicRegion> regions)
		{
			var oldRegions = new HashSet<GeographicRegion>(regions);
			_regions.RemoveAll(vm => oldRegions.Contains(vm.ModelRegion));
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			DispatcherHelper.CheckBeginInvokeOnUI(() =>
				{
					switch (e.Action)
					{
						case NotifyCollectionChangedAction.Add:
							AddVarieties(e.NewItems.Cast<Variety>());
							break;

						case NotifyCollectionChangedAction.Remove:
							RemoveVarieties(e.OldItems.Cast<Variety>());
							break;

						case NotifyCollectionChangedAction.Replace:
							RemoveVarieties(e.OldItems.Cast<Variety>());
							AddVarieties(e.NewItems.Cast<Variety>());
							break;

						case NotifyCollectionChangedAction.Reset:
							Set(() => Regions, ref _regions, new ObservableCollection<VarietyRegionViewModel>());
							AddVarieties(_project.Varieties);
							break;
					}
				});
		}

		private void AddVarieties(IEnumerable<Variety> varieties)
		{
			ResetClusters();
			foreach (Variety variety in varieties)
			{
				AddRegions(variety, variety.Regions);
				Variety v = variety;
				variety.Regions.CollectionChanged += (sender, args) => RegionsChanged(v, args);
			}
		}

		private void RemoveVarieties(IEnumerable<Variety> varieties)
		{
			var oldVarieties = new HashSet<Variety>(varieties);
			_regions.RemoveAll(vm => oldVarieties.Contains(vm.ModelVariety));
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			ChildrenAcceptChanges(_regions);
		}

		public ObservableCollection<VarietyRegionViewModel> Regions
		{
			get { return _regions; }
		}

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set
			{
				if (Set(() => SimilarityMetric, ref _similarityMetric, value))
					ClusterVarieties();
			}
		}

		public double SimilarityScoreThreshold
		{
			get { return _similarityScoreThreshold; }
			set
			{
				Set(() => SimilarityScoreThreshold, ref _similarityScoreThreshold, value);
				ClusterVarieties();
			}
		}

		public ICommand NewRegionCommand
		{
			get { return _newRegionCommand; }
		}
	}
}
