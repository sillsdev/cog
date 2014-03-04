using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Presentation.Behaviors;
using Xceed.Wpf.DataGrid;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for SimilarityMatrixView.xaml
	/// </summary>
	public partial class SimilarityMatrixView
	{
		public SimilarityMatrixView()
		{
			InitializeComponent();
			SimMatrixGrid.ClipboardExporters.Clear();
			BusyCursor.DisplayUntilIdle();
		}

		private void SimilarityMatrixView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = e.NewValue as SimilarityMatrixViewModel;
			if (vm != null)
			{
				LoadCollectionView();
				vm.PropertyChanged += ViewModel_PropertyChanged;
			}
		}

		private void LoadCollectionView()
		{
			var vm = (SimilarityMatrixViewModel) DataContext;

			SimMatrixGrid.Columns.Clear();
			var view = new DataGridCollectionView(vm.Varieties, typeof(SimilarityMatrixVarietyViewModel), false, false);
			view.ItemProperties.Add(new DataGridItemProperty("Variety", "Name", typeof(string)));
			for (int i = 0; i < vm.Varieties.Count; i++)
				view.ItemProperties.Add(new DataGridItemProperty("Variety" + i, string.Format("VarietyPairs[{0}]", i), typeof(SimilarityMatrixVarietyPairViewModel)));
			SimMatrixGrid.ItemsSource = view;

			var headerColumn = new Column {FieldName = "Variety", Title = "", DisplayMemberBindingInfo = new DataGridBindingInfo {Path = new PropertyPath("Name"), ReadOnly = true}};
			DataGridControlBehaviors.SetIsRowHeader(headerColumn, true);
			DataGridControlBehaviors.SetAutoSize(headerColumn, true);
			SimMatrixGrid.Columns.Add(headerColumn);
			for (int i = 0; i < vm.Varieties.Count; i++)
			{
				var column = new Column {FieldName = "Variety" + i, Width = 32};
				var titleBinding = new Binding(string.Format("DataGridControl.DataContext.Varieties[{0}].Name", i)) {RelativeSource = RelativeSource.Self};
				BindingOperations.SetBinding(column, ColumnBase.TitleProperty, titleBinding);
				SimMatrixGrid.Columns.Add(column);
			}
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Varieties":
					DispatcherHelper.CheckBeginInvokeOnUI(LoadCollectionView);
					break;
			}
		}
	}
}
