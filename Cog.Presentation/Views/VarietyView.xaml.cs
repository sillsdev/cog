using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for VarietyView.xaml
	/// </summary>
	public partial class VarietyView
	{
		public VarietyView()
		{
			InitializeComponent();
		}

		private void SegmentsDataGrid_OnTargetUpdated(object sender, DataTransferEventArgs e)
		{
			if (e.Property == ItemsControl.ItemsSourceProperty)
			{
				ICollectionView segmentsView = CollectionViewSource.GetDefaultView(SegmentsDataGrid.Items);
				using (segmentsView.DeferRefresh())
				{
					segmentsView.SortDescriptions.Clear();
					segmentsView.SortDescriptions.Add(new SortDescription("Probability", ListSortDirection.Descending));
				}
				SegmentsDataGrid.Columns[1].SortDirection = ListSortDirection.Descending;
				Dispatcher.BeginInvoke(new Action(() => SegmentsDataGrid.UnselectAll()));
			}
		}
	}
}
