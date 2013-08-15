using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace SIL.Cog.Presentation.Views
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

		private void CorrespondenceDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
		{
			ICollectionView soundChangesView = CollectionViewSource.GetDefaultView(CorrespondenceDataGrid.Items);
			using (soundChangesView.DeferRefresh())
			{
				soundChangesView.SortDescriptions.Clear();
				soundChangesView.SortDescriptions.Add(new SortDescription("Lhs.Target", ListSortDirection.Ascending));
				soundChangesView.SortDescriptions.Add(new SortDescription("Lhs.Environment", ListSortDirection.Ascending));

				ListSortDirection direction = e.Column.SortDirection != ListSortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;

				e.Column.SortDirection = direction;
				string path = null;
				switch ((string) e.Column.Header)
				{
					case "Segment":
						path = "Correspondence";
						break;
					case "Probability":
						path = "Probability";
						break;
					case "Frequency":
						path = "Frequency";
						break;
				}
				soundChangesView.SortDescriptions.Add(new SortDescription(path, direction));
			}
			e.Handled = true;
		}

		private void CorrespondenceDataGrid_OnTargetUpdated(object sender, DataTransferEventArgs e)
		{
			if (e.Property == ItemsControl.ItemsSourceProperty)
			{
				ICollectionView soundChangesView = CollectionViewSource.GetDefaultView(CorrespondenceDataGrid.Items);
				using (soundChangesView.DeferRefresh())
				{
					if (soundChangesView.GroupDescriptions.Count == 0)
						soundChangesView.GroupDescriptions.Add(new PropertyGroupDescription("Lhs"));

					soundChangesView.SortDescriptions.Clear();
					soundChangesView.SortDescriptions.Add(new SortDescription("Lhs.Target", ListSortDirection.Ascending));
					soundChangesView.SortDescriptions.Add(new SortDescription("Lhs.Environment", ListSortDirection.Ascending));
					soundChangesView.SortDescriptions.Add(new SortDescription("Probability", ListSortDirection.Descending));
				}
				CorrespondenceDataGrid.Columns[1].SortDirection = ListSortDirection.Descending;
				Dispatcher.BeginInvoke(new Action(() => CorrespondenceDataGrid.UnselectAll()));
			}
		}
	}
}
