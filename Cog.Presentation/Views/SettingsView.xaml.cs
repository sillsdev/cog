using System;
using System.Windows;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for SettingsView.xaml
	/// </summary>
	public partial class SettingsView
	{
		public SettingsView()
		{
			InitializeComponent();
			BusyCursor.DisplayUntilIdle();
		}


		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (IsVisible)
				Dispatcher.BeginInvoke(new Action(() => ComponentSettingsControl.Focus()));
		}
	}
}
