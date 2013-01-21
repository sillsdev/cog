using System;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for NetworkGraphView.xaml
	/// </summary>
	public partial class NetworkGraphView
	{
		public NetworkGraphView()
		{
			InitializeComponent();
		}

		private void _graphLayout_OnLayoutFinished(object sender, EventArgs e)
		{
			_zoomControl.ZoomToFill();
		}
	}
}
