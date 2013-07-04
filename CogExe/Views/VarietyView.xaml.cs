using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
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

		private void MarkerClicked(object sender, MouseButtonEventArgs e)
		{
			var rect = (Rectangle) sender;
			var word = (WordViewModel) rect.DataContext;

			var cp = (ContentPresenter) WordsControl.ItemContainerGenerator.ContainerFromItem(word);
			var point = cp.TransformToAncestor(WordsControl).Transform(new Point());
			ScrollViewer.ScrollToVerticalOffset((point.Y + (cp.ActualHeight / 2)) - (ScrollViewer.ActualHeight / 2));
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
