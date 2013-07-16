using System.Windows;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for ExportNetworkGraphDialog.xaml
	/// </summary>
	public partial class ExportNetworkGraphDialog
	{
		public ExportNetworkGraphDialog()
		{
			InitializeComponent();
		}

		private void exportButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
