using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for VarietyPairView.xaml
	/// </summary>
	public partial class VarietyPairView
	{
		public VarietyPairView()
		{
			InitializeComponent();
		}

		private void VarietyPairView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = (VarietyPairViewModel) DataContext;
			var source = new ListCollectionView(vm.Correspondences);
			source.GroupDescriptions.Add(new PropertyGroupDescription("Lhs"));
			_correspondenceDataGrid.ItemsSource = source;
			source.SortDescriptions.Add(new SortDescription("Lhs", ListSortDirection.Ascending));
			source.SortDescriptions.Add(new SortDescription("Probability", ListSortDirection.Descending));
			source.Refresh();
			_correspondenceDataGrid.SelectedIndex = 0;
		}
	}
}
