using System;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for HierarchicalGraphView.xaml
	/// </summary>
	public partial class HierarchicalGraphView
	{
		public HierarchicalGraphView()
		{
			InitializeComponent();
			BusyCursor.DisplayUntilIdle();
		}

		private void _graphLayout_OnLayoutFinished(object sender, EventArgs e)
		{
			ZoomControl.ZoomToFill();
		}
	}
}
