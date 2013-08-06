using System.Windows;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for ExportGlobalCorrespondencesChartDialog.xaml
	/// </summary>
	public partial class ExportGlobalCorrespondencesChartDialog
	{
		public ExportGlobalCorrespondencesChartDialog()
		{
			InitializeComponent();
		}

		private void exportButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
