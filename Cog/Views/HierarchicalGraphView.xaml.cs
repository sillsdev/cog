using System;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for HierarchicalGraphView.xaml
	/// </summary>
	public partial class HierarchicalGraphView
	{
		public HierarchicalGraphView()
		{
			InitializeComponent();
		}

		private void _graphLayout_OnLayoutFinished(object sender, EventArgs e)
		{
			_zoomControl.ZoomToFill();
		}
	}
}
