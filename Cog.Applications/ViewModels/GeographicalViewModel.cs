using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Clusterers;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class GeographicalViewModel : WorkspaceViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly IDialogService _dialogService;
		private readonly IImportService _importService;
		private readonly IImageExportService _imageExportService;
		private readonly ICommand _newRegionCommand;
		private readonly List<Cluster<Variety>> _currentClusters;
		private double _similarityScoreThreshold;
		private SimilarityMetric _similarityMetric;
		private ReadOnlyMirroredList<Variety, GeographicalVarietyViewModel> _varieties;

		public GeographicalViewModel(IProjectService projectService, IDialogService dialogService, IImportService importService, IImageExportService imageExportService)
			: base("Geographical")
		{
			_projectService = projectService;
			_dialogService = dialogService;
			_importService = importService;
			_imageExportService = imageExportService;
			_newRegionCommand = new RelayCommand<IEnumerable<Tuple<double, double>>>(AddNewRegion);
			_currentClusters = new List<Cluster<Variety>>();

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => ClusterVarieties());
			Messenger.Default.Register<DomainModelChangingMessage>(this, msg => ResetClusters());

			_similarityScoreThreshold = 0.7;

			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Similarity metric",
				new TaskAreaCommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new TaskAreaCommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Import regions", new RelayCommand(ImportRegions)),
				new TaskAreaCommandViewModel("Export this map", new RelayCommand(Export))));
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			Set("Varieties", ref _varieties, new ReadOnlyMirroredList<Variety, GeographicalVarietyViewModel>(_projectService.Project.Varieties,
				variety =>
					{
						var newVariety = new GeographicalVarietyViewModel(_dialogService, _projectService.Project.Varieties, variety);
						newVariety.Regions.CollectionChanged += (s, evt) => RegionsChanged(newVariety);
						return newVariety;
					}, vm => vm.DomainVariety));
			if (_projectService.Project.VarietyPairs.Count > 0)
				ClusterVarieties();
			else
				ResetClusters();
		}

		private void ImportRegions()
		{
			_importService.ImportGeographicRegions(this);
		}

		private void Export()
		{
			_imageExportService.ExportCurrentMap(this);
		}

		private void AddNewRegion(IEnumerable<Tuple<double, double>> coordinates)
		{
			var vm = new EditRegionViewModel(_projectService.Project.Varieties);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var region = new GeographicRegion(coordinates.Select(coord => new GeographicCoordinate(coord.Item1, coord.Item2))) {Description = vm.Description};
				vm.CurrentVariety.DomainVariety.Regions.Add(region);
			}
		}

		private void ClusterVarieties()
		{
			if (_projectService.Project.VarietyPairs.Count == 0)
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
			_currentClusters.AddRange(clusterer.GenerateClusters(_varieties.Where(v => v.Regions.Count > 0).Select(v => v.DomainVariety)).OrderByDescending(c => c.DataObjects.Count));
			foreach (Cluster<Variety> cluster in _currentClusters)
			{
				var clusterVarieties = new HashSet<Variety>(cluster.DataObjects);
				foreach (GeographicalVarietyViewModel variety in _varieties.Where(v => clusterVarieties.Contains(v.DomainVariety)))
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

		public ReadOnlyObservableList<GeographicalVarietyViewModel> Varieties
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
