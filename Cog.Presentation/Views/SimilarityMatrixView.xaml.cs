using System.ComponentModel;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Applications.ViewModels;
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
			var source = new DataGridCollectionView(vm.Varieties, typeof(SimilarityMatrixVarietyViewModel), false, false);
			source.ItemProperties.Add(new DataGridItemProperty("Variety", "Name", typeof(string)));
			for (int i = 0; i < vm.Varieties.Count; i++)
				source.ItemProperties.Add(new DataGridItemProperty(vm.Varieties[i].Name, string.Format("VarietyPairs[{0}]", i), typeof (SimilarityMatrixVarietyPairViewModel)));
			SimMatrixGrid.ItemsSource = source;

			SimMatrixGrid.Columns.Clear();
			var headerColumn = new Column {FieldName = "Variety", Title = ""};
			DataGridControlBehaviors.SetIsRowHeader(headerColumn, true);
			SimMatrixGrid.Columns.Add(headerColumn);
			headerColumn.SetWidthToFit<SimilarityMatrixVarietyViewModel>(v => v.Name, 18);
			foreach (SimilarityMatrixVarietyViewModel variety in vm.Varieties)
				SimMatrixGrid.Columns.Add(new Column {FieldName = variety.Name, Title = variety.Name, Width = 32});
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
