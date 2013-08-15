using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Applications.ViewModels;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Views;

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
			SimMatrixGrid.Columns.CollectionChanged += Columns_CollectionChanged;
			BusyCursor.DisplayUntilIdle();
		}

		private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (Column c in e.NewItems)
					c.Width = 30;
			}
		}

		private void SimilarityMatrixView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = e.NewValue as SimilarityMatrixViewModel;
			if (vm != null)
			{
				LoadCollectionView();
				SizeRowSelectorPaneToFit();
				vm.PropertyChanged += ViewModel_PropertyChanged;
			}
		}

		private void LoadCollectionView()
		{
			var vm = (SimilarityMatrixViewModel) DataContext;
			var source = (DataGridCollectionViewSource) Resources["VarietiesSource"];
			using (source.DeferRefresh())
			{
				source.ItemProperties.Clear();
				for (int i = 0; i < vm.Varieties.Count; i++)
					source.ItemProperties.Add(new DataGridItemProperty(vm.Varieties[i].Name, string.Format("VarietyPairs[{0}]", i), typeof (SimilarityMatrixVarietyPairViewModel)) {Title = vm.Varieties[i].Name});
			}
		}

		private void SizeRowSelectorPaneToFit()
		{
			var vm = (SimilarityMatrixViewModel) DataContext;
			if (vm == null)
				return;

			var textBrush = (Brush) Application.Current.FindResource("HeaderTextBrush");
			double maxWidth = 0;
			foreach (SimilarityMatrixVarietyViewModel variety in vm.Varieties)
			{
				var formattedText = new FormattedText(variety.Name, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
					new Typeface(SimMatrixGrid.FontFamily, SimMatrixGrid.FontStyle, SimMatrixGrid.FontWeight, SimMatrixGrid.FontStretch), SimMatrixGrid.FontSize, textBrush);
				if (formattedText.Width > maxWidth)
					maxWidth = formattedText.Width;
			}

			var tableView = (TableView) SimMatrixGrid.View;
			tableView.RowSelectorPaneWidth = maxWidth + 18;
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Varieties":
					DispatcherHelper.CheckBeginInvokeOnUI(() =>
						{
							LoadCollectionView();
							SizeRowSelectorPaneToFit();
						});
					break;
			}
		}
	}
}
