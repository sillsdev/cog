using System;
using System.Windows;

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

		private void GraphLayout_OnLayoutFinished(object sender, EventArgs e)
		{
			ZoomControl.ZoomToFill();
		}

	    private void GraphLayout_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	    {
            if (GraphLayout.IsVisible)
                ZoomControl.ZoomToFill();
	    }

	    private void DendrogramLayout_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	    {
            if (DendrogramLayout.IsVisible)
                ZoomControl.ZoomToFill();
	    }
	}
}
