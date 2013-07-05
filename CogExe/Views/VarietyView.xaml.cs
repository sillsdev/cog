using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
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
				var vm = (VarietiesVarietyViewModel) DataContext;
				if (vm != null)
				{
					using (vm.SegmentsView.DeferRefresh())
					{
						vm.SegmentsView.SortDescriptions.Clear();
						vm.SegmentsView.SortDescriptions.Add(new SortDescription("Probability", ListSortDirection.Descending));
					}
					SegmentsDataGrid.Columns[1].SortDirection = ListSortDirection.Descending;
					Dispatcher.BeginInvoke(new Action(() => SegmentsDataGrid.UnselectAll()));
				}
			}
		}
	}
}
