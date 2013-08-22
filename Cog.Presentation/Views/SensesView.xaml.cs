using System;
using System.Windows;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for SensesView.xaml
	/// </summary>
	public partial class SensesView
	{
		public SensesView()
		{
			InitializeComponent();
			SensesGrid.ClipboardExporters.Clear();
			BusyCursor.DisplayUntilIdle();
		}

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (IsVisible)
				Dispatcher.BeginInvoke(new Action(() => SensesGrid.Focus()));
		}
	}
}
