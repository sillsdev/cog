using System.Windows;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for ExportSimilarityMatrixDialog.xaml
	/// </summary>
	public partial class ExportSimilarityMatrixDialog
	{
		public ExportSimilarityMatrixDialog()
		{
			InitializeComponent();
		}

		private void exportButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
