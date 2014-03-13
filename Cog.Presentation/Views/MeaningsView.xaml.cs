using System;
using System.Windows;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for MeaningsView.xaml
	/// </summary>
	public partial class MeaningsView
	{
		public MeaningsView()
		{
			InitializeComponent();
			MeaningsGrid.ClipboardExporters.Clear();
			BusyCursor.DisplayUntilIdle();
		}

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (IsVisible)
				Dispatcher.BeginInvoke(new Action(() => MeaningsGrid.Focus()));
		}
	}
}
