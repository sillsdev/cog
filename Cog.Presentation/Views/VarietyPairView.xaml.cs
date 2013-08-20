using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.DataGrid;

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

		private void CorrespondenceDataGrid_OnTargetUpdated(object sender, DataTransferEventArgs e)
		{
			if (e.Property == ItemsControl.ItemsSourceProperty)
			{
				var view = CorrespondenceDataGrid.ItemsSource as DataGridCollectionView;
				if (view != null)
				{
					using (view.DeferRefresh())
					{
						view.SortDescriptions.Clear();
						view.SortDescriptions.Add(new SortDescription("Lhs", ListSortDirection.Ascending));
						view.SortDescriptions.Add(new SortDescription("Probability", ListSortDirection.Descending));
					}
					((INotifyCollectionChanged) view.SortDescriptions).CollectionChanged += OnSortChanged;
					Dispatcher.BeginInvoke(new Action(() =>
						{
							CorrespondenceDataGrid.CurrentItem = null;
							CorrespondenceDataGrid.SelectedItem = null;
						}));
				}
			}
		}

		private void OnSortChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				var sortDescs = (SortDescriptionCollection) sender;
				sortDescs.Add(new SortDescription("Lhs", ListSortDirection.Ascending));
			}
		}
	}
}
