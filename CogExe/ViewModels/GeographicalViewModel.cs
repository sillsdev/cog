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

namespace SIL.Cog.ViewModels
{
	public class GeographicalViewModel : WorkspaceViewModelBase
	{
		private readonly IDialogService _dialogService;
		private readonly IImportService _importService;
		private readonly IExportService _exportService;
		private CogProject _project;
		private readonly ICommand _newRegionCommand;
		private readonly List<Cluster<Variety>> _currentClusters;
		private double _similarityScoreThreshold;
		private SimilarityMetric _similarityMetric;
		private ReadOnlyMirroredCollection<Variety, GeographicalVarietyViewModel> _varieties;

		public GeographicalViewModel(IDialogService dialogService, IImportService importService, IExportService exportService)
			: base("Geographical")
		{
			_dialogService = dialogService;
			_importService = importService;
			_exportService = exportService;
			_newRegionCommand = new RelayCommand<IEnumerable<Tuple<double, double>>>(AddNewRegion);
			_currentClusters = new List<Cluster<Variety>>();
			Messenger.Default.Register<NotificationMessage>(this, HandleNotificationMessage);
			_similarityScoreThreshold = 0.7;

			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Similarity metric",
				new CommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new CommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaCommandsViewModel("Other tasks",
				new CommandViewModel("Import regions", new RelayCommand(ImportRegions)),
				new CommandViewModel("Export this map", new RelayCommand(Export))));
		}

		private void ImportRegions()
		{
			_importService.ImportGeographicRegions(this, _project);
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
			_currentClusters.AddRange(clusterer.GenerateClusters(_varieties.Where(v => v.Regions.Count > 0).Select(v => v.ModelVariety)).OrderByDescending(c => c.DataObjects.Count));
			foreach (Cluster<Variety> cluster in _currentClusters)
			{
				var clusterVarieties = new HashSet<Variety>(cluster.DataObjects);
				foreach (GeographicalVarietyViewModel variety in _varieties.Where(v => clusterVarieties.Contains(v.ModelVariety)))
					variety.ClusterIndex = index;
				index++;
			}
		}

		private void ResetClusters()
		{
			_currentClusters.Clear();
			int index = 0;
			foreach (GeographicalVarietyViewModel variety in _varieties.Where(v => v.Regions.Count > 0))
			{
				variety.ClusterIndex = index;
				index++;
			}
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			Set("Varieties", ref _varieties, new ReadOnlyMirroredCollection<Variety, GeographicalVarietyViewModel>(project.Varieties,
				variety =>
					{
						var newVariety = new GeographicalVarietyViewModel(_dialogService, project, variety);
						((INotifyCollectionChanged) newVariety.Regions).CollectionChanged += (sender, e) => RegionsChanged(newVariety);
						newVariety.PropertyChanged += ChildPropertyChanged;
						return newVariety;
					}));
			if (_project.VarietyPairs.Count > 0)
				ClusterVarieties();
			else
				ResetClusters();
			project.Varieties.CollectionChanged += VarietiesChanged;
		}

		private void RegionsChanged(GeographicalVarietyViewModel variety)
		{
			if (variety.ClusterIndex == -1 || (variety.ClusterIndex != -1 && variety.Regions.Count == 0))
			{
				if (_currentClusters.Count == 0)
					ResetClusters();
				else
					ClusterVarieties();
			}
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			ResetClusters();
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			ChildrenAcceptChanges(_varieties);
		}

		public ReadOnlyObservableCollection<GeographicalVarietyViewModel> Varieties
		{
			get { return _varieties; }
		}

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set
			{
				if (Set(() => SimilarityMetric, ref _similarityMetric, value) && _currentClusters.Count > 0)
					ClusterVarieties();
			}
		}

		public double SimilarityScoreThreshold
		{
			get { return _similarityScoreThreshold; }
			set
			{
				if (Set(() => SimilarityScoreThreshold, ref _similarityScoreThreshold, value) && _currentClusters.Count > 0)
					ClusterVarieties();
			}
		}

		public ICommand NewRegionCommand
		{
			get { return _newRegionCommand; }
		}
	}
}
