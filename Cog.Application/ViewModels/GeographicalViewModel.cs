using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Application.Collections;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine.Clusterers;

namespace SIL.Cog.Application.ViewModels
{
	public class GeographicalViewModel : WorkspaceViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly IDialogService _dialogService;
		private readonly IImportService _importService;
		private readonly IImageExportService _imageExportService;
		private readonly GeographicalVarietyViewModel.Factory _varietyFactory; 

		private readonly ICommand _newRegionCommand;
		private readonly List<Cluster<Variety>> _currentClusters;
		private double _similarityScoreThreshold;
		private SimilarityMetric _similarityMetric;
		private MirroredBindableList<Variety, GeographicalVarietyViewModel> _varieties;
		private GeographicalRegionViewModel _selectedRegion;

		public GeographicalViewModel(IProjectService projectService, IDialogService dialogService, IImportService importService, IImageExportService imageExportService,
			GeographicalVarietyViewModel.Factory varietyFactory)
			: base("Geographical")
		{
			_projectService = projectService;
			_dialogService = dialogService;
			_importService = importService;
			_imageExportService = imageExportService;
			_varietyFactory = varietyFactory;

			_newRegionCommand = new RelayCommand<IEnumerable<Tuple<double, double>>>(AddNewRegion);
			_currentClusters = new List<Cluster<Variety>>();

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => ClusterVarieties());
			Messenger.Default.Register<DomainModelChangedMessage>(this, msg =>
			{
				if (msg.AffectsComparison)
					ResetClusters();
				if (_selectedRegion != null && (!_varieties.Contains(_selectedRegion.Variety) || !_selectedRegion.Variety.Regions.Contains(_selectedRegion)))
					SelectedRegion = null;
			});
			Messenger.Default.Register<PerformingComparisonMessage>(this, msg => ResetClusters());

			_similarityScoreThreshold = 0.7;

			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Similarity metric",
				new TaskAreaCommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new TaskAreaCommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Import regions", new RelayCommand(ImportRegions)),
				new TaskAreaCommandViewModel("Export map", new RelayCommand(Export))));
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			Set("Varieties", ref _varieties, new MirroredBindableList<Variety, GeographicalVarietyViewModel>(_projectService.Project.Varieties,
				variety =>
					{
						var newVariety = _varietyFactory(variety);
						newVariety.Regions.CollectionChanged += (s, evt) => RegionsChanged(newVariety);
						return newVariety;
					}, vm => vm.DomainVariety));
			if (_projectService.AreAllVarietiesCompared)
				ClusterVarieties();
			else
				ResetClusters();
			SelectedRegion = null;
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
				vm.SelectedVariety.DomainVariety.Regions.Add(region);
				SelectedRegion = _varieties[vm.SelectedVariety.DomainVariety].Regions.Single(r => r.DomainRegion == region);
				Messenger.Default.Send(new DomainModelChangedMessage(false));
			}
		}

		private void ClusterVarieties()
		{
			if (!_projectService.AreAllVarietiesCompared)
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
			_currentClusters.Clear();
			_currentClusters.AddRange(clusterer.GenerateClusters(_varieties.Select(v => v.DomainVariety)).Where(c => c.DataObjects.Any(v => v.Regions.Count > 0)).OrderByDescending(c => c.DataObjects.Count));
			foreach (GeographicalVarietyViewModel variety in _varieties)
			{
				if (variety.Regions.Count > 0)
				{
					int index = _currentClusters.FindIndex(c => c.DataObjects.Contains(variety.DomainVariety));
					variety.ClusterIndex = index;
				}
				else
				{
					variety.ClusterIndex = -1;
				}
			}
		}

		private void ResetClusters()
		{
			_currentClusters.Clear();
			int index = 0;
			foreach (GeographicalVarietyViewModel variety in _varieties)
			{
				if (variety.Regions.Count > 0)
				{
					variety.ClusterIndex = index;
					index++;
				}
				else
				{
					variety.ClusterIndex = -1;
				}
			}
		}

		private void RegionsChanged(GeographicalVarietyViewModel variety)
		{
			if (variety.ClusterIndex == -1 || (variety.ClusterIndex != -1 && variety.Regions.Count == 0))
			{
				if (_projectService.AreAllVarietiesCompared)
					ClusterVarieties();
				else
					ResetClusters();
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

		public GeographicalRegionViewModel SelectedRegion
		{
			get { return _selectedRegion; }
			set { Set(() => SelectedRegion, ref _selectedRegion, value); }
		}

		public ICommand NewRegionCommand
		{
			get { return _newRegionCommand; }
		}
	}
}
