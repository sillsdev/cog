using System;
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
				Dispatcher.BeginInvoke(new Action(() => SegmentsDataGrid.SelectedItem = null));
		}
	}
}
