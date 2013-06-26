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

using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using SIL.Cog.Services;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

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
            SimpleIoc.Default.Register<IWindowViewModelMappings, WindowViewModelMappings>();
			SimpleIoc.Default.Register<IDialogService, DialogService>();
			SimpleIoc.Default.Register<IProgressService, ProgressService>();
			SimpleIoc.Default.Register<SpanFactory<ShapeNode>, ShapeSpanFactory>();
			SimpleIoc.Default.Register<IExportService, ExportService>();
			SimpleIoc.Default.Register<IImportService, ImportService>();

            SimpleIoc.Default.Register<MainWindowViewModel>();
			SimpleIoc.Default.Register<InputMasterViewModel>();
			SimpleIoc.Default.Register<CompareMasterViewModel>();
			SimpleIoc.Default.Register<AnalyzeMasterViewModel>();
			SimpleIoc.Default.Register<WordListsViewModel>();
			SimpleIoc.Default.Register<VarietiesViewModel>();
			SimpleIoc.Default.Register<SensesViewModel>();
			SimpleIoc.Default.Register<InputSettingsViewModel>();
			SimpleIoc.Default.Register<SimilarityMatrixViewModel>();
			SimpleIoc.Default.Register<VarietyPairsViewModel>();
			SimpleIoc.Default.Register<CompareSettingsViewModel>();
			SimpleIoc.Default.Register<HierarchicalGraphViewModel>();
			SimpleIoc.Default.Register<NetworkGraphViewModel>();
			SimpleIoc.Default.Register<GeographicalViewModel>();
			SimpleIoc.Default.Register<SimilarSegmentsViewModel>();
			SimpleIoc.Default.Register<SenseAlignmentViewModel>();
        }

        public MainWindowViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainWindowViewModel>();
            }
        }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}