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

		private void GraphLayout_OnLayoutFinished(object sender, EventArgs e)
		{
			ZoomControl.ZoomToFill();
		}
	}
}
