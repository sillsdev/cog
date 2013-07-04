using System;
using System.ComponentModel;
using System.Windows.Controls;
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

		private void CorrespondenceDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
		{
			var vm = (VarietyPairViewModel) DataContext;
			using (vm.SoundChangesView.DeferRefresh())
			{
				vm.SoundChangesView.SortDescriptions.Clear();
				vm.SoundChangesView.SortDescriptions.Add(new SortDescription("Lhs.Target", ListSortDirection.Ascending));
				vm.SoundChangesView.SortDescriptions.Add(new SortDescription("Lhs.Environment", ListSortDirection.Ascending));

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
				vm.SoundChangesView.SortDescriptions.Add(new SortDescription(path, direction));
			}
			e.Handled = true;
		}

		private void CorrespondenceDataGrid_OnTargetUpdated(object sender, DataTransferEventArgs e)
		{
			if (e.Property == ItemsControl.ItemsSourceProperty)
			{
				var vm = (VarietyPairViewModel) DataContext;
				if (vm != null)
				{
					using (vm.SoundChangesView.DeferRefresh())
					{
						vm.SoundChangesView.SortDescriptions.Clear();
						vm.SoundChangesView.SortDescriptions.Add(new SortDescription("Lhs.Target", ListSortDirection.Ascending));
						vm.SoundChangesView.SortDescriptions.Add(new SortDescription("Lhs.Environment", ListSortDirection.Ascending));
						vm.SoundChangesView.SortDescriptions.Add(new SortDescription("Probability", ListSortDirection.Descending));
					}
					CorrespondenceDataGrid.Columns[1].SortDirection = ListSortDirection.Descending;
					Dispatcher.BeginInvoke(new Action(() => CorrespondenceDataGrid.UnselectAll()));
				}
			}
		}
	}
}
