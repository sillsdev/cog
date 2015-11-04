using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SIL.Cog.Presentation.Controls;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for AlineView.xaml
	/// </summary>
	public partial class AlineView
	{
		public AlineView()
		{
			InitializeComponent();
		}

		private void TreeListView_Loaded(object sender, RoutedEventArgs e)
		{
			var treeListView = (TreeListView) sender;
			double w = treeListView.ActualWidth;
			ScrollViewer sv = treeListView.FindVisualDescendants<ScrollViewer>().First();
			if (sv.ComputedVerticalScrollBarVisibility == Visibility.Visible)
				w -= SystemParameters.VerticalScrollBarWidth;
			double total = 0;
			for (int i = 1; i < treeListView.Columns.Count; i++)
				total += treeListView.Columns[i].ActualWidth;
			treeListView.Columns[0].Width = w - total;
		}
	}
}
