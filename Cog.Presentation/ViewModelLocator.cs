/*
  In App.xaml:
  <Application.Resources>
	  <vm:ViewModelLocator xmlns:vm="clr-namespace:Cog"
						   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using Autofac;
using SIL.Cog.Application.Services;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.Cog.Presentation.Services;
using SIL.Machine.Annotations;

namespace SIL.Cog.Presentation
{
	/// <summary>
	/// This class contains static references to all the view models in the
	/// application and provides an entry point for the bindings.
	/// </summary>
	public class ViewModelLocator
	{
		private readonly IContainer _container;

		/// <summary>
		/// Initializes a new instance of the ViewModelLocator class.
		/// </summary>
		public ViewModelLocator()
		{
			var builder = new ContainerBuilder();

			////if (ViewModelBase.IsInDesignModeStatic)
			////{
			////    // Create design time view services and models
			////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
			////}
			////else
			////{
			////    // Create run time view services and models
			////    SimpleIoc.Default.Register<IDataService, DataService>();
			////}
			
			// Services
			builder.RegisterType<AnalysisService>().As<IAnalysisService>().SingleInstance();
			builder.RegisterType<ProjectService>().As<IProjectService>().SingleInstance();
			builder.RegisterType<WindowViewModelMappings>().As<IWindowViewModelMappings>().SingleInstance();
			builder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();
			builder.RegisterType<BusyService>().As<IBusyService>().SingleInstance();
			builder.RegisterType<ShapeSpanFactory>().As<SpanFactory<ShapeNode>>().SingleInstance();
			builder.RegisterType<ExportService>().As<IExportService>().SingleInstance();
			builder.RegisterType<ImportService>().As<IImportService>().SingleInstance();
			builder.RegisterType<ImageExportService>().As<IImageExportService>().SingleInstance();
			builder.RegisterType<SettingsService>().As<ISettingsService>().SingleInstance();
			builder.RegisterType<GraphService>().As<IGraphService>().SingleInstance();
			builder.RegisterType<SegmentPool>().SingleInstance();

			// Master view models
			builder.RegisterType<MainWindowViewModel>().SingleInstance();
			builder.RegisterType<InputViewModel>().SingleInstance();
			builder.RegisterType<CompareViewModel>().SingleInstance();
			builder.RegisterType<AnalyzeViewModel>().SingleInstance();

			// Workspace view models
			builder.RegisterType<WordListsViewModel>().SingleInstance();
			builder.RegisterType<VarietiesViewModel>().SingleInstance();
			builder.RegisterType<MeaningsViewModel>().SingleInstance();
			builder.RegisterType<InputSettingsViewModel>().SingleInstance();
			builder.RegisterType<SimilarityMatrixViewModel>().SingleInstance();
			builder.RegisterType<VarietyPairsViewModel>().SingleInstance();
			builder.RegisterType<CompareSettingsViewModel>().SingleInstance();
			builder.RegisterType<HierarchicalGraphViewModel>().SingleInstance();
			builder.RegisterType<NetworkGraphViewModel>().SingleInstance();
			builder.RegisterType<GeographicalViewModel>().SingleInstance();
			builder.RegisterType<GlobalCorrespondencesViewModel>().SingleInstance();
			builder.RegisterType<MultipleWordAlignmentViewModel>().SingleInstance();
			builder.RegisterType<SegmentsViewModel>().SingleInstance();

			// Component settings view models
			builder.RegisterType<SyllabifierViewModel>().SingleInstance();
			builder.RegisterType<PoorMansAffixIdentifierViewModel>().SingleInstance();
			builder.RegisterType<AlineViewModel>().SingleInstance();
			builder.RegisterType<CognateIdentifierOptionsViewModel>().SingleInstance();
			builder.RegisterType<BlairCognateIdentifierViewModel>().SingleInstance();
			builder.RegisterType<ThresholdCognateIdentifierViewModel>().SingleInstance();
			builder.RegisterType<DolgopolskyCognateIdentifierViewModel>().SingleInstance();

			// Multiple instance view models
			builder.RegisterType<WordListsVarietyViewModel>();
			builder.RegisterType<WordListsVarietyMeaningViewModel>();
			builder.RegisterType<WordViewModel>();
			builder.RegisterType<VarietiesVarietyViewModel>();
			builder.RegisterType<WordsViewModel>();
			builder.RegisterType<GeographicalVarietyViewModel>();
			builder.RegisterType<GeographicalRegionViewModel>();
			builder.RegisterType<SimilarSegmentMappingsViewModel>();
			builder.RegisterType<SoundClassesViewModel>();
			builder.RegisterType<SegmentMappingsViewModel>();
			builder.RegisterType<SegmentMappingViewModel>();
			builder.RegisterType<VarietyPairViewModel>();
			builder.RegisterType<WordPairsViewModel>();
			builder.RegisterType<SegmentMappingsChartViewModel>();
			builder.RegisterType<SegmentMappingsChartSegmentPairViewModel>();
			builder.RegisterType<NewSegmentMappingViewModel>();

			_container = builder.Build();
		}

		public MainWindowViewModel Main
		{
			get { return _container.Resolve<MainWindowViewModel>(); }
		}
		
		public static void Cleanup()
		{
			// TODO Clear the ViewModels
		}
	}
}