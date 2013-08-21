using System;
using System.Collections.Specialized;
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
			((INotifyCollectionChanged) CorrespondenceDataGrid.Items.SortDescriptions).CollectionChanged += OnSortChanged;
		}

		private void CorrespondenceDataGrid_OnTargetUpdated(object sender, DataTransferEventArgs e)
		{
			if (e.Property == ItemsControl.ItemsSourceProperty)
				Dispatcher.BeginInvoke(new Action(() => CorrespondenceDataGrid.SelectedItem = null));
		}

		private void OnSortChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var sortDescs = (SortDescriptionCollection) sender;
			if (e.Action == NotifyCollectionChangedAction.Reset && sortDescs.Count == 0)
				sortDescs.Add(new SortDescription("Lhs", ListSortDirection.Ascending));
		}
	}
}
